using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    class ServerStorage : IServerStorage
    {
        public  IDatabaseLoader  Loader{ get; set; }

        public SqlConnectionInfo Connection { get; private set; }
        public ServerStorage()
        {
        }

        public string Name
        {
            get { return Connection.ServerName; }
        }

        public void Initialise(SqlConnectionInfo connectionInfo)
        {
            Connection = connectionInfo;
            if (Loader != null)
            {
                Loader = new DatabaseLoader();
                Loader.Initialise(connectionInfo);
            }
            
            DatabaseList= new List<IDatabaseDictionary>();

            foreach (string name in Loader.DatabaseList)
            {
                var dic = new DatabaseDictionary();
                dic.Initialise(name);
            }

        }

        public List<IDatabaseDictionary> DatabaseList { get; private set; }
    

        public void RefreshDatabaseList()
        {
            
        }
    }
}
