
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine.Impl
{
    class DatabaseLoader : IDatabaseLoader
    {
        protected readonly Log log = LogFactory.GetLog();

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

        public List<String> DatabaseList
        {
            get
            {
                return (from Database d in server.Databases
                        where !d.IsSystemObject
                        select d.Name).ToList();
            }
        }

        public void Initialise(SqlConnectionInfo connectionInfo)
        {
            DictionaryList = new List<IDatabaseDictionary>();
            this.connectionInfo = connectionInfo;
            server = new Server(new ServerConnection(connectionInfo));

            // TODO: Performance - init fields should be "IsSystemObject", "Name". Need to test performance.

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

        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        public List<DatabaseSearchResult> Find(String searchText, String databaseName, Int32 limit, List<string> keywordsToHighligh)
        {
            var dbDictionary = DictionaryList.FirstOrDefault(x => x.DatabaseName == databaseName);

            if (dbDictionary == null)
            {
                dbDictionary = new DatabaseDictionary();
                dbDictionary.Initialise(databaseName);
                DictionaryList.Add(dbDictionary);
                FillDatabase(dbDictionary);
            }

            return dbDictionary.Find(searchText, limit, keywordsToHighligh);
        }

        public void RefreshDatabaseList()
        {
            server.Databases.Refresh();
        }

        void FillDatabase(IDatabaseDictionary databaseDictionary)
        {
            var analyzer = new PerformanceAnalyzer();

            Database d = null;

            if (server.Databases.Contains(databaseDictionary.DatabaseName))
            {
                d = server.Databases[databaseDictionary.DatabaseName];
            }
            else
            {
                log.Error("Database name could not be found: " + databaseDictionary.DatabaseName + "; loader failed");
                return;
            }

            databaseDictionary.Clear();

            // do not need to refresh database RefresDatabase(d);

            log.Performance("Refreshing database " + databaseDictionary.DatabaseName, analyzer.Result);

            LoadObjects(d, databaseDictionary);
            databaseDictionary.MarkAsLoaded();

            log.Performance("Loading database " + databaseDictionary.DatabaseName, analyzer.Result);
            analyzer.Stop();
        }

        void LoadObjects(Database d, IDatabaseDictionary databaseDictionary)
        {
            if (!d.IsSystemObject && d.IsAccessible)
            {
                try
                {
                    var analyzer = new PerformanceAnalyzer();

                    LoadTables(d, databaseDictionary);
                    log.Performance("Loading tables " + d.Name, analyzer.Result);

                    LoadStoredProcs(d, databaseDictionary);
                    log.Performance("Loading procedures " + d.Name, analyzer.Result);

                    LoadViews(d, databaseDictionary);
                    log.Performance("Loading views " + d.Name, analyzer.Result);

                    LoadFunctions(d, databaseDictionary);
                    log.Performance("Loading functions " + d.Name, analyzer.Result);

                    analyzer.Stop();
                }
                catch (Exception ex)
                {
                    // this can get thrown for security reasons - probably need to swallow here
                    log.Error("Security Error in database: " + d.Name, ex);
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
                // TODO: Do not swallow, log at least.
            }

            try
            {
                d.Tables.Refresh();
            }
            catch
            {
                // TODO: Do not swallow, log at least.
            }

            try
            {
                d.StoredProcedures.Refresh();
            }
            catch
            {
                // TODO: Do not swallow, log at least.
            }

            try
            {
                d.Views.Refresh();
            }
            catch
            {
                // TODO: Do not swallow, log at least.
            }

            try
            {
                d.UserDefinedFunctions.Refresh();
            }
            catch
            {
                // TODO: Do not swallow, log at least.
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
                    log.Error("Error loading view " + f.Name + " from db " + d.Name, ex);
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
                    log.Error("Error loading view " + v.Name + " from db " + d.Name, ex);
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
                    log.Error("Error loading procedure " + p.Name + " from db " + d.Name, ex);
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
                    log.Error("Error loading table " + t.Name + " from db " + d.Name, ex);
                }
            }
        }
    }
}
