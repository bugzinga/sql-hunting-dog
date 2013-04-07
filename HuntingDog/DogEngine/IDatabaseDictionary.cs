
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
    public interface IDatabaseDictionary
    {
        String DatabaseName
        {
            get;
        }

        Boolean IsLoaded
        {
            get;
        }

        List<DatabaseSearchResult> Find(String searchCriteria, Int32 limit,List<string> keywordsToHighlight );

        void Initialise(String databaseName);

        void Clear();

        void Add(Database d, ScriptSchemaObjectBase obj, SqlConnectionInfo connectionInfo);

        void MarkAsLoaded();
    }
}
