
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    public class Item : DependencyObject
    {
        private String name;

        public String Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                UpperCaseNames = Name.ToUpper();
            }
        }

        public Entity Entity
        {
            get;
            set;
        }

        public string UpperCaseNames
        {
            get;
            private set;
        }

        public String NavigationTooltip
        {
            get;
            set;
        }

        public String MainObjectTooltip
        {
            get;
            set;
        }

        public ImageSource Image
        {
            get;
            set;
        }

        public IList<Action> Actions
        {
            get;
            private set;
        }

        public List<String> Keywords
        {
            get;
            set;
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(Boolean),
            typeof(Item)
        );

        public Boolean IsChecked
        {
            get
            {
                return (Boolean) GetValue(IsCheckedProperty);
            }

            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.Register(
            "IsMouseOver",
            typeof(Boolean),
            typeof(Item)
        );

        public Boolean IsMouseOver
        {
            get
            {
                return (Boolean) GetValue(IsMouseOverProperty);
            }

            set
            {
                SetValue(IsMouseOverProperty, value);
            }
        }

        public Item()
        {
            Actions = new List<Action>();
        }
    }
}
