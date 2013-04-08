
using System;

namespace DatabaseObjectSearcher
{
    class SearchCriteria
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

        public String[] CriteriaAnd
        {
            get;
            set;
        }
    }
}
