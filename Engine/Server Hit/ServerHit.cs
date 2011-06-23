using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseObjectSearcher;

namespace DatabaseObjectSearcher
{


    public class DatabaseHit : SmartDictionary<string, int>
    {
        public int GetHitValue(string name)
        {
            return GetDictionary()[name];
        }


        public void IncreaseHitValue(string name)
        {
            var hit = GetOrCreate(name);
            GetDictionary()[name] = ++hit;
        }

        public void RemoveUnusedHits(Dictionary<string, DatabaseSearchResult> existingObjects)
        {
            var dic = GetDictionary();

            // find al unused keys
            var unsusedList = (from string k in dic.Keys
                               where !existingObjects.ContainsKey(k)
                               select k).ToList<string>();


            // remove them from dictionary
            foreach (string unusedKey in unsusedList)
                dic.Remove(unusedKey);

        }

    }

    public class ServerHit : SmartDictionary<string, DatabaseHit>
    {

    }


    public class TotalHit : SmartDictionary<string, ServerHit>
    {

    }
}
