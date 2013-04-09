
using System;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace.Items
{
    public class Action
    {
        public String Name
        {
            get;
            set;
        }

        public System.Action<IStudioController, String> Routine
        {
            get;
            set;
        }
    }
}
