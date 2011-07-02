using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }

    public class ProcedureParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Direction { get; set; }
    }

    public class TableColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
    }

    public interface IStudioController
    {
        void ConnectNewServer();

        event Action ShowYourself;

        // Search
        List<Entity> Find(string serverName, string databaseName, string searchText);

        // fire when new server is connected/disconected
        void Initialise();
        event Action OnServersChanged;
        List<string> ListServers();

        List<string> ListDatabase(string serverName);

        void RefreshServer(string serverName);

        void RefreshDatabase(string serverName,string databaseName);

        // columns, pro parameters
        List<TableColumn> ListColumns(string name);
        List<ProcedureParameter> ListProcParameters(string name);

        // dependencies
        List<Entity> GetInvokedBy(string name);
        List<Entity> GetInvokes(string name);

        // change script
        void ModifyFunction(string server, Entity entityObject);
        void ModifyView(string server, Entity entityObject);
        void ModifyProcedure(string server, Entity entityObject);
      
        // select * from script
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
