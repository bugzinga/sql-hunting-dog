using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseObjectSearcher;
using System.IO;
using System.IO.IsolatedStorage;


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

        public const string _settingFileName = "HuntingDogPreferences.txt";

        public void Save()
        {
            try
            {
                var isoStore = GetIsolatedStorageFile();

                var oStream =new IsolatedStorageFileStream(_settingFileName, FileMode.Create, isoStore);

                using(var writer = new StreamWriter(oStream))
                {
                    foreach (var entry in this)
                    {
                        writer.WriteLine(entry.Key);
                        writer.WriteLine(entry.Value);                        
                    }
                    writer.Close();
                }

                oStream.Close();
                //var dirName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HuntingDog");

                //if (!Directory.Exists(dirName))
                //    Directory.CreateDirectory(dirName);

                //var fullName = Path.Combine(dirName, _settingFileName);

                //Serializator.Save(fullName, this);
            }
            catch (Exception ex)
            {
                MyLogger.LogError("Could not save user preferences:" + ex.Message,ex);
            }
        }


        public static UserPreferencesStorage Load()
        {
            try
            {
                var isoStore = GetIsolatedStorageFile();

                if(isoStore.GetFileNames(_settingFileName).Length>0)
                {
                
                    var iStream =new IsolatedStorageFileStream(_settingFileName,FileMode.Open, isoStore);

                    using(var reader = new StreamReader(iStream))
                    {

                        var newPref = new UserPreferencesStorage();

                        while(true)
                        {
                            var lineKey = reader.ReadLine();
                            var lineValue = reader.ReadLine();
                            if(lineKey==null || lineValue==null)
                                break;

                            newPref.Add(new Entry(){Key = lineKey,Value = lineValue});
                        }
                     
                        return newPref;
                    }

                  
                    //return Serializator.Load<UserPreferencesStorage>(fullName);
                }
            }
            catch(Exception ex)
            {
                MyLogger.LogMessage("Could not load user preferences:" + ex.Message);
            }

            return new UserPreferencesStorage();

        }

        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            return isoStore;
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
