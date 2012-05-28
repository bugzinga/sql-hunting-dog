using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Reflection;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Threading;
using DatabaseObjectSearcher;
using System.Linq;


namespace HuntingDog.DogEngine
{

    public class StudioController : IStudioController
    {

        private EnvDTE.Window toolWindow;
        DatabaseObjectSearcher.ObjectExplorerManager manager = new DatabaseObjectSearcher.ObjectExplorerManager();
        public Dictionary<string, DatabaseLoader> Servers { get; private set; }

        public int SearchLimit = 2000;

        private StudioController()
        {
        }

        static StudioController currentInstance = new StudioController();
        public static StudioController Current
        {
            get { return currentInstance; }
        }

        public event Action<List<string> > OnServersAdded;
        public event Action<List<string> > OnServersRemoved;

        List<Entity> IStudioController.Find(string serverName, string databaseName, string searchText)
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

        void IStudioController.NavigateObject(string server, Entity entityObject)
        {
            var bars = (Microsoft.VisualStudio.CommandBars.CommandBars)_inst.DTE.CommandBars;
            foreach (var b in bars)
            {
                int i = 1;
            }

            try
            {
                //var oControl = 
                //  bars["Query"].Controls.Add(MsoControl Type.msoControlButton,
                //  System.Reflection.Missing.Value,
                //  System.Reflection.Missing.Value,1,true);
                // Set the caption of the submenuitem
                //oControl.Caption = "Navigate In Object Explorer";
            }
            catch
            {
            }



            var srv = this.Servers[server];
            manager.SelectSMOObjectInObjectExplorer(entityObject.InternalObject as ScriptSchemaObjectBase, srv.Connection);
        }

        System.Threading.Thread _serverCheck;
         System.Threading.AutoResetEvent _stopThread = new AutoResetEvent(false);
        void IStudioController.Initialise()
        {

            Servers = new Dictionary<string,DatabaseLoader>();

            manager.Init();
            manager.OnNewServerConnected += manager_OnNewServerConnected;
     

           ReloadServerList();

            // run thread - this thread will check connected servers and will report if some servers will be disconnected.
            _serverCheck = new System.Threading.Thread(BackgroundThreadCheckServer);
            _serverCheck.Start();

        }

        
        private void BackgroundThreadCheckServer()
        {
            List<SqlConnectionInfo> oldList = manager.GetAllServers(); ;
            while(true)
            {

                if (_stopThread.WaitOne(5 * 1000) == true)
                    break;

                try
                {
                    lock (this)
                    {
                        var newList = manager.GetAllServers();
                        if (oldList != null)
                        {
                            var removed =
                                oldList.Where(x => !newList.Any(y => y.ConnectionString == x.ConnectionString)).ToList();

                            if(removed.Count>0)
                            {
                                MyLogger.LogMessage("Detected disconnected server");

                                var removedNameList = removed.Select(x => x.ServerName).ToList();
                                foreach (var removedServer in removedNameList)
                                {
                                    var srv = Servers[removedServer];
                                    srv.Dispose();
                                }

                                ReloadServerList();

                                OnServersRemoved(removedNameList);

                                if(Servers.Count==0)
                                {
                                    // no servers left. Clean after yourself
                                    GC.Collect();
                                }
                            }

                          

                        }

                        oldList = newList;
                    }
                }
                catch (Exception e)
                {
                    
                    MyLogger.LogError("Thread server checker ",e);
                }
                // check all servers
              
                
            }
        }

        private void  ReloadServerList()
        {
            Servers.Clear();
            // read all servers
            foreach (var srvConnectionInfo in manager.GetAllServers())
            {
                try
                {
                   
                    var nvServer = new DatabaseLoader();
                    nvServer.Initialise(srvConnectionInfo);
                    Servers.Add(srvConnectionInfo.ServerName, nvServer);
                }
                catch (Exception ex)
                {
                    // NEED TO LOG: FATAL ERROR:
                    MyLogger.LogError("Error reloading server list.",ex);
                }
            }
          
        }

     
        private DatabaseLoader GetServerByName(string name)
        {
            string lowerName = name.ToLower();

            string key =  Servers.Keys.FirstOrDefault(x => x.ToLower() == lowerName);

            return key!=null?Servers[key]:null;
        }

        void manager_OnNewServerConnected(string serverName)
        {


            if (GetServerByName(serverName)==null)
            {
                lock (this)
                {
                    ReloadServerList();
                }

                // do not beleive server name provided by Object Explorer - it can have different case
                var newServer = GetServerByName(serverName);
                if (OnServersAdded != null)
                    OnServersAdded(new List<string>() { newServer.Name });
            }
            else
            {
                MyLogger.LogError("Controller:new server connected event (but already connected):" + serverName);
            }
        }

        List<string> IStudioController.ListServers()
        {
            return Servers.Keys.ToList();
        }

