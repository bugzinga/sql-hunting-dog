
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    public class DatabaseLoader : IDatabaseLoader, IDisposable
    {
        private SqlConnectionInfo connectionInfo;

        private Server server;

        public Server Server
        {
            get
            {
                return server;
            }
        }

        public SqlConnectionInfo Connection
        {
            get
            {
                return connectionInfo;
            }
        }

        private List<IDatabaseDictionary> DictionaryList
        {
            get;
            set;
        }

        public String Name
        {
            get
            {
                return connectionInfo.ServerName;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
        }

        public void Initialise(SqlConnectionInfo connectionInfo)
        {
            DictionaryList = new List<IDatabaseDictionary>();
            this.connectionInfo = connectionInfo;
            server = new Server(new ServerConnection(connectionInfo));

            //TODO: Performance - init fields should be "IsSystemObject","Name". Need to test performance.

            // these give you a HUGE perf win with SMO - it pre-fetches these, rather than having to make another call to SQL Server to get this value
            server.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(StoredProcedure), "Name");
            server.SetDefaultInitFields(typeof(View), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(View), "Name");
            server.SetDefaultInitFields(typeof(Table), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(Table), "Name");
            server.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(UserDefinedFunction), "Name");
            server.SetDefaultInitFields(typeof(Database), "IsSystemObject");
            server.SetDefaultInitFields(typeof(Database), "IsAccessible");
            //_server.SetDefaultInitFields(typeof(Column), true);
            //_server.SetDefaultInitFields(typeof(StoredProcedureParameter), true);
        }

        public List<DatabaseSearchResult> Find(String searchText, String databaseName, Int32 limit)
        {
            var dbDictionary = DictionaryList.FirstOrDefault(x => x.DatabaseName == databaseName);

            if (dbDictionary == null)
            {
                dbDictionary = new DatabaseDictionary();
                dbDictionary.Initialise(databaseName);
                DictionaryList.Add(dbDictionary);
                FillDatabase(dbDictionary);
            }

            return dbDictionary.Find(searchText, limit);
        }

        public void RefreshDatabaseList()
        {
            server.Databases.Refresh();
        }

        public List<string> DatabaseList
        {
            get
            {
                return (from Database d in server.Databases
                        where d.IsSystemObject == false
                        select d.Name).ToList();
            }
        }

        void FillDatabase(IDatabaseDictionary databaseDictionary)
        {
            var timer = new Stopwatch();
            timer.Start();

            Database d = null;

            if (server.Databases.Contains(databaseDictionary.DatabaseName))
            {
                d = server.Databases[databaseDictionary.DatabaseName];
            }
            else
            {
                MyLogger.LogError("Database name could not be found :" + databaseDictionary.DatabaseName + ". Loader failed.");
                return;
            }

            databaseDictionary.Clear();

            // do not need to refresh database RefresDatabase(d);

            MyLogger.LogPerformace("Refreshing database " + databaseDictionary.DatabaseName, timer);

            LoadObjects(d, databaseDictionary);

            databaseDictionary.MarkAsLoaded();

            MyLogger.LogPerformace("Loading database " + databaseDictionary.DatabaseName, timer);
        }

        void LoadObjects(Database d, IDatabaseDictionary databaseDictionary)
        {
            if (!d.IsSystemObject && d.IsAccessible)
            {
                try
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    LoadTables(d, databaseDictionary);

                    MyLogger.LogPerformace("Loading tables " + d.Name, timer);

                    timer.Reset();
                    timer.Start();
                    LoadStoredProcs(d, databaseDictionary);

                    MyLogger.LogPerformace("Loading procedures " + d.Name, timer);

                    timer.Reset();
                    timer.Start();
                    LoadViews(d, databaseDictionary);

                    MyLogger.LogPerformace("Loading views " + d.Name, timer);
                    timer.Reset();
                    timer.Start();

                    LoadFunctions(d, databaseDictionary);

                    MyLogger.LogPerformace("Loading functions " + d.Name, timer);
                }
                catch (Exception ex)
                {
                    // this can get thrown for security reasons - probably need to swallow here
                    MyLogger.LogError("Security Error in database :" + d.Name + "", ex);
                }
            }
        }

        public void RefreshDatabase(String name)
        {
            var d = server.Databases[name];
            RefresDatabase(d);

            // remove hashed objects
            var dbDictionary = DictionaryList.FirstOrDefault(x => x.DatabaseName == name);

            if (dbDictionary != null)
            {
                DictionaryList.Remove(dbDictionary);
            }
        }

        private void RefresDatabase(Database d)
        {
            try
            {
                d.Refresh();
            }
            catch
            {
            }

            try
            {
                d.Tables.Refresh();
            }
            catch
            {
            }

            try
            {
                d.StoredProcedures.Refresh();
            }
            catch
            {
            }

            try
            {
                d.Views.Refresh();
            }
            catch
            {
            }
            
            try
            {
                d.UserDefinedFunctions.Refresh();
            }
            catch
            {
            }
        }

        private void LoadFunctions(Database d, IDatabaseDictionary databaseDictionary)
        {
            foreach (UserDefinedFunction f in d.UserDefinedFunctions)
            {
                try
                {
                    if (!f.IsSystemObject)
                    {
                        databaseDictionary.Add(d, f, connectionInfo);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading view:" + f.Name + " from db:" + d.Name + "", ex);
                }
            }
        }

        private void LoadViews(Database d, IDatabaseDictionary databaseDictionary)
        {
            foreach (View v in d.Views)
            {
                try
                {
                    if (!v.IsSystemObject)
                    {
                        databaseDictionary.Add(d, v, connectionInfo);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading view:" + v.Name + " from db:" + d.Name + "", ex);
                }
            }
        }

        private void LoadStoredProcs(Database d, IDatabaseDictionary databaseDictionary)
        {
            foreach (StoredProcedure p in d.StoredProcedures)
            {
                try
                {
                    if (!p.IsSystemObject)
                    {
                        databaseDictionary.Add(d, p, connectionInfo);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading procedure:" + p.Name + " from db:" + d.Name + "", ex);
                }
            }
        }

        private void LoadTables(Database d, IDatabaseDictionary databaseDictionary)
        {
            foreach (Table t in d.Tables)
            {
                try
                {
                    if (!t.IsSystemObject)
                    {
                        databaseDictionary.Add(d, t, connectionInfo);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading table:" + t.Name + " from db:" + d.Name + "", ex);
                }
            }
        }
    }
}
