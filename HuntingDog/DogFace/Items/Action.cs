using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace HuntingDog.DogFace.Items
{
    public class Action
    {
        public Action()
        {
            Visibility = Visibility.Visible;
        }

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

        public Visibility Visibility
        {
            get;
            set;
        }
    }
}
