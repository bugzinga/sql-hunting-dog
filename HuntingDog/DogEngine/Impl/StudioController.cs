
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DatabaseObjectSearcher;
using EnvDTE;
using EnvDTE80;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;



namespace HuntingDog.DogEngine.Impl
{
    public sealed class StudioController : IStudioController
    {
        public event Action ShowYourself;

        public event Action<List<IServer>> OnServersAdded;

        public event Action<List<IServer>> OnServersRemoved;

        private readonly Log log = LogFactory.GetLog();


        private ObjectExplorerManager manager ;

        private Int32 searchLimit = 10000;

        IServerWatcher _srvWatcher;

        public StudioController(ObjectExplorerManager mgr,IServerWatcher watcher)
        {
            manager = mgr;
            _srvWatcher = watcher;
            _srvWatcher.OnServersAdded += _srvWatcher_OnServersAdded;
            _srvWatcher.OnServersRemoved += _srvWatcher_OnServersRemoved;

            Servers = new Dictionary<IServer, DatabaseLoader>();
        }

        void IStudioController.Initialise()
        {
           
        }


        void _srvWatcher_OnServersRemoved(List<IServerWithConnection> removedServers)
        {

            foreach (var removedServer in removedServers)
            {
                if (Servers.ContainsKey(removedServer))
                {
                    Servers.Remove(removedServer);
                }               
            }

            if (Servers.Count == 0)
            {
                GC.Collect();
            }

            OnServersRemoved(removedServers.Cast<IServer>().ToList());
        }

        void _srvWatcher_OnServersAdded(List<IServerWithConnection> addedServers)
        {

            foreach (var addedServer in addedServers)
            {
                var nvServer = new DatabaseLoader();
                nvServer.Initialise(addedServer);
                Servers.Add(addedServer, nvServer);
            }
        

            OnServersAdded(addedServers.Cast<IServer>().ToList());

        }
    
   
        Dictionary<IServer, DatabaseLoader> Servers
        {
            get;
            set;
        }

    

     

        List<Entity> IStudioController.Find(IServer serverName, String databaseName, String searchText)
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

        void IStudioController.NavigateObject(IServer server, Entity entityObject)
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



        List<IServer> IStudioController.ListServers()
        {
            var res = new List<IServer>();
            foreach(var srvKey in Servers.Keys)
            {
                res.Add(srvKey);
            }

            return res;
        }

        public List<String> ListDatabase(IServer serverName)
        {

            if (Servers.ContainsKey(serverName))
            {
                return Servers[serverName].DatabaseList;
            }
            else
            {
                log.Error("Requested unknown server " + serverName.ID);

                foreach (var srv in Servers)
                {
                    log.Error("Available server: " + srv.Key);
                }

                return new List<String>();
            }
        }

        void IStudioController.RefreshServer(IServer serverName)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(serverName);
                serverInfo.RefreshDatabaseList();

            }, "Refreshing database list failed - " + serverName.ServerName );

        }

        void IStudioController.RefreshDatabase(IServer serverName, String dbName)
        {

            this.SafeRun(() =>
            {
                var serverInfo = GetServer(serverName);
                serverInfo.RefreshDatabase(dbName);

            }, "Refreshing database failed - " + serverName.ServerName + " "+ dbName);

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

     
        public void ForceShowYourself()
        {
            if (ShowYourself != null)
            {
                ShowYourself();
            }
        }

         public void ModifyFunction(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.OpenFunctionForModification(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }, "ModifyFunction failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyView(IServer server , Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.ModifyView(entityObject.InternalObject as View, serverInfo.Connection);
            }, "ModifyView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ModifyProcedure(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.OpenStoredProcedureForModification(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }, "ModifyProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromView(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.SelectFromView(entityObject.InternalObject as View, serverInfo.Connection);

            }, "SelectFromView failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteProcedure(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.ExecuteStoredProc(entityObject.InternalObject as StoredProcedure, serverInfo.Connection);
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        public void ExecuteFunction(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.ExecuteFunction(entityObject.InternalObject as UserDefinedFunction, serverInfo.Connection);
            }, "ExecuteProcedure failed - " + GetSafeEntityObject(entityObject));
        }

        private DatabaseLoader GetServer(IServer server)
        {
            return Servers[server];
        }

        public void ScriptTable(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.ScriptTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "ScriptTable - " + GetSafeEntityObject(entityObject));
        }

        public void SelectFromTable(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.SelectFromTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "SelectFromTable - " + GetSafeEntityObject(entityObject));
        }

        public void EditTableData(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                manager.OpenTable2(entityObject.InternalObject as Table, serverInfo.Connection, serverInfo.Server);
            }, "EditTableData - " + GetSafeEntityObject(entityObject));
        }

        public void DesignTable(IServer server, Entity entityObject)
        {
            this.SafeRun(() =>
            {
                var serverInfo = GetServer(server);
                ManagementStudioController.DesignTable(entityObject.InternalObject as Table, serverInfo.Connection);
            }, "DesignTable - " + GetSafeEntityObject(entityObject));
        }
    }
}