        public List<string> ListDatabase(string serverName)
        {
            if (Servers.ContainsKey(serverName))
                return Servers[serverName].DatabaseList;
            else
            {
                MyLogger.LogError("Controller: requested unknown server " + serverName + ".");
                foreach (var srv in Servers)
                {
                    MyLogger.LogError("Controller:available server:" + srv.Key);
                }

                return new List<string>();
            }
        }

        void IStudioController.RefreshServer(string serverName)
        {
            if (Servers.ContainsKey(serverName))
            {
                var server = Servers[serverName];
                server.RefreshDatabaseList();     
            }
            else
            {
                MyLogger.LogError("Controller: Refresh server. Unknown server name " + serverName + ".");
            }
           
        }

        void IStudioController.RefreshDatabase(string serverName, string dbName)
        {
            if (Servers.ContainsKey(serverName))
            {
                var server = Servers[serverName];
                server.RefreshDatabase(dbName);
            }
            else
            {
                MyLogger.LogError("Controller: Refresh Database. Unknown server name " + serverName + ".");
            }
        }


        List<TableColumn> IStudioController.ListViewColumns(Entity entityObjecte)
        {
            var result = new List<TableColumn>();

            var view = entityObjecte.InternalObject as View;
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

            return result;
        }

        List<TableColumn> IStudioController.ListColumns(Entity entityObjecte)
        {
            var result = new List<TableColumn>();

            var table = entityObjecte.InternalObject as Table;
            table.Columns.Refresh();
            foreach (Column tc in table.Columns)
            {
                result.Add(new TableColumn(){ 
                    Name = tc.Name,
                    IsPrimaryKey = tc.InPrimaryKey, 
                    IsForeignKey = tc.IsForeignKey,
                    Nullable = tc.Nullable, 
                    Type = tc.DataType.Name} );
            }

            return result;
        }

        List<FuncParameter> IStudioController.ListFuncParameters(Entity entityObject)
        {
            var result = new List<FuncParameter>();

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

            return result;
        }

        List<ProcedureParameter> IStudioController.ListProcParameters(Entity entityObject)
        {
            var result = new List<ProcedureParameter>();

            var procedure = entityObject.InternalObject as StoredProcedure;
            procedure.Parameters.Refresh();
            foreach (StoredProcedureParameter tc in procedure.Parameters)
            {
                result.Add(new ProcedureParameter()
                {
                    Name = tc.Name,
                    IsOut = tc.IsOutputParameter,
                    DefaultValue  = tc.DefaultValue,
                    Type = tc.DataType.Name,
                });
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

 

        void IStudioController.GenerateCreateScript(string name)
        {
            throw new NotImplementedException();
        }

      

        public EnvDTE.Window SearchWindow
        {
            get { return toolWindow; }
        }

        public EnvDTE.Window CreateAddinWindow(AddIn addinInstance)
        {
            Assembly asm = Assembly.Load("HuntingDog");
            // Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b9"); // this guid doesn't seem to matter?
            //toolWindow = CreateToolWindow("DatabaseObjectSearcherUI.ucMainControl", asm.Location, id,  addinInstance);

            Guid id = new Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b8"); // this guid doesn't seem to matter?
            toolWindow = CreateToolWindow("HuntingDog.ucHost", asm.Location, id, addinInstance);
            return toolWindow;
        }

     

        public void ForceShowYourself()
        {
            if (ShowYourself != null)
                ShowYourself();
        }

        AddIn _inst; 
        private EnvDTE.Window CreateToolWindow(string typeName, string assemblyLocation, Guid uiTypeGuid, AddIn addinInstance)
        {
            Windows2 win2 = ServiceCache.ExtensibilityModel.Windows as Windows2;

            _inst = addinInstance;
            //Windows2 win2 = applicationObject.Windows as Windows2;
            if (win2 != null)
            {
                object controlObject = null;
                Assembly asm = Assembly.GetExecutingAssembly();
                EnvDTE.Window toolWindow = win2.CreateToolWindow2(addinInstance, assemblyLocation, typeName, "Hunting Dog", "{" + uiTypeGuid.ToString() + "}", ref controlObject);

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


        public void ModifyFunction(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
        }

        public void ModifyView(string server, Entity entityObject)
        {
            //var serverInfo = Servers[server];
            //ManagementStudioController.OpenView(entityObject.InternalObject as View, serverInfo.ConnInfo);
        }

        public void ModifyProcedure(string server, Entity entityObject)
        {
               var serverInfo = Servers[server];
               ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
        }

      

        public void SelectFromView(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.Connection); 
        }

        public void ExecuteProcedure(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
        }

        public void ExecuteFunction(string server, Entity entityObject)
        {
            //var serverInfo = Servers[server];
            //ManagementStudioController.e(entityObject.InternalObject as StoredProcedure, serverInfo.ConnInfo);
        }

        public void SelectFromTable(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.Connection);
        }

        public void EditTableData(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            manager.OpenTable(entityObject.InternalObject as Table, serverInfo.Connection);
        }

        public void DesignTable(string server, Entity entityObject)
        {
               var serverInfo = Servers[server];
               ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.Connection);
        }

        public event Action ShowYourself;

        public void ConnectNewServer()
        {
           
           
        }
    }
}
