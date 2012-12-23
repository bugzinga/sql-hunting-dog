
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
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

        void Initialize(SqlConnectionInfo connectionInfo);

        void RefreshDatabaseList();
    }
}
