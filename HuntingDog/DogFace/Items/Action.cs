
using System;
using System.Windows.Media;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace.Items
{
    public class Action
    {
        public ImageSource Image
        {
            get;
            set;
        }

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
