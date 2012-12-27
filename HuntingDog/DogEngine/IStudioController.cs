
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
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
        List<FunctionParameter> ListFuncParameters(Entity entityObject);

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
