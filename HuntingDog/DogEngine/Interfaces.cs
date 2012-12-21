
using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    public class Entity
    {
        public String FullName
        {
            get;
            set;
        }

        public String Name
        {
            get;
            set;
        }

        public Boolean IsProcedure
        {
            get;
            set;
        }

        public Boolean IsTable
        {
            get;
            set;
        }

        public Boolean IsFunction
        {
            get;
            set;
        }

        public Boolean IsView
        {
            get;
            set;
        }

        public Object InternalObject
        {
            get;
            set;
        }

        public String ToSafeString()
        {
            if (InternalObject == null)
            {
                return String.Format("Name:{0} but internal object is null.", FullName);
            }

            return FullName;
        }
    }

    public class FuncParameter
    {
        public String Name
        {
            get;
            set;
        }

        public String Type
        {
            get;
            set;
        }
    }

    public class ProcedureParameter
    {
        public String Name
        {
            get;
            set;
        }

        public String Type
        {
            get;
            set;
        }

        public String DefaultValue
        {
            get;
            set;
        }

        public Boolean IsOut
        {
            get;
            set;
        }
    }

    public class TableColumn
    {
        public String Name
        {
            get;
            set;
        }

        public String Type
        {
            get;
            set;
        }

        public Boolean Nullable
        {
            get;
            set;
        }

        public Boolean IsPrimaryKey
        {
            get;
            set;
        }

        public Boolean IsForeignKey
        {
            get;
            set;
        }
    }

    public interface IServerStorage
    {
        String Name
        {
            get;
        }

        List<IDatabaseDictionary> DatabaseList
        {
            get;
        }

        void Initialise(SqlConnectionInfo connectionInfo);

        void RefreshDatabaseList();
    }

    public interface IDatabaseLoader
    {
        SqlConnectionInfo Connection
        {
            get;
        }
        
        String Name
        {
            get;
        }

        List<String> DatabaseList
        {
            get;
        }

        void Initialise(SqlConnectionInfo connectionInfo);
        
        void RefreshDatabaseList();

        void RefreshDatabase(String name);
    }

    public interface IDatabaseDictionary
    {
        String DatabaseName
        {
            get;
        }

        Boolean IsLoaded
        {
            get;
        }

        List<DatabaseSearchResult> Find(String searchCriteria, Int32 limit);

        void Initialise(String databaseName);

        void Clear();

        void Add(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo);

        void MarkAsLoaded();
    }

    public interface IStudioController
    {
        void ConnectNewServer();

        event Action ShowYourself;

        // fire when new server is connected/disconnected
        event Action<List<String>> OnServersAdded;
        event Action<List<String>> OnServersRemoved;

        // Search
        List<Entity> Find(String serverName, String databaseName, String searchText);

        void Initialise();

        List<String> ListServers();

        List<String> ListDatabase(String serverName);

        void RefreshServer(String serverName);

        void RefreshDatabase(String serverName, String databaseName);

        // columns, pro parameters
        List<TableColumn> ListColumns(Entity entityObject);
        List<TableColumn> ListViewColumns(Entity entityObject);
        List<ProcedureParameter> ListProcParameters(Entity entityObject);
        List<FuncParameter> ListFuncParameters(Entity entityObject);

        // dependencies
        List<Entity> GetInvokedBy(Entity entityObjecte);
        List<Entity> GetInvokes(Entity entityObject);

        // change script
        void ModifyFunction(String server, Entity entityObject);
        void ModifyView(String server, Entity entityObject);
        void ModifyProcedure(String server, Entity entityObject);

        // select * from script
        void ScriptTable(String server, Entity entityObject);
        void SelectFromTable(String server, Entity entityObject);
        void SelectFromView(String server, Entity entityObject);

        // execute sp script
        void ExecuteProcedure(String server, Entity entityObject);
        void ExecuteFunction(String server, Entity entityObject);

        // open Edit table window/ design table window
        void EditTableData(String server, Entity entityObject);
        void DesignTable(String server, Entity entityObject);

        // generate new table/view - script
        void GenerateCreateScript(String name);

        // navigate object in object explorer
        void NavigateObject(String server, Entity entityObject);
    }
}
