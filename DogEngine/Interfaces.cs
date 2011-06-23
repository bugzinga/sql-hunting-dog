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
        void ModifyFunction(string name);
        void ModifyView(string name);
        void ModifyProcedure(string name);
      
        // select * from script
        void SelectFromTable(string name);
        void SelectFromView(string name);

        // execute sp script
        void ExecuteProcedure(string name);
        void ExecuteFunction(string name);

        // open Edit table window/ design table window
        void EditTableData(string name);
        void DesignTable(string name);

        // generate new table/view - script
        void GenerateCreateScript(string name);

        // navigate object in object explorer
        void NavigateObject(string name);
    }


}
