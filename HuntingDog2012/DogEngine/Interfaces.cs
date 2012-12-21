using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    public class Entity
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsTable { get; set; }
        public bool IsFunction { get; set; }
        public bool IsView { get; set; }

        public object InternalObject { get; set; }

        public string ToSafeString()
        {
            if (InternalObject == null)
                return string.Format("Name:{0} but internal object is null.", FullName);

            return FullName;
        }
    }

    public class FuncParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class ProcedureParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public bool IsOut { get; set; }
    }

    public class TableColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
    }

    public interface IServerStorage
    {
        string Name { get; }
        void Initialise(SqlConnectionInfo connectionInfo);
        List<IDatabaseDictionary> DatabaseList { get; }
        void RefreshDatabaseList();
    }

    public interface IDatabaseLoader
    {
        SqlConnectionInfo Connection { get; }
        string Name { get; }
        void Initialise(SqlConnectionInfo connectionInfo);    
        List<string> DatabaseList { get; }
        void RefreshDatabaseList();
        void RefreshDatabase(string name);

    }

    public interface IDatabaseDictionary
    {
        List<DatabaseSearchResult> Find(string searchCriteria, int limit);
        void Initialise(string databaseName);
        string DatabaseName { get; }
        bool IsLoaded { get; }
        void Clear();
        void Add(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo);
        void MarkAsLoaded();
    }


    public interface IStudioController
    {
        void ConnectNewServer();

        event Action ShowYourself;

        // Search
        List<Entity> Find(string serverName, string databaseName, string searchText);

        // fire when new server is connected/disconected
        void Initialise();
        event Action<List<string> > OnServersAdded;
        event Action<List<string> > OnServersRemoved;

        List<string> ListServers();

        List<string> ListDatabase(string serverName);

        void RefreshServer(string serverName);

        void RefreshDatabase(string serverName,string databaseName);

        // columns, pro parameters
        List<TableColumn> ListColumns(Entity entityObject);
        List<TableColumn> ListViewColumns(Entity entityObject);
        List<ProcedureParameter> ListProcParameters(Entity entityObject);
        List<FuncParameter> ListFuncParameters(Entity entityObject);
  

        // dependencies
        List<Entity> GetInvokedBy(Entity entityObjecte);
        List<Entity> GetInvokes(Entity entityObject);

        // change script
        void ModifyFunction(string server, Entity entityObject);
        void ModifyView(string server, Entity entityObject);
        void ModifyProcedure(string server, Entity entityObject);
      
        // select * from script
        void ScriptTable(string server, Entity entityObject);
        void SelectFromTable(string server, Entity entityObject);
        void SelectFromView(string server, Entity entityObject);

        // execute sp script
        void ExecuteProcedure(string server, Entity entityObject);
        void ExecuteFunction(string server, Entity entityObject);

        // open Edit table window/ design table window
        void EditTableData(string server, Entity entityObject);
        void DesignTable(string server, Entity entityObject);

        // generate new table/view - script
        void GenerateCreateScript(string name);

        // navigate object in object explorer
        void NavigateObject(string server, Entity entityObject);
    }


}
