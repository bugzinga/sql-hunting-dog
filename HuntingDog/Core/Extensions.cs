
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace HuntingDog.Core
{
    public static class Extensions
    {
        public static Boolean IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static Boolean SafeRun(this Object o, Action action, String context)
        {
            var success = true;

            try
            {
                if (action != null)
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = (context != null)
                    ? (context + ": ")
                    : String.Empty;

                LogFactory.GetLog(o.GetType()).Error((errorMessage + ex.Message), ex);

                success = false;
            }

            return success;
        }

        public static T FindChild<T>(this DependencyObject from) where T : class
        {
            T candidate = (from as T);

            if (candidate != null)
            {
                return candidate;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(from); i++)
            {
                var found = FindChild<T>(VisualTreeHelper.GetChild(from, i));

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static T FindAncestor<T>(this DependencyObject from) where T : class
        {
            T candidate = (from as T);

            if (candidate != null)
            {
                return candidate;
            }

            return FindAncestor<T>(VisualTreeHelper.GetParent(from));
        }
    }
}
