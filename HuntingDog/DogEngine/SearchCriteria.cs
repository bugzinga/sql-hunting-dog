
using System;

namespace HuntingDog.DogEngine
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
