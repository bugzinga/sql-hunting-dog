
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
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

        void Initialise(IServerWithConnection connectionInfo);

        void RefreshDatabaseList();

        void RefreshDatabase(String name);
    }
}
