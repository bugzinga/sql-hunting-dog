
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DatabaseObjectSearcher;
using EnvDTE;
using EnvDTE80;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;

using Thread = System.Threading.Thread;

namespace HuntingDog.DogEngine.Impl
{
    sealed class StudioController : IStudioController
    {
        public event Action ShowYourself;

        public event Action<List<String>> OnServersAdded;

        public event Action<List<String>> OnServersRemoved;

        private readonly Log log = LogFactory.GetLog();

        private static readonly String windowId = "{7146B360-D37D-44A1-8D4C-5E7E36EA81D4}";

        private AddIn addIn;

        private ObjectExplorerManager manager = new ObjectExplorerManager();

        private Int32 searchLimit = 10000;

        private Thread serverCheck;

        private AutoResetEvent stopThread = new AutoResetEvent(false);

        public static StudioController Instance
        {
            get;
            private set;
        }

        public Dictionary<String, DatabaseLoader> Servers
        {
            get;
            private set;
        }

        public EnvDTE.Window SearchWindow
        {
            get;
            private set;
        }

        static StudioController()
        {
            Instance = new StudioController();
        }

        private StudioController()
        {
        }

        List<Entity> IStudioController.Find(String serverName, String databaseName, String searchText)
        {
            var server = Servers[serverName];
            var keywords = new List<String>();
            var listFound = server.Find(searchText, databaseName, searchLimit, keywords);
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
                e.Keywords = keywords;
                result.Add(e);
            }

            return result;
        }

        void IStudioController.NavigateObject(String server, Entity entityObject)
        {
            try
            {
                var srv = this.Servers[server];
                manager.SelectSMOObjectInObjectExplorer(entityObject.InternalObject as ScriptSchemaObjectBase, srv.Connection);
            }
            catch (Exception ex)
            {
                log.Error("Error locating object", ex);
            }
        }

        void IStudioController.Initialise()
        {
            Servers = new Dictionary<String, DatabaseLoader>();

            manager.Init();
            manager.OnNewServerConnected += manager_OnNewServerConnected;

            ReloadServerList();

            // run thread - this thread will check connected servers and will report if some servers will be disconnected.
            serverCheck = new System.Threading.Thread(BackgroundThreadCheckServer);
            serverCheck.Start();
        }

        private void BackgroundThreadCheckServer()
        {
            List<SqlConnectionInfo> oldList = manager.GetAllServers();

            while (true)
            {
                if (stopThread.WaitOne(1 * 1000))
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
                            var removed = oldList.Where(x => !newList.Any(y => (y.ConnectionString == x.ConnectionString))).ToList();
                            var added = newList.Where(x => !oldList.Any(y => (y.ConnectionString == x.ConnectionString))).ToList();

                            if (removed.Any() || added.Any())
                            {
                                if (removed.Any())
                                {
                                    log.Message("Found " + removed.Count.ToString() + " disconnected server");
                                }

                                if (removed.Any())
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

                                if (Servers.IsEmpty())
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
                    log.Error("Thread server checker", e);
                }
                
                if (stopThread.WaitOne(4 * 1000))
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
                    var srvName = srvConnectionInfo.ServerName;

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
                    log.Error("Error reloading server list", ex);
                }
            }
        }

        private DatabaseLoader GetServerByName(String name)
        {
            var lowerName = name.ToLower();
            var key = Servers.Keys.FirstOrDefault(x => (x.ToLower() == lowerName));

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

                // do not believe server name provided by Object Explorer - it can have different case
                var newServer = GetServerByName(serverName);

                if (OnServersAdded != null)
                {
                    OnServersAdded(new List<String>() { newServer.Name });
                }
            }
            else
            {
                log.Error("New server connected event (but already connected): " + serverName);
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
                log.Error("Requested unknown server " + serverName);

                foreach (var srv in Servers)
                {
                    log.Error("Available server: " + srv.Key);
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
                    log.Error("Unknown server name (refreshing server): " + serverName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Refreshing server failed (server: " + serverName + ")", ex);
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
                    log.Error("Unknown server name (refreshing database): " + serverName);
                }
            }
            catch (Exception ex)
            {
                log.Error("Refreshing database failed (server: " + serverName + ", database: " + dbName + ")", ex);
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
                log.Error("ListViewColumns failed: " + GetSafeEntityObject(entityObject), ex);
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
                log.Error("ListColumns failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        List<FunctionParameter> IStudioController.ListFuncParameters(Entity entityObject)
        {
            var result = new List<FunctionParameter>();

            try
            {
                var func = entityObject.InternalObject as UserDefinedFunction;
                func.Parameters.Refresh();

                foreach (UserDefinedFunctionParameter tc in func.Parameters)
                {
                    result.Add(new FunctionParameter()
                    {
                        Name = tc.Name,
                        Type = tc.DataType.Name
                    });
                }

            }
            catch (Exception ex)
            {
                log.Error("ListFuncParameters failed: " + GetSafeEntityObject(entityObject), ex);
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
                log.Error("ListProcParameters failed: " + GetSafeEntityObject(entityObject), ex);
            }

            return result;
        }

        public EnvDTE.Window CreateAddinWindow(AddIn addIn)
        {
            try
            {
                this.addIn = addIn;

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var className = typeof(HuntingDog.ucHost).FullName;
                var caption = "Hunting Dog (Ctrl+D)";
                Object userControl = null;

                var windows = ServiceCache.ExtensibilityModel.Windows as Windows2;

                if (windows != null)
                {
                    if ((SearchWindow == null) || (windows.Item(windowId) == null))
                    {
                        SearchWindow = windows.CreateToolWindow2(addIn, assemblyLocation, className, caption, windowId, ref userControl);
                        SearchWindow.SetTabPicture(HuntingDog.Properties.Resources.footprint.GetHbitmap());
                    }

                    SearchWindow.Visible = true;
                }

                return SearchWindow;
            }
            catch (Exception ex)
            {
                log.Error("AddIn window could not be created", ex);
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

         public void ModifyFunction(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }, "ModifyFunction failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyView(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ModifyView(entityObject.InternalObject as View, serverInfo.Connection);
            }, "ModifyView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyProcedure(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }, "ModifyProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromView(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.Connection);

            }, "SelectFromView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteProcedure(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteFunction(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ExecuteFunction(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void ScriptTable(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.ScriptTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "ScriptTable - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromTable(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "SelectFromTable - " + GetSafeEntityObject(entityObject));
        }

        public void EditTableData(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                manager.OpenTable2(entityObject.InternalObject as Table, serverInfo.Connection, serverInfo.Server);
            }, "EditTableData - " + GetSafeEntityObject(entityObject));
        }

        public void DesignTable(String server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = Servers[server];
                ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "DesignTable - " + GetSafeEntityObject(entityObject));
        }
    }
}
