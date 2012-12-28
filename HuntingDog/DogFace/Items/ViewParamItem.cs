
using System;

namespace HuntingDog.DogFace.Items
{
    public class ViewParamItem : BaseParamItem
    {
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

        public String defaultValue
        {
            get;
            set;
        }
    }
}
