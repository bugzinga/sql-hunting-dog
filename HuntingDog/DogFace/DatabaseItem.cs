
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    public interface IDatabaseItem
    {
        string Name { get; }
    }


    public class DatabaseItem : DependencyObject, IDatabaseItem
    {
        public DatabaseItem(string dbName, ImageSource img)
        {
            Name = dbName;
            Image = img;
        }

        public ImageSource Image {get;set;}
        public String Name { get; set; }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(Boolean),
            typeof(DatabaseItem)
        );

        public Boolean IsChecked
        {
            get
            {
                return (Boolean)GetValue(IsCheckedProperty);
            }

            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsMouseOverProperty = DependencyProperty.Register(
            "IsMouseOver",
            typeof(Boolean),
            typeof(DatabaseItem)
        );

        public Boolean IsMouseOver
        {
            get
            {
                return (Boolean)GetValue(IsMouseOverProperty);
            }

            set
            {
                SetValue(IsMouseOverProperty, value);
            }
        }

    }
}
