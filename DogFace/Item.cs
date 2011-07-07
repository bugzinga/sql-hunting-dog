using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuntingDog.DogFace
{
    public class ProcedureParamItem : DependencyObject
    {
        public HuntingDog.DogEngine.ProcedureParameter Entity { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Out { get; set; }
    }

    public class Item:DependencyObject
    {
        public HuntingDog.DogEngine.Entity Entity { get; set; }

        public string Name { get; set; }

        public ImageSource Image { get; set; }

        public ImageSource Action1 { get; set; }
        public string Action1Description { get; set; }
        public string Action1Tooltip { get; set; }
        
        public ImageSource Action2 { get; set; }
        public string Action2Description { get; set; }
        public string Action2Tooltip { get; set; }

        public Visibility Action3Visibility { get; set; }
        public ImageSource Action3 { get; set; }
        public string Action3Description { get; set; }
        public string Action3Tooltip { get; set; }

        public bool IsChecked { get { return (bool)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Item));


        public bool IsMouseOver { get { return (bool)GetValue(IsMouseOverProperty); } set { SetValue(IsMouseOverProperty, value); } }
        public static readonly DependencyProperty IsMouseOverProperty =
            DependencyProperty.Register("IsMouseOver", typeof(bool), typeof(Item));
    }
}
