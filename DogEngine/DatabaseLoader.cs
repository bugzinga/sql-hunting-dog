using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Diagnostics;

namespace HuntingDog.DogEngine
{
    public class DatabaseLoader : IDatabaseLoader,IDisposable
    {

        SqlConnectionInfo _connectionInfo;
        Server _server;
        public SqlConnectionInfo Connection { get { return _connectionInfo; }  }
        List<IDatabaseDictionary> DictionaryList { get; set; }

        public string Name
        {
            get { return _connectionInfo.ServerName; }
        }

        public void Dispose()
        {
        }

        public void Initialise(SqlConnectionInfo connectionInfo)
        {
            DictionaryList = new List<IDatabaseDictionary>();
            this._connectionInfo = connectionInfo;
            _server = new Server(new ServerConnection(connectionInfo));

            //TODO: Performance - init fields should be "IsSystemObject","Name". Need to test performance.

            // these give you a HUGE perf win with SMO - it pre-fetches these, rather than having to make another call to SQL Server to get this value
            _server.SetDefaultInitFields(typeof(StoredProcedure), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(StoredProcedure), "Name");
            _server.SetDefaultInitFields(typeof(View), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(View), "Name");
            _server.SetDefaultInitFields(typeof(Table), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(Table), "Name");
            _server.SetDefaultInitFields(typeof(UserDefinedFunction), "IsSystemObject");
            //server.SetDefaultInitFields(typeof(UserDefinedFunction), "Name");
            _server.SetDefaultInitFields(typeof(Database), "IsSystemObject");
            _server.SetDefaultInitFields(typeof(Database), "IsAccessible");
            _server.SetDefaultInitFields(typeof(Column), true);
            _server.SetDefaultInitFields(typeof(StoredProcedureParameter), true);


         
        }


        public List<DatabaseSearchResult> Find(string searchText, string databaseName,int limit)
        {
             IDatabaseDictionary dbDictionary = DictionaryList.FirstOrDefault(x=>x.DatabaseName == databaseName);
             if(dbDictionary==null)
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
            _server.Databases.Refresh();


        }

        public List<string> DatabaseList
        {
            get { return (from Database d in _server.Databases select d.Name).ToList(); }
        }

        void FillDatabase(IDatabaseDictionary databaseDictionary)
        {
            var timer= new Stopwatch();
            timer.Start();

            Database d = null;
            if(_server.Databases.Contains(databaseDictionary.DatabaseName))
            {
                d = _server.Databases[databaseDictionary.DatabaseName];
            }
            else
            {
                MyLogger.LogError("Database name could not be found :" +databaseDictionary.DatabaseName + ". Loader failed.");
                return;
            }

            databaseDictionary.Clear();

            RefresDatabase(d);

            LoadObjects(d, databaseDictionary);

            databaseDictionary.MarkAsLoaded();

            MyLogger.LogPerformace("Loading database " + databaseDictionary.DatabaseName, timer);
        }

        void LoadObjects(Database d,IDatabaseDictionary databaseDictionary)
        {
            
               if (!d.IsSystemObject && d.IsAccessible)
                {
                    try
                    {
                        LoadTables(d,databaseDictionary);

                        LoadStoredProcs(d,databaseDictionary);

                        LoadViews(d,databaseDictionary);

                        LoadFunctions(d,databaseDictionary);
                    }
                    catch (Exception ex)
                    {
                        // this can get thrown for security reasons - probably need to swallow here
                        MyLogger.LogError("Security Error in database :" + d.Name + "", ex);
                        var a = d.Name;
                    }
                }

        }

        public void RefreshDatabase(string name)
        {
            var d = _server.Databases[name];
            RefresDatabase(d);
        }

        private void RefresDatabase(Database d)
        {
                   
            try
            {
                d.Refresh();
            }
            catch { }

            try
            {
                d.Tables.Refresh();
            }
            catch { }

            try
            {
                d.StoredProcedures.Refresh();
            }
            catch
            {

            }

            try { d.Views.Refresh(); }
            catch { }
            try { d.UserDefinedFunctions.Refresh(); }
            catch { }

        }



        private void LoadFunctions(Database d,IDatabaseDictionary databaseDictionary)
        {
            foreach (UserDefinedFunction f in d.UserDefinedFunctions)
            {
                try
                {
                    if (!f.IsSystemObject)
                    {
                        databaseDictionary.Add(d, f, _connectionInfo);
                    }
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading view:" + f.Name + " from db:" + d.Name + "", ex);
                }
            }
        }
     
        private void LoadViews(Database d,IDatabaseDictionary databaseDictionary)
        {
            foreach (View v in d.Views)
            {
                try
                {
                    if (!v.IsSystemObject)
                        databaseDictionary.Add(d, v, _connectionInfo);
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading view:" + v.Name + " from db:" + d.Name + "", ex);
                }
            }
        }

        private void LoadStoredProcs(Database d,IDatabaseDictionary databaseDictionary)
        {
            foreach (StoredProcedure p in d.StoredProcedures)
            {
                try
                {
                    if (!p.IsSystemObject)
                        databaseDictionary.Add(d, p, _connectionInfo);
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading procedure:" + p.Name + " from db:" + d.Name + "", ex);
                }
            }
        }

        private void LoadTables(Database d,IDatabaseDictionary databaseDictionary)
        {
            foreach (Table t in d.Tables)
            {
                try
                {
                    if (!t.IsSystemObject)
                        databaseDictionary.Add(d, t, _connectionInfo);
                }
                catch (Exception ex)
                {
                    MyLogger.LogError("Error loading table:" + t.Name + " from db:" + d.Name + "", ex);
                }
            }
        }
    }
}
