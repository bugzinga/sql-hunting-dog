
using System;
using System.Windows;

namespace HuntingDog.DogFace.Items
{
    public class BaseParamItem : DependencyObject
    {
        public HuntingDog.DogEngine.ProcedureParameter Entity
        {
            get;
            set;
        }

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
    }
}
