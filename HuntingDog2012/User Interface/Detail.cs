using System;
using System.Collections.Generic;
using System.Text;
using DatabaseObjectSearcher;

namespace DatabaseObjectSearcherUI
{

    public class Detail: IComparable
    {
      
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

        public int CompareTo(object obj)
        {
            Detail other = (Detail)obj;
            return string.Compare(other.PropertyName, this.PropertyName);
        }
    }

    public class IndexDetail : Detail, IComparable
    {
        public void AddColumn(string name)
        {
            if(!string.IsNullOrEmpty(Columns))
                Columns += ", ";
            Columns += name;
        }
        public bool IsClustered { get; set; }
        public string Columns { get; set; }
        public int FillFactor { get; set; }

        public int CompareTo(object obj)
        {
            IndexDetail other = (IndexDetail)obj;
            if(this.IsClustered && other.IsClustered)
                return string.Compare(other.Columns, this.Columns);

            if (this.IsClustered)
                return 1;

            if (other.IsClustered)
                return -1;

            return string.Compare(other.Columns, this.Columns);
        }
    }

    public class ParamDetail : Detail, IComparable
    {
        public bool In { get; set; }
        public bool Out { get; set; }

        public string DefaultValue { get; set; }

        public int CompareTo(object obj)
        {
            ParamDetail other = (ParamDetail)obj;
            return string.Compare(other.PropertyName, this.PropertyName);
        }
    }

    public class ColumnDetail : Detail, IComparable
    {
        public bool isFK { get; set; }
        public bool isPK { get; set; }
        public string FKTable { get; set; }

        public int CompareTo(object obj)
        {
            ColumnDetail other = (ColumnDetail)obj;
            if (isPK)
            {
                if (other.isPK)
                    return string.Compare(other.PropertyName, this.PropertyName);
                return 1;
            }

            if (isFK)
            {
                if (other.isPK)
                    return -1;
                if(other.isFK)
                    return string.Compare(other.PropertyName, this.PropertyName);

                return 1;
            }

            if (other.isFK || other.isPK)
                return -1;
                
            return string.Compare(other.PropertyName, this.PropertyName);

        }     
    }


    public class ServerViewState
    {
        public NavigatorServer NavServer { get; set; }
        public string SelectedDataBase{get;set;}
    }


    //helper class for Hashtable serialisation
    public class Entry
    {
        public Entry()
        {
        }

        public object Key;
        public object Value;
      
        public Entry(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }


}
