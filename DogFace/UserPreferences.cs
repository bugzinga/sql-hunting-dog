using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseObjectSearcher;
using System.IO;
using System.Xml.Serialization;

namespace HuntingDog.DogFace
{

    [Serializable]
    public class Entry
    {
            public string Key;
            public string Value;
     }


    [Serializable]
    public class UserPreferencesStorage:List<Entry> 
    {

        public const string _settingFileName = "Preferences.xml";

        public void Save()
        {
            try
            {
                var dirName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuntingDog");

                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                var fullName = Path.Combine(dirName, _settingFileName);

                Serializator.Save(fullName, this);
            }
            catch
            {
                // need to inform user - but don't want to disturb him.
            }
        }


        public static UserPreferencesStorage Load()
        {
            try
            {
                var dirName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuntingDog");
                var fullName = Path.Combine(dirName, _settingFileName);

                if(File.Exists(fullName))
                {
                    return Serializator.Load<UserPreferencesStorage>(fullName);
                }
            }
            catch
            {
                
            }

            return new UserPreferencesStorage();

        }

        public string GetByName(string key)
        {
            var item = this.FirstOrDefault(x => x.Key == key);
            if (item == null)
                return null;

            return item.Value;
        }


        public void StoreByName(string key, string value)
        {
            var item = this.FirstOrDefault(x => x.Key == key);
            if (item == null)
                Add(new Entry() { Key = key, Value = value });
            else
                item.Value = value;
        }
     

    }
}
