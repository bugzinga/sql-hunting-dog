
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Common;

namespace HuntingDog.DogEngine
{
    class ServerStorage : IServerStorage
    {
        public IDatabaseLoader Loader
        {
            get;
            set;
        }

        public SqlConnectionInfo Connection
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                return Connection.ServerName;
            }
        }

        public List<IDatabaseDictionary> DatabaseList
        {
            get;
            private set;
        }

        public ServerStorage()
        {
        }

        public void Initialise(SqlConnectionInfo connectionInfo)
        {
            Connection = connectionInfo;

            if (Loader != null)
            {
                Loader = new DatabaseLoader();
                Loader.Initialise(connectionInfo);
            }

            DatabaseList = new List<IDatabaseDictionary>();

            foreach (var name in Loader.DatabaseList)
            {
                // TODO: What is the goal of creating a dictionary
                //       losing the reference to it then?
                //var dic = new DatabaseDictionary();
                //dic.Initialise(name);
            }
        }

        public void RefreshDatabaseList()
        {
        }
    }
}
