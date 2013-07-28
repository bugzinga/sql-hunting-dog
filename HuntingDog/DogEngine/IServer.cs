using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace HuntingDog.DogEngine
{
    public interface IServer
    {
         string ServerName { get;}
         string ID { get; }
    }

    public interface IServerWithConnection : IServer
    {
        SqlConnectionInfo Connection { get; }
    }


    public interface IDatabase
    {
        string DatabaseName { get; }
    }

}
