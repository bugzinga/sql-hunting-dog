
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using DatabaseObjectSearcher;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace HuntingDog.DogEngine
{
    public sealed class StudioController : IStudioController
    {
        private static readonly Log log = LogFactory.GetLog(typeof(StudioController));

        private static StudioController currentInstance = new StudioController();

        private AddIn inst;

        private EnvDTE.Window toolWindow;

        private DatabaseObjectSearcher.ObjectExplorerManager manager = new DatabaseObjectSearcher.ObjectExplorerManager();

        public Int32 SearchLimit = 10000;

        public static StudioController Current
        {
            get
            {
                return currentInstance;
            }
        }

        public Dictionary<String, DatabaseLoader> Servers
        {
            get;
            private set;
        }

        public EnvDTE.Window SearchWindow
        {
            get
            {
                return toolWindow;
            }
        }

        private StudioController()
        {
        }

        public event Action ShowYourself;

        public event Action<List<String>> OnServersAdded;

        public event Action<List<String>> OnServersRemoved;

        List<Entity> IStudioController.Find(String serverName, String databaseName, String searchText)
        {
            var server = Servers[serverName];
            var listFound = server.Find(searchText, databaseName, SearchLimit);

            var result = new List<Entity>();

            foreach (var found in listFound)
            {
                var e = new Entity();
                e.Name = found.Name;
                e.IsFunction = found.IsFunction;
                e.IsProcedure = found.IsStoredProc;
                e.IsTable = found.IsTable;
                e.IsView = found.IsView;
                e.FullName = found.SchemaAndName;
                e.InternalObject = found.Result;
                result.Add(e);
            }

            return result;
        }

        void IStudioController.NavigateObject(String server, Entity entityObject)
        {
            var bars = (Microsoft.VisualStudio.CommandBars.CommandBars) inst.DTE.CommandBars;

            foreach (var b in bars)
            {
                // TODO: Commented to avoid a compilation warning.
                //       What was the purpose of this line?
                //Int32 i = 1;
            }

            try
            {
                var srv = this.Servers[server];
                manager.SelectSMOObjectInObjectExplorer(entityObject.InternalObject as ScriptSchemaObjectBase, srv.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Error locating object.", ex);
            }
        }

        System.Threading.Thread _serverCheck;
        System.Threading.AutoResetEvent _stopThread = new AutoResetEvent(false);

        void IStudioController.Initialise()
        {
            Servers = new Dictionary<String, DatabaseLoader>();

            manager.Init();
            manager.OnNewServerConnected += manager_OnNewServerConnected;

            ReloadServerList();

            // run thread - this thread will check connected servers and will report if some servers will be disconnected.
            _serverCheck = new System.Threading.Thread(BackgroundThreadCheckServer);
            _serverCheck.Start();
        }

        private void BackgroundThreadCheckServer()
        {
            List<SqlConnectionInfo> oldList = manager.GetAllServers();

            while (true)
            {
                if (_stopThread.WaitOne(1 * 1000))
                {
                    break;
                }

                try
                {
                    lock (this)
                    {
                        var newList = manager.GetAllServers();

                        if (oldList != null)
                        {
                            var removed = oldList.Where(x => !newList.Any(y => y.ConnectionString == x.ConnectionString)).ToList();
                            var added = newList.Where(x => !oldList.Any(y => y.ConnectionString == x.ConnectionString)).ToList();

                            if ((removed.Count > 0) || (added.Count > 0))
                            {
                                if (removed.Count > 0)
                                {
                                    log.Message("Found " + removed.Count.ToString() + " disconnected server");
                                }

                                if (removed.Count > 0)
                                {
                                    log.Message("Found " + added.Count.ToString() + " connected server");
                                }

                                var removedNameList = removed.Select(x => x.ServerName).ToList();

                                foreach (var removedServer in removedNameList)
                                {
                                    var srv = Servers[removedServer];
                                }

                                var addedNameList = added.Select(x => x.ServerName).ToList();

                                ReloadServerList();

                                OnServersRemoved(removedNameList);

                                if (Servers.Count == 0)
                                {
                                    // no servers left. Clean after yourself
                                    GC.Collect();
                                }

                                OnServersAdded(addedNameList);
                            }
                        }

                        oldList = newList;
                    }
                }
                catch (Exception e)
                {
                    log.Error("Thread server checker ", e);
                }
                
                // check all servers

                if (_stopThread.WaitOne(4 * 1000))
                {
                    break;
                }
            }
        }

        private void ReloadServerList()
        {
            // we need to preserve old servers (and previously cached objects) when new server is added
            var oldServers = new Dictionary<string, DatabaseLoader>();
            foreach (var srvKeyValue in Servers)
            {
                oldServers.Add(srvKeyValue.Key, srvKeyValue.Value);
            }

            Servers.Clear();

            // read all servers
            foreach (var srvConnectionInfo in manager.GetAllServers())
            {
                try
                {
                    String srvName = srvConnectionInfo.ServerName;

                    if (oldServers.ContainsKey(srvName))
                    {
                        Servers.Add(srvName, oldServers[srvName]);
                    }
                    else
                    {
                        var nvServer = new DatabaseLoader();
                        nvServer.Initialise(srvConnectionInfo);
                        Servers.Add(srvName, nvServer);
                    }
                }
                catch (Exception ex)
                {
                    // NEED TO LOG: FATAL ERROR:
                    log.Error("Error reloading server list.", ex);
                }
            }
        }

        private DatabaseLoader GetServerByName(String name)
        {
            String lowerName = name.ToLower();
            String key = Servers.Keys.FirstOrDefault(x => x.ToLower() == lowerName);

            return (key != null)
                ? Servers[key]
                : null;
        }

        void manager_OnNewServerConnected(String serverName)
        {
            if (GetServerByName(serverName) == null)
            {
                lock (this)
                {
                    ReloadServerList();
                }

                // do not beleive server name provided by Object Explorer - it can have different case
                var newServer = GetServerByName(serverName);
                if (OnServersAdded != null)
                {
                    OnServersAdded(new List<String>() { newServer.Name });
                }
            }
            else
            {
                log.Error("Controller:new server connected event (but already connected):" + serverName);
            }
        }

        List<String> IStudioController.ListServers()
        {
            return Servers.Keys.ToList();
        }

        public List<String> ListDatabase(String serverName)
        {
            if (Servers.ContainsKey(serverName))
            {
                return Servers[serverName].DatabaseList;
            }
            else
            {
                log.Error("Controller: requested unknown server " + serverName + ".");

                foreach (var srv in Servers)
                {
                    log.Error("Controller:available server:" + srv.Key);
                }

                return new List<String>();
            }
        }

        void IStudioController.RefreshServer(String serverName)
        {
            try
            {
                if (Servers.ContainsKey(serverName))
                {
                    var server = Servers[serverName];
                    server.RefreshDatabaseList();
                }
                else
                {
                    log.Error("Controller: Refresh server. Unknown server name " + serverName + ".");
                }
            }
            catch (Exception ex)
            {
                log.Error("Controller: RefreshServer", ex);
            }
        }

        void IStudioController.RefreshDatabase(String serverName, String dbName)
        {
            try
            {
                if (Servers.ContainsKey(serverName))
                {
                    var server = Servers[serverName];
                    server.RefreshDatabase(dbName);
                }
                else
                {
                    log.Error("Controller: Refresh Database. Unknown server name " + serverName + ".");
                }
            }
            catch (Exception ex)
            {
                log.Error("Controller: RefreshDatabase failed. Server:" + serverName + " database:" + dbName, ex);
            }
        }

        private String GetSafeEntityObject(Entity entityObject)
        {
            return (entityObject != null)
                ? entityObject.ToSafeString()
                : "NULL entityObject";
        }

        List<TableColumn> IStudioController.ListViewColumns(Entity entityObject)
        {
            var result = new List<TableColumn>();

            try
            {
                var view = entityObject.InternalObject as View;
                view.Columns.Refresh();

                foreach (Column tc in view.Columns)
                {
                    result.Add(new TableColumn()
                    {
                        Name = tc.Name,
                        IsPrimaryKey = tc.InPrimaryKey,
                        IsForeignKey = tc.IsForeignKey,
                        Nullable = tc.Nullable,
                        Type = tc.DataType.Name
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error("Controller: ListViewColumns failed. " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<TableColumn> IStudioController.ListColumns(Entity entityObject)
        {
            var result = new List<TableColumn>();

            try
            {
                var table = entityObject.InternalObject as Table;
                table.Columns.Refresh();

                foreach (Column tc in table.Columns)
                {
                    result.Add(new TableColumn()
                                   {
                                       Name = tc.Name,
                                       IsPrimaryKey = tc.InPrimaryKey,
                                       IsForeignKey = tc.IsForeignKey,
                                       Nullable = tc.Nullable,
                                       Type = tc.DataType.Name
                                   });
                }
            }
            catch (Exception ex)
            {
                log.Error("Controller: ListColumns failed. " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<FuncParameter> IStudioController.ListFuncParameters(Entity entityObject)
        {
            var result = new List<FuncParameter>();

            try
            {
                var func = entityObject.InternalObject as UserDefinedFunction;
                func.Parameters.Refresh();

                foreach (UserDefinedFunctionParameter tc in func.Parameters)
                {
                    result.Add(new FuncParameter()
                    {
                        Name = tc.Name,
                        Type = tc.DataType.Name
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("Controller: ListFuncParameters failed. " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<ProcedureParameter> IStudioController.ListProcParameters(Entity entityObject)
        {
            var result = new List<ProcedureParameter>();

            try
            {
                var procedure = entityObject.InternalObject as StoredProcedure;
                procedure.Parameters.Refresh();

                foreach (StoredProcedureParameter tc in procedure.Parameters)
                {
                    result.Add(new ProcedureParameter()
                    {
                        Name = tc.Name,
                        IsOut = tc.IsOutputParameter,
                        DefaultValue = tc.DefaultValue,
                        Type = tc.DataType.Name,
                    });
                }


            }
            catch (Exception ex)
            {
                log.Error("Controller: ListProcParameters failed. " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<Entity> IStudioController.GetInvokedBy(Entity entityObject)
        {
            throw new NotImplementedException();
        }

        List<Entity> IStudioController.GetInvokes(Entity entityObject)
        {
            throw new NotImplementedException();
        }

        void IStudioController.GenerateCreateScript(String name)
        {
            throw new NotImplementedException();
        }

        public EnvDTE.Window CreateAddinWindow(AddIn addinInstance)
        {
            try
            {
                Assembly asm = Assembly.Load("HuntingDog");

                // Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b9"); // this guid doesn't seem to matter?
                //toolWindow = CreateToolWindow("DatabaseObjectSearcherUI.ucMainControl", asm.Location, id,  addinInstance);

                Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b8"); // this guid doesn't seem to matter?
                toolWindow = CreateToolWindow("HuntingDog.ucHost", asm.Location, id, addinInstance);

                return toolWindow;
            }
            catch (Exception ex)
            {
                log.Error("Controller: CreateAddinWindow failed.", ex);
                throw;
            }
        }

        public void ForceShowYourself()
        {
            if (ShowYourself != null)
            {
                ShowYourself();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122")]
        private EnvDTE.Window CreateToolWindow(String typeName, String assemblyLocation, Guid uiTypeGuid, AddIn addinInstance)
        {
            Windows2 win2 = ServiceCache.ExtensibilityModel.Windows as Windows2;

            inst = addinInstance;

            //Windows2 win2 = applicationObject.Windows as Windows2;
            if (win2 != null)
            {
                Object controlObject = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                EnvDTE.Window toolWindow = win2.CreateToolWindow2(addinInstance, assemblyLocation, typeName, "Hunting Dog (Ctrl+D)", "{" + uiTypeGuid.ToString() + "}", ref controlObject);

                EnvDTE.Window oe = null;

                foreach (EnvDTE.Window w1 in addinInstance.DTE.Windows)
                {
                    if (w1.Caption == "Object Explorer")
                    {
                        oe = w1;
                    }
                }

                //toolWindow.Width = oe.Width;
                // toolWindow.SetKind((vsWindowType)oe.Kind);
                // toolWindow.IsFloating = oe.IsFloating;
                // oe.LinkedWindows.Add(toolWindow);
                //Window frame = win2.CreateLinkedWindowFrame(toolWindow,oe, vsLinkedWindowType.vsLinkedWindowTypeHorizontal);
                //frame.SetKind(vsWindowType.vsWindowTypeDocumentOutline);
                //addinInstance.DTE.MainWindow.LinkedWindows.Add(frame);
                //frame.Activate();

                toolWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                toolWindow.Visible = true;

                return toolWindow;
            }

            return null;
        }

        public void ModifyFunction(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: ModifyFunction failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void ModifyView(String server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.ModifyView(entityObject.InternalObject as View, serverInfo.Connection);
        }

        public void ModifyProcedure(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: ModifyProcedure failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void SelectFromView(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.Connection);

            }
            catch (Exception ex)
            {
                log.Error("Controller: SelectFromView failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void ExecuteProcedure(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: ExecuteProcedure failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void ExecuteFunction(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ExecuteFunction(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: ExecuteProcedure failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void ScriptTable(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ScriptTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: ScriptTable failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void SelectFromTable(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: SelectFromTable failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void EditTableData(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                manager.OpenTable2(entityObject.InternalObject as Table, serverInfo.Connection, serverInfo.Server);
            }
            catch (Exception ex)
            {
                log.Error("Controller: EditTableData failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void DesignTable(String server, Entity entityObject)
        {
            try
            {
                var serverInfo = Servers[server];
                ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Controller: DesignTable failed." + GetSafeEntityObject(entityObject), ex);
            }
        }

        public void ConnectNewServer()
        {
        }
    }
}
