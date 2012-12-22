
using System;
using System.Collections.Generic;
using DatabaseObjectSearcher;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    /// <summary>
    /// Stores all objects for a database. (tables/views/functions/procedures). Can search using a search criteria.
    /// </summary>
    public class DatabaseDictionary : IDatabaseDictionary, IDisposable
    {
        private static readonly Log log = LogFactory.GetLog(typeof(DatabaseDictionary));

        public const String And_Clause = "{AND}";

        private readonly Dictionary<String, DatabaseSearchResult> dictionary = new Dictionary<String, DatabaseSearchResult>();

        public Boolean IsLoaded
        {
            get;
            private set;
        }

        public String DatabaseName
        {
            get;
            private set;
        }

        public List<DatabaseSearchResult> Find(String searchText, Int32 limit)
        {
            var result = new List<DatabaseSearchResult>();

            if (!IsLoaded)
            {
                log.Error("Trying to search not loaded database. DB name:" + DatabaseName);
                return result;
            }

            SearchCriteria searchCrit = PrepareCriteria(searchText);

            // now search through all objects
            foreach (var entry in dictionary)
            {
                if (IsMatch(entry.Value, searchCrit))
                {
                    result.Add(entry.Value);
                }

                // stop searching once we reached limit
                if (result.Count >= limit)
                {
                    break;
                }

            }

            return result;

        }

        private bool IsMatch(DatabaseSearchResult entry, SearchCriteria crit)
        {
            // filter by schema name
            if (crit.Schema != null)
            {
                if (!entry.Schema.Contains(crit.Schema))
                {
                    return false;
                }
            }

            // filter only one flag is set (-s or -t ir -f or -v or combinations)
            // if both flags are set - do not filter
            // FILTER OUT 
            if (crit.FilterType != 0)
            {
                // test Bits inside filter
                if (((Int32) entry.ObjectType & crit.FilterType) == 0)
                {
                    return false;
                }
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

        private bool MatchAnd(String[] critsAnd, String p)
        {
            foreach (var and in critsAnd)
            {
                if (!p.Contains(and))
                {
                    return false;
                }

            }

            return true;
        }

        public void Initialise(String databaseName)
        {
            DatabaseName = databaseName;
            IsLoaded = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                dictionary.Clear();
            }
        }

        public void Clear()
        {

            IsLoaded = false;
            dictionary.Clear();
        }

        public void MarkAsLoaded()
        {
            IsLoaded = true;
        }

        public void Add(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo)
        {
            var searchResult = new DatabaseSearchResult(obj, connectionInfo, d);
            dictionary[searchResult.SearchName] = searchResult;
        }

        private static SearchCriteria PrepareCriteria(String criteria)
        {
            // ignore all brackets
            criteria = criteria.Replace("]", "").Replace("[", "");
            var searchCrit = new SearchCriteria();

            searchCrit.Schema = GetSchema(criteria);

            // remove criteria from search string
            if (searchCrit.Schema != null)
            {
                criteria = criteria.Replace("x:" + searchCrit.Schema, "");
            }

            String crtLower = criteria.ToLower().Trim();

            crtLower = crtLower.Replace(" ", And_Clause);

            if (crtLower.Contains("/s"))
            {
                searchCrit.FilterType |= (int) ObjType.StoredProc;
            }

            if (crtLower.Contains("/t"))
            {
                searchCrit.FilterType |= (int) ObjType.Table;
            }

            if (crtLower.Contains("/f"))
            {
                searchCrit.FilterType |= (int) ObjType.Func;
            }

            if (crtLower.Contains("/v"))
            {
                searchCrit.FilterType |= (int) ObjType.View;
            }

            crtLower = crtLower.Replace("/s", "");
            crtLower = crtLower.Replace("/t", "");
            crtLower = crtLower.Replace("/f", "");
            crtLower = crtLower.Replace("/v", "");

            searchCrit.CritariaAnd = crtLower.Split(new String[] { And_Clause }, StringSplitOptions.RemoveEmptyEntries);

            return searchCrit;
        }

        private static String GetSchema(String criteria)
        {
            var indexOFschema = criteria.IndexOf("x:");

            if (indexOFschema == -1)
            {
                return null;
            }

            indexOFschema += 2;

            var lastIndex = criteria.IndexOf(" ", indexOFschema);

            return (lastIndex == -1)
                ? criteria.Substring(indexOFschema)
                : criteria.Substring(indexOFschema, lastIndex - indexOFschema);
        }

    }
}
