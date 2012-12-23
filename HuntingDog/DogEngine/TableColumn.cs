
using System;

namespace HuntingDog.DogEngine
{
    public class TableColumn
    {
        public String Name
        {
            get;
            set;
        }

        public String Type
        {
            get;
            set;
        }

        public Boolean Nullable
        {
            get;
            set;
        }

        public Boolean IsPrimaryKey
        {
            get;
            set;
        }

        public Boolean IsForeignKey
        {
            get;
            set;
        }
    }
}
