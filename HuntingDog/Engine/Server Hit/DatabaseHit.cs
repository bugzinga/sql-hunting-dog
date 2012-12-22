
using System;
using System.Collections.Generic;
using System.Linq;
using HuntingDog.DogEngine;
using System.Runtime.InteropServices;

namespace DatabaseObjectSearcher
{
    [ComVisible(false)]
    public class DatabaseHit : SmartDictionary<String, Int32>
    {
        public Int32 GetHitValue(String name)
        {
            return GetDictionary()[name];
        }

        public void IncreaseHitValue(String name)
        {
            var hit = GetOrCreate(name);
            GetDictionary()[name] = ++hit;
        }

        public void RemoveUnusedHits(Dictionary<String, DatabaseSearchResult> existingObjects)
        {
            var dic = GetDictionary();

            // find all unused keys
            var unsusedList = (from k in dic.Keys
                               where !existingObjects.ContainsKey(k)
                               select k).ToList<String>();

            // remove them from dictionary
            foreach (var unusedKey in unsusedList)
            {
                dic.Remove(unusedKey);
            }
        }
    }
}
