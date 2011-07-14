using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DatabaseObjectSearcher
{
    public class DbObjectSearcher
    {
        SqlConnectionInfo connectionInfo;
        Server server;
        private object searchLock = new object();

        DBDictionary dbDic = new DBDictionary();
        public string ServerName
        {
            get
            {
                return connectionInfo.ServerName;
            }
        }

        public List<string> GetAvailableDataBases()
        {
            return dbDic.GetAvailableDataBases();
        }

        public void SetFilter(IEnumerable<string> filerDbs)
        {
            dbDic.SetFilter(filerDbs);
        }

        public void IncreaseHit(DatabaseSearchResult res)
        {
            dbDic.IncreaseHit(res);
        }

        public void SetHitStorage(ServerHit srvHit)
        {
            dbDic.SetHitStorage(srvHit);
        }

        public DbObjectSearcher(SqlConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
            server = new Server(new ServerConnection(connectionInfo));

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
            server.SetDefaultInitFields(typeof(Column), true);
            server.SetDefaultInitFields(typeof(StoredProcedureParameter), true);

        }

        public List<DatabaseDependencyResult> FindDependencyObjects(DatabaseSearchResult src, DependecyResults links)
        {
            string dbName = src.DataBase.Name;
            var res = new List<DatabaseDependencyResult>();

            foreach(var dsr in dbDic.Find(dbName, links.DependsOn))
            {
                res.Add(new DatabaseDependencyResult(){ Obj = dsr, Direction = Direction.DependsOn});
            }

            foreach (var dsr in dbDic.Find(dbName, links.DependantUpon))
            {
                res.Add(new DatabaseDependencyResult() { Obj = dsr, Direction = Direction.DependentOn });
            }

            return res;
        }

        public List<DatabaseSearchResult> FindMatchingObjects(string name, int limit,
            EResultBehaviour behavior,ObjectFilter objectFilter,
            out bool isMoreThanLimit, out bool foundHitObjects)
        {

            return dbDic.Search(name, limit, behavior,objectFilter, out isMoreThanLimit, out foundHitObjects);

        }

        public List<DatabaseSearchResult> Find(string searchText, string databaseName,int limit)
        {
            return dbDic.SearchInDatabase(searchText, databaseName,limit);
        }

        public bool HasObjectDictionary { get; private set; }


        public void RefresBaseDictionary()
        {
            lock (searchLock)
            {
                server.Refresh();
                server.Databases.Refresh();
                foreach (Database d in server.Databases)
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

                    try { d.Views.Refresh(); }
                    catch { }
                    try { d.UserDefinedFunctions.Refresh(); }
                    catch { }


                }
            }

        }

        public void BuilDataBaseDictionary()
        {
            lock (searchLock)
            {
                foreach (Database d in server.Databases)
                {
                    dbDic.AddDataBase(d, connectionInfo);
                }
            }
        }


        public void BuildDBObjectDictionary()
        {
            lock (searchLock)
            {
                // need to refresh object - new object can be added/removed
                if (HasObjectDictionary)
                    RefresBaseDictionary();

                dbDic.ClearObjectsOnly();

                foreach (Database d in server.Databases)
                {
                    try
                    {

                        if (!d.IsSystemObject && d.IsAccessible) // even with pre-fetching this is still slow if there are thousands of databases on a remote server with a slow connection
                        {
                            //dbDic.AddWithConn(d,connectionInfo);
                            //databaseObjectDictionary.Add(new DatabaseSearchResult(d, connectionInfo), d.Name);

                            try
                            {

                                foreach (Table t in d.Tables)
                                {
                                    try
                                    {
                                        if (!t.IsSystemObject)
                                        {
                                            dbDic.AddWithConn(d, t, connectionInfo);
                                        }
                                    }
                                    catch { }

                                }

                                foreach (StoredProcedure p in d.StoredProcedures)
                                {
                                    try
                                    {
                                        if (!p.IsSystemObject)
                                        {
                                            dbDic.AddWithConn(d, p, connectionInfo);
                                        }
                                    }
                                    catch { }
                                }

                                foreach (View v in d.Views)
                                {
                                    try
                                    {
                                        if (!v.IsSystemObject)
                                        {
                                            dbDic.AddWithConn(d, v, connectionInfo);
                                        }
                                    }
                                    catch { }
                                }

                                foreach (UserDefinedFunction f in d.UserDefinedFunctions)
                                {
                                    try
                                    {
                                        if (!f.IsSystemObject)
                                        {
                                            dbDic.AddWithConn(d, f, connectionInfo);
                                        }
                                    }
                                    catch { }
                                }

                            }
                            catch (ExecutionFailureException)
                            {
                                // this can get thrown for security reasons - probably need to swallow here
                                var a = d.Name;

                            }
                            catch 
                            {
                                // this also seems to be a possible security-related exception - also swallow
                                var a = d.Name;
                            }
                        }

                    }
                    catch
                    {
                        // DB can throw exception
                    }
                }//foreach
            }//lock
            HasObjectDictionary = true;
        }



    }
}
