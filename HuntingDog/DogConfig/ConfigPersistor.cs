using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace HuntingDog.Config
{
    public class ConfigPersistor
    {
        private IEnumerable<PropertyInfo> GetProperties(Type typeOfObject)
        {
            if(!_properties.ContainsKey(typeOfObject))
            {
                _properties[typeOfObject] = new List<PropertyInfo>(typeOfObject.GetProperties());
            }
            return _properties[typeOfObject];        
        }

        Dictionary<Type, IEnumerable<PropertyInfo>> _properties = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        public void Persist(object objectToPersist, DogEngine.IStorage storage)
        {
            if (objectToPersist == null)
                throw new ArgumentNullException("objectToPersist", "Unable to persist Null object");

            foreach (var prop in GetProperties(objectToPersist.GetType()))
            {
                var propValue = prop.GetValue(objectToPersist,null);
                PersistProperty(prop, propValue, storage);
            }
        }

        public T Restore<T>(DogEngine.IStorage storage) where T:class
        {
            var restoredObject = Activator.CreateInstance( typeof(T));

            foreach (var prop in GetProperties(typeof(T)))
            {
                RestoreProperty(prop, restoredObject, storage);
            }

            return (T)restoredObject;
        }

        private void RestoreProperty(PropertyInfo prop, object objToRead,DogEngine.IStorage storage)
        {
            if (storage.Exists(prop.Name))
            {
                try
                {
                    var propValue = storage.GetByName(prop.Name);
                    prop.SetValue(objToRead, GetConvertedValue(prop,propValue), null); 
                }
                catch(Exception ex)
                {  
                    //TODO: add log if unable to restore property for any reason
                }
            }
        }

        private object GetConvertedValue(PropertyInfo prop,object propValue)
        {

            if (prop.PropertyType.IsEnum)
            {
                return Enum.Parse(prop.PropertyType, propValue.ToString());
            }
            else
            {
                return Convert.ChangeType(propValue, prop.PropertyType);
            }
        }

        private void PersistProperty(PropertyInfo prop, object propValue,DogEngine.IStorage storage)
        {         
           var toString = (string)Convert.ChangeType(propValue, typeof(string));
           storage.StoreByName(prop.Name, toString);
        }
    }
}
