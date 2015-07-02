
using System;
using System.Collections.Generic;

namespace HuntingDog.DogEngine
{
    public class Entity
    {
        public String FullName
        {
            get;
            set;
        }

        public String Name
        {
            get;
            set;
        }


        public String Details
        {
            get;
            set;
        }

        public Boolean IsProcedure
        {
            get;
            set;
        }

        public Boolean IsTable
        {
            get;
            set;
        }

        public Boolean IsFunction
        {
            get;
            set;
        }

        public Boolean IsView
        {
            get;
            set;
        }

        public Object InternalObject
        {
            get;
            set;
        }

        public String ToSafeString()
        {
            if (InternalObject == null)
            {
                return String.Format("Name:{0} but internal object is null.", FullName);
            }

            return FullName;
        }

        public List<String> Keywords 
        { 
            get; 
            set;
        }
    }
}
