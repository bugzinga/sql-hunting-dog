
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DatabaseObjectSearcher;
using EnvDTE;
using EnvDTE80;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;

using Thread = System.Threading.Thread;

namespace HuntingDog.DogEngine.Impl
{

    public class MyServer : IServerWithConnection, IEquatable<MyServer>, IEquatable<IServer>
    {
        SqlConnectionInfo _connectionInfo;
        public MyServer(SqlConnectionInfo ci)
        {
            _connectionInfo = ci;
        }

        public string ServerName
        {
            get { return _connectionInfo.ServerName; }
        }

        public string ID
        {
            get { return _connectionInfo.ConnectionString; }
        }


        public bool Equals(MyServer other)
        {
            if (other == null)
                return false;
            return string.Compare(other.ID, this.ID, true ) == 0;
        }


        public SqlConnectionInfo Connection
        {
            get { return _connectionInfo; }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IServer);
        }

        public override int GetHashCode()
        {
           
                return ID.GetHashCode();

        }

        public bool Equals(IServer other)
        {
            if (other == null)
                return false;
            return string.Compare(other.ID, this.ID, true) == 0;
        }
    }

    

    public interface IServerWatcher
    {
        event Action<List<IServerWithConnection>> OnServersAdded;

        event Action<List<IServerWithConnection>> OnServersRemoved;

        IEnumerable<IServerWithConnection> GetCurrentlyConnectedServers();
    }

    /// <summary>
    /// Checks connected and disconnected servers periodically and notifies subscribers
    /// </summary>
    public class ServerWatcher : IServerWatcher
    {
        private Thread serverCheck;

        private ObjectExplorerManager  _manager;
        private AutoResetEvent stopThread = new AutoResetEvent(false);

        private AutoResetEvent verifyConnectionEvent = new AutoResetEvent(false);

        private readonly Log log = LogFactory.GetLog();

        public event Action<List<IServerWithConnection>> OnServersAdded;
        public event Action<List<IServerWithConnection>> OnServersRemoved;

        public ServerWatcher(ObjectExplorerManager manager)
        {
            _manager = manager;
            _manager.OnNewServerConnected += new Action<SqlConnectionInfo>(_manager_OnNewServerConnected);
            Start();
        }

        void _manager_OnNewServerConnected(SqlConnectionInfo obj)
        {
            verifyConnectionEvent.Set();
        }


        public void Start()
        {
              // run thread - this thread will check connected servers and will report if some servers will be disconnected.
            serverCheck = new System.Threading.Thread(BackgroundThreadCheckServer);
            serverCheck.Start();
        }

        public void Stop()
        {
            stopThread.Set();
        }


        public IEnumerable<IServerWithConnection> GetCurrentlyConnectedServers()
        {
            var connStringList = _manager.GetAllServers();
            return connStringList.ConvertAll(x => new MyServer(x)).Cast<IServerWithConnection>(); 
        }

        private void BackgroundThreadCheckServer()
        {
            var oldList = GetCurrentlyConnectedServers();

            if (stopThread.WaitOne(1 * 1000))
            {
                return;
            }

            while (true)
            {
             
                try
                {
                    lock (this)
                    {
                        var newList = GetCurrentlyConnectedServers();

                        if (oldList != null)
                        {
                          
                            var removed = oldList.Except(newList).ToList();
                            var added = newList.Except(oldList).ToList();


                            if (added.Any() && OnServersAdded != null)
                            {
                                log.Info("Found " + added.Count.ToString() + " connected server");
                                OnServersAdded(added);
                            }

                            if (removed.Any() && OnServersRemoved != null)
                            {
                                log.Info("Found " + removed.Count.ToString() + " disconnected server");
                                OnServersRemoved(removed);
                            }
                            
                        }

                        oldList = newList;
                    }
                }
                catch (Exception e)
                {
                    log.Error("Thread server checker", e);
                }
                

                var waitHandles = new WaitHandle[] { stopThread, verifyConnectionEvent };

                var waitResult = WaitHandle.WaitAny(waitHandles, 3 * 1000);
                if (waitResult == 0) // stopThread event was set
                {
                    break;
                }
                
            }
        }
    }
}
