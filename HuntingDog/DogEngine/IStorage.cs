using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuntingDog.DogEngine
{
    public interface IStorage
    {
        bool Exists(String key);
        String GetByName(String key);
        void StoreByName(String key, String value);
    }

    public interface ISavableStorage : IStorage
    {
        void Save();
    }
}
