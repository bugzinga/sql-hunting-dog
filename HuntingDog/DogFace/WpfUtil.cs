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
    class WpfUtil
    {
        public static T FindChild<T>(DependencyObject from) where T : class
        {
            if (from == null)
            {
                return null;
            }

            T candidate = from as T;
            if (candidate != null)
            {
                return candidate;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(from); i++)
            {
                var isOur = FindChild<T>(VisualTreeHelper.GetChild(from, i));
                if (isOur != null)
                    return isOur;
            }

            return null;
        }


        public static T FindAncestor<T>(DependencyObject from) where T : class
        {
            if (from == null)
            {
                return null;
            }

            T candidate = from as T;
            if (candidate != null)
            {
                return candidate;
            }

            return FindAncestor<T>(VisualTreeHelper.GetParent(from));
        }

    }
}
