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
    public class Item:DependencyObject
    {
        public HuntingDog.DogEngine.Entity Entity { get; set; }

        public string Name { get; set; }

        public ImageSource Image { get; set; }

        public ImageSource Action { get; set; }

        public bool IsChecked { get { return (bool)GetValue(IsCheckedProperty); } set { SetValue(IsCheckedProperty, value); } }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Item));
    }
}
