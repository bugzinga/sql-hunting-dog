using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using HuntingDog.DogEngine.Impl;
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

        public String Description
        {
            get;
            set;
        }

        public String Tooltip
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
