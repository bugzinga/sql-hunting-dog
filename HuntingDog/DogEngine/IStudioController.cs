
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
    public interface IStudioController
    {
        event Action ShowYourself;

        // fire when new server is connected/disconnected
        event Action<List<IServer>> OnServersAdded;
        event Action<List<IServer>> OnServersRemoved;

        // Search
        List<Entity> Find(IServer serverName, String databaseName, String searchText);

        void Initialise();

        void SetConfiguration(HuntingDog.Config.DogConfig cfg);

        List<IServer> ListServers();

        List<String> ListDatabase(IServer serverName);

        void RefreshServer(IServer serverName);

        void RefreshDatabase(IServer serverName, String databaseName);

        // columns, pro parameters
        List<TableColumn> ListColumns(Entity entityObject);
        List<TableColumn> ListViewColumns(Entity entityObject);
        List<ProcedureParameter> ListProcParameters(Entity entityObject);
        List<FunctionParameter> ListFuncParameters(Entity entityObject);

        // change script
        void ModifyFunction(IServer server, Entity entityObject);
        void ModifyView(IServer server, Entity entityObject);
        void ModifyProcedure(IServer server, Entity entityObject);

        // select * from script
        void ScriptTable(IServer server, Entity entityObject);
        void SelectFromTable(IServer server, Entity entityObject);
        void SelectFromView(IServer server, Entity entityObject);

        // execute sp script
        void ExecuteProcedure(IServer server, Entity entityObject);
        void ExecuteFunction(IServer server, Entity entityObject);

        // open Edit table window/ design table window
        void EditTableData(IServer server, Entity entityObject);
        void DesignTable(IServer server, Entity entityObject);

        // navigate object in object explorer
        void NavigateObject(IServer server, Entity entityObject);
    }
}
