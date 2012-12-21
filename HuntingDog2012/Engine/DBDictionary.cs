using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;


using EnvDTE80;
using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Linq;
using StringUtils;

namespace DatabaseObjectSearcher
{
    public enum EResultBehaviour : int
    {
        ByUsage = 1,
        Alphabetically = 2
    }

    public class ObjectFilter
    {
        public bool ShowTables { get; set; }
        public bool ShowSP { get; set; }
        public bool ShowViews { get; set; }
        public bool ShowFunctions { get; set; }

    }

    public class SearchCriteria
    {
        public string Schema { get; set; }
        public int FilterType { get; set; }
        public string[] CritariaAnd { get; set; }
        public EResultBehaviour ResultBehaviour { get; set; }
    }
    public class DBDictionary
    {

        Dictionary<string, Dictionary<string, DatabaseSearchResult>> dic = new Dictionary<string, Dictionary<string, DatabaseSearchResult>>();

        public List<string> GetAvailableDataBases()
        {
            return dic.Keys.ToList<string>();
        }

        ServerHit _srvHit;


        internal void SetHitStorage(ServerHit srvHit)
        {

            foreach (var dbData in dic)
            {
                if (srvHit.IsExist(dbData.Key))
                {
                    // remove unused objects from hit history
                    DatabaseHit dbHit = srvHit.Get(dbData.Key);
                    dbHit.RemoveUnusedHits(dbData.Value);
                }
            }
            _srvHit = srvHit;
        }

        internal void AddDataBase(Database d, SqlConnectionInfo connectionInfo)
        {
            dic[d.Name] = new Dictionary<string, DatabaseSearchResult>();
        }


        // clea all objects from the database
        internal void ClearObjectsOnly()
        {
            foreach (var dbName in dic)
            {
                dbName.Value.Clear();
            }
        }

        private List<string> _filterDbNames;
        public void SetFilter(IEnumerable<string> filerDbs)
        {
            _filterDbNames = filerDbs.ToList<string>();
        }

        public void ResetFilter()
        {
            _filterDbNames = null;
        }

        public void AddWithConn(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo)
        {
            if (!dic.ContainsKey(d.Name))
                dic[d.Name] = new Dictionary<string, DatabaseSearchResult>();

            

            var objDictionary = dic[d.Name];
            var searchResult = new DatabaseSearchResult(obj, connectionInfo, d);

            objDictionary[searchResult.SearchName] = searchResult;
        }

        public bool Exist(string key)
        {
            return dic.ContainsKey(key);
        }

        public const string AND = "{AND}";

        public List<DatabaseSearchResult> Find(string dbName, List<Link> listToSearch)
        {
            var res = new List<DatabaseSearchResult>();
            var dataBase = dic[dbName];
            foreach (var item in listToSearch)
            {
                DatabaseSearchResult found;
                string searchName = (item.Schema + item.Name).ToLower();
                if (dataBase.TryGetValue(searchName, out found))
                    res.Add(found);
            }
          

            return res;
        }

        public List<DatabaseSearchResult> SearchInDatabase(string searchText, string databaseName, int limit)
        {

            var result = new List<DatabaseSearchResult>();
            // return emtpy result if datavase name is invalid
            if (!dic.ContainsKey(databaseName))
                return result;

            SearchCriteria searchCrit = PrepareCriteria(searchText, new ObjectFilter());   

            // now search through all objects
            foreach (KeyValuePair<string, DatabaseSearchResult> entry in dic[databaseName])
            {
                if (IsMatch(entry.Value, searchCrit))
                {
                     result.Add(entry.Value);
                }

                // stop searching once we reached limit
                if (result.Count >= limit)
                    break;
            
            }

            return result;
            
        }

        public List<DatabaseSearchResult> Search(string criteria, int limit,
            EResultBehaviour behaviour,ObjectFilter objectFilter,
            out bool isMoreThanLimit, out bool foundHitObjects)
        {
            var searchCrit = PrepareCriteria(criteria, objectFilter);             
            searchCrit.ResultBehaviour = behaviour;

            var res = new Dictionary<string, DatabaseSearchResult>();

            isMoreThanLimit = false;
            foundHitObjects = false;

            foreach (string dbName in dic.Keys)
            {
                if (_filterDbNames != null)
                {
                    // Filter DataBase
                    if (!_filterDbNames.Contains(dbName))
                        continue;
                }

                if (SearchInDatabase(dbName, dic[dbName],
                    searchCrit, res, limit))
                {
                    // we found enough objects (equal to Limit)
                    foundHitObjects = true;

                }

                if (res.Count >= limit)
                {
                    isMoreThanLimit = true;
                    break;
                }

            }

            return res.Values.ToList<DatabaseSearchResult>();
        }


