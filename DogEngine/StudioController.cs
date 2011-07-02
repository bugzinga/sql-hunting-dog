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
using System.Text.RegularExpressions;
using DatabaseObjectSearcher;
using System.Linq;

namespace HuntingDog.DogEngine
{

    public class StudioController : IStudioController
    {

        private EnvDTE.Window toolWindow;
        DatabaseObjectSearcher.ObjectExplorerManager manager = new DatabaseObjectSearcher.ObjectExplorerManager();
        public Dictionary<string, NavigatorServer> Servers { get; private set; }

        public int SearchLimit = 2000;

        private StudioController()
        {
        }

        static StudioController currentInstance = new StudioController();
        public static StudioController Current
        {
            get { return currentInstance; }
        }

        public event Action OnServersChanged;


        List<Entity> IStudioController.Find(string serverName, string databaseName, string searchText)
        {
            var server = Servers[serverName];
            var listFound = server.DbSearcher.Find(searchText, databaseName, SearchLimit);

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
            manager.SelectSMOObjectInObjectExplorer(entityObject.InternalObject as ScriptSchemaObjectBase, srv.ConnInfo);
        }

        void IStudioController.Initialise()
        {
            Servers = new Dictionary<string,NavigatorServer>();

            manager.Init();
            manager.OnNewServerConnected += manager_OnNewServerConnected;
            manager.OnServerDisconnected += manager_OnServerDisconnected;


           ReloadServerList();

          
        }

        private void  ReloadServerList()
        {
            // read all servers
            foreach (var srvConnectionInfo in manager.GetAllServers())
            {
                try
                {
                    var nvServer = new NavigatorServer(srvConnectionInfo, srvConnectionInfo.ServerName);
                    Servers.Add(srvConnectionInfo.ServerName, nvServer);
                }
                catch (Exception ex)
                {
                    // NEED TO LOG: FATAL ERROR:
                }
            }
          
        }

        void manager_OnServerDisconnected()
        {
            ReloadServerList();
            if(OnServersChanged!=null)
                 OnServersChanged();
        }

        void manager_OnNewServerConnected(string serverName)
        {
            if (!Servers.ContainsKey(serverName))
            {
                ReloadServerList();
                if (OnServersChanged != null)
                    OnServersChanged();
            }
        }

        List<string> IStudioController.ListServers()
        {
            return Servers.Keys.ToList();
        }

        public List<string> ListDatabase(string serverName)
        {
            var server = Servers[serverName];
            return server.DbSearcher.GetAvailableDataBases();
        }

        void IStudioController.RefreshServer(string serverName)
        {
            var server = Servers[serverName];
            server.DbSearcher.BuilDataBaseDictionary();


             RefreshDatabase(serverName,null);
 
        }

     

        public void RefreshDatabase(string serverName,string dbNameIsNotUserHere)
        {
            var server = Servers[serverName];
            server.DbSearcher.BuildDBObjectDictionary();
        }

        List<TableColumn> IStudioController.ListColumns(string name)
        {
            throw new NotImplementedException();
        }

        List<ProcedureParameter> IStudioController.ListProcParameters(string name)
        {
            throw new NotImplementedException();
        }

        List<Entity> IStudioController.GetInvokedBy(string name)
        {
            throw new NotImplementedException();
        }

        List<Entity> IStudioController.GetInvokes(string name)
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
            ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction,serverInfo.ConnInfo);
        }

        public void ModifyView(string server, Entity entityObject)
        {
            //var serverInfo = Servers[server];
            //ManagementStudioController.OpenView(entityObject.InternalObject as View, serverInfo.ConnInfo);
        }

        public void ModifyProcedure(string server, Entity entityObject)
        {
               var serverInfo = Servers[server];
               ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.ConnInfo);
        }

      

        public void SelectFromView(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.ConnInfo); 
        }

        public void ExecuteProcedure(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.ConnInfo);
        }

        public void ExecuteFunction(string server, Entity entityObject)
        {
            //var serverInfo = Servers[server];
            //ManagementStudioController.e(entityObject.InternalObject as StoredProcedure, serverInfo.ConnInfo);
        }

        public void SelectFromTable(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.ConnInfo);
        }

        public void EditTableData(string server, Entity entityObject)
        {
            var serverInfo = Servers[server];
            manager.OpenTable(entityObject.InternalObject as Table, serverInfo.ConnInfo);
        }

        public void DesignTable(string server, Entity entityObject)
        {
               var serverInfo = Servers[server];
               ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.ConnInfo);
        }

        public event Action ShowYourself;

        public void ConnectNewServer()
        {
            ManagementStudioController.ConnectNew();
            
        }
    }
}
