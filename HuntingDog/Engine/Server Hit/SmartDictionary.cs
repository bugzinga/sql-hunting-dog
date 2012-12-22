
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace DatabaseObjectSearcher
{
    public class SmartDictionary<TKey, TValue> : IXmlSerializable, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> internalDic = new Dictionary<TKey, TValue>();

        public Dictionary<TKey, TValue> GetDictionary()
        {
            return internalDic;
        }

        public TValue Get(TKey key)
        {
            return internalDic[key];
        }

        public TValue GetOrCreate(TKey key)
        {
            // try to find value in dictionary
            TValue value;

            if (!internalDic.TryGetValue(key, out value))
            {
                // cannot find - create a new one
                value = Activator.CreateInstance<TValue>();
                internalDic[key] = value;
            }

            return value;
        }

        public Boolean IsExist(TKey key)
        {
            return internalDic.ContainsKey(key);
        }

        public void Save(String fullName)
        {
            Serializator.Save(fullName, this);
        }

        public static T LoadFrom<T>(String fullName)
        {
            return Serializator.Load<T>(fullName);
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            // Start to use the reader.
            reader.Read();

            // Read the first element i.e. root of this object
            reader.ReadStartElement("PublicFields");

            var fields = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var f in fields)
            {
                XmlSerializer fieldSer = new XmlSerializer(f.PropertyType);
                var value = fieldSer.Deserialize(reader);
                f.SetValue(this, value, null);

                // stop reading fields
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            reader.ReadEndElement();

            // Read the first element i.e. root of this object
            reader.ReadStartElement("SmartDictionary");

            // Read all elements
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                // Parsing the key and value 
                TKey key = (TKey) keySerializer.Deserialize(reader);
                TValue value = (TValue) valueSerializer.Deserialize(reader);

                // end reading the item.
                //reader.ReadEndElement();
                //reader.MoveToContent();

                // add the item
                internalDic.Add(key, value);
            }

            // Extremely important to read the node to its end.
            // next call of the reader methods will crash if not called.
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            writer.WriteStartElement("PublicFields");
            var fields = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var f in fields)
            {
                XmlSerializer fieldSer = new XmlSerializer(f.PropertyType);
                fieldSer.Serialize(writer, f.GetValue(this, null), null);
            }

            writer.WriteEndElement();

            // Write the root element 
            writer.WriteStartElement("SmartDictionary");

            // store all items from dictionary
            foreach (var keyValue in internalDic)
            {
                // Write item, key and value
                keySerializer.Serialize(writer, keyValue.Key, null);
                valueSerializer.Serialize(writer, keyValue.Value, null);
            }

            // close /SmartDictionary tag
            writer.WriteEndElement();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return internalDic.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalDic.GetEnumerator();
        }

        #endregion
    }
}