        private bool SearchInDatabase(string dbName,
            Dictionary<string, DatabaseSearchResult> dbData,
            SearchCriteria crit,
            Dictionary<string, DatabaseSearchResult> res,
            int limitSearch)
        {

            bool foundHitObjects = false;

            // search in hit history first
            if (crit.ResultBehaviour == EResultBehaviour.ByUsage && _srvHit!=null && _srvHit.IsExist(dbName))
            {
                var dbHit = _srvHit.Get(dbName);

                foreach (var hitEntry in dbHit)
                {
                    string hitObjectName = hitEntry.Key;
                    if (dbData.ContainsKey(hitObjectName))
                    {
                        var entry = dbData[hitObjectName];

                        if (AddIfMatch(entry, crit, res))
                        {
                            foundHitObjects = true;
                            if (res.Count >= limitSearch)
                            {
                                return foundHitObjects;
                            }

                        }
                    }


                }
            }


            // now search through all objects
            foreach (KeyValuePair<string, DatabaseSearchResult> entry in dbData)
            {
                if (AddIfMatch(entry.Value, crit, res))
                {
                    if (res.Count >= limitSearch)
                    {
                        return foundHitObjects;
                    }

                }

            }

            return foundHitObjects;

        }

        private bool AddIfMatch(
            DatabaseSearchResult entry,
             SearchCriteria crit,
             Dictionary<string, DatabaseSearchResult> res)
        {
            if (IsMatch(entry, crit) && !res.ContainsKey(entry.SearchName))
            {
                res[entry.SearchName] = entry;
                return true;
            }

            return false;
        }

        private bool IsMatch(
            DatabaseSearchResult entry,
             SearchCriteria crit)
        {
            // filter by schema name
            if (crit.Schema != null)
            {
                if (!entry.Schema.Contains(crit.Schema))
                    return false;
            }

            // filter only one flag is set (-s or -t ir -f or -v or combinations)
            // if both flags are set - do not filter
            // FILTER OUT 
            if (crit.FilterType != 0)
            {
                // test Bits inside filter
                if (((int)entry.ObjectType & crit.FilterType) == 0)
                    return false;
            }

            // filter by search criteria
            if (MatchAnd(crit.CritariaAnd, entry.SearchName))
            {
               // if(highlightMatch)
               //     entry.HighlightName = Utils.ReplaceString(entry.Name, crit.CritariaAnd);
                return true;
            }

            return false;
        }


        private static string GetSchema(string criteria)
        {
            var indexOFschema = criteria.IndexOf("x:");
            if (indexOFschema == -1)
                return null;

            indexOFschema += 2;

            var lastIndex = criteria.IndexOf(" ", indexOFschema);
            if (lastIndex == -1)
                return criteria.Substring(indexOFschema);
            else
                return criteria.Substring(indexOFschema, lastIndex - indexOFschema);
        }

        private static SearchCriteria PrepareCriteria(string criteria,ObjectFilter objFilter)
        {
            var searchCrit = new SearchCriteria();

            searchCrit.Schema = GetSchema(criteria);

            // remove criteria from search string
            if (searchCrit.Schema != null)
                criteria = criteria.Replace("x:" + searchCrit.Schema, "");

            string crtLower = criteria.ToLower().Trim();

            crtLower = crtLower.Replace(" ", AND);

            if (crtLower.Contains("/s") || objFilter.ShowSP)
                searchCrit.FilterType |= (int)ObjType.StoredProc;

            if (crtLower.Contains("/t") || objFilter.ShowTables)
                searchCrit.FilterType |= (int)ObjType.Table;

            if (crtLower.Contains("/f")|| objFilter.ShowFunctions)
                searchCrit.FilterType |= (int)ObjType.Func;

            if (crtLower.Contains("/v")|| objFilter.ShowViews)
                searchCrit.FilterType |= (int)ObjType.View;

            crtLower = crtLower.Replace("/s", "");
            crtLower = crtLower.Replace("/t", "");
            crtLower = crtLower.Replace("/f", "");
            crtLower = crtLower.Replace("/v", "");           

            searchCrit.CritariaAnd = crtLower.Split(new string[] { AND }, StringSplitOptions.RemoveEmptyEntries);

            return searchCrit;
        }

        private bool MatchAnd(string[] critsAnd, string p)
        {
            foreach (var and in critsAnd)
            {

                if (!p.Contains(and))
                    return false;

            }

            return true;

        }

        internal void IncreaseHit(DatabaseSearchResult res)
        {
            if (_srvHit == null)
                return;
            string dbName = res.DataBase.Name;

            var dbHit = _srvHit.GetOrCreate(dbName);
            dbHit.IncreaseHitValue(res.SearchName);
        }
    }
}
