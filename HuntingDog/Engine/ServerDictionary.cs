
using System;

namespace DatabaseObjectSearcher
{
    public enum EResultBehaviour : int
    {
        ByUsage = 1,
        Alphabetically = 2
    }

    public class ObjectFilter
    {
        public Boolean ShowTables
        {
            get;
            set;
        }

        public Boolean ShowSP
        {
            get;
            set;
        }

        public Boolean ShowViews
        {
            get;
            set;
        }

        public Boolean ShowFunctions
        {
            get;
            set;
        }
    }

    public class SearchCriteria
    {
        public String Schema
        {
            get;
            set;
        }

        public Int32 FilterType
        {
            get;
            set;
        }

        public String[] CritariaAnd
        {
            get;
            set;
        }

        public EResultBehaviour ResultBehaviour
        {
            get;
            set;
        }
    }
}
