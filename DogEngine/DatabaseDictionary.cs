using System;
using System.Collections.Generic;
using DatabaseObjectSearcher;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    /// <summary>
    /// Stores all objects for a database. (tables/views/functions/procedures). Can search using a search criteria
    /// </summary>
    public class DatabaseDictionary : IDatabaseDictionary, IDisposable
    {
        public const string And_Clause = "{AND}";
        public bool IsLoaded { get; private set; }
        public string DatabaseName { get; private set; }
        readonly Dictionary<string, DatabaseSearchResult> _dictionary = new Dictionary<string, DatabaseSearchResult>();

        public List<DatabaseSearchResult> Find(string searchText, int limit)
        {
          
            var result = new List<DatabaseSearchResult>();

            if (!IsLoaded)
            {

                MyLogger.LogError("Trying to search not loaded database. DB name:"+ DatabaseName);
                return result;
            }

            SearchCriteria searchCrit = PrepareCriteria(searchText);   

            // now search through all objects
            foreach (KeyValuePair<string, DatabaseSearchResult> entry in _dictionary)
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

        private bool MatchAnd(string[] critsAnd, string p)
        {
            foreach (var and in critsAnd)
            {
                if (!p.Contains(and))
                    return false;

            }

            return true;

        }



        public void Initialise(string databaseName)
        {
            DatabaseName = databaseName;
            IsLoaded = false;
        }    

        public void Dispose()
        {
            _dictionary.Clear();
        }

        public void Clear()
        {

            IsLoaded = false;
            _dictionary.Clear();
        }

        public void MarkAsLoaded()
        {
            IsLoaded = true; 
        }

        public void Add(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo)
        {
            var searchResult = new DatabaseSearchResult(obj, connectionInfo, d);
            _dictionary[searchResult.SearchName] = searchResult;
        }




        private static SearchCriteria PrepareCriteria(string criteria)
        {
            var searchCrit = new SearchCriteria();

            searchCrit.Schema = GetSchema(criteria);

            // remove criteria from search string
            if (searchCrit.Schema != null)
                criteria = criteria.Replace("x:" + searchCrit.Schema, "");

            string crtLower = criteria.ToLower().Trim();

            crtLower = crtLower.Replace(" ", And_Clause);

            if (crtLower.Contains("/s") )
                searchCrit.FilterType |= (int)ObjType.StoredProc;

            if (crtLower.Contains("/t") )
                searchCrit.FilterType |= (int)ObjType.Table;

            if (crtLower.Contains("/f") )
                searchCrit.FilterType |= (int)ObjType.Func;

            if (crtLower.Contains("/v") )
                searchCrit.FilterType |= (int)ObjType.View;

            crtLower = crtLower.Replace("/s", "");
            crtLower = crtLower.Replace("/t", "");
            crtLower = crtLower.Replace("/f", "");
            crtLower = crtLower.Replace("/v", "");

            searchCrit.CritariaAnd = crtLower.Split(new string[] { And_Clause }, StringSplitOptions.RemoveEmptyEntries);

            return searchCrit;
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

    }
}