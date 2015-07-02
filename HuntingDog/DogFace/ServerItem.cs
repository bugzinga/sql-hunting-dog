
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    public interface IServerItem
    {
        IServer Server{get;}
        string Name { get; }    
    }

    public class ServerItem : DependencyObject, IServerItem
    {
        public  ServerItem(IServer srv, ImageSource img)
        {
            Server = srv;
            name = srv.ServerName;
            Image = img;
        }

        public ImageSource Image
        {
            get;
            set;
        }

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
            }
        }

        public IServer Server
        {
            get;
            set;
        }


        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(Boolean),
            typeof(ServerItem)
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
            typeof(ServerItem)
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
