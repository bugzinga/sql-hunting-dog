
using System.Windows;
using System.Windows.Media;

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
                {
                    return isOur;
                }
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
