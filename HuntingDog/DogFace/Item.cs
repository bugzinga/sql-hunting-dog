
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
  
    public interface IHighlightedItem
    {
        string Name {get;}
        List<String> Keywords { get; }
    }

    public class ChildItem : DependencyObject, IHighlightedItem
    {
        public ChildItem(string name, List<string> keywords)
        {
            Name = name;
            Keywords = keywords;
        }


        public string Name
        {
            get;
            set;
        }

        public List<string> Keywords
        {
            get;
            set;
        }
    }

    public class Item : DependencyObject, IHighlightedItem
    {
        private String name;

        public IHighlightedItem ParentItem
        {
            get
            {
                return this;
            }
        }

        public Item()
        {
            Actions = new List<Action>();
        }

        public IHighlightedItem ChildItem
        {
            get
            {
                return _detailsItem;
            }
        }


        ChildItem _detailsItem;
        public void SetDetails(string details, IEnumerable<string> keywords)
        {
            _detailsItem = new ChildItem(details, Keywords);
        }

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

     
    }
}
