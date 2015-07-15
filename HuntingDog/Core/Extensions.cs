
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

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
                var contextMessage = (context != null)
                    ? (context + ": ")
                    : String.Empty;

                LogFactory.GetLog(o.GetType()).Error((contextMessage + ex.Message), ex);

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

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            var bitmapImage = new BitmapImage();

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        
            /// <summary>
            /// Turning blur on
            /// </summary>
            /// <param name="element">bluring element</param>
            /// <param name="blurRadius">blur radius</param>
            /// <param name="duration">blur animation duration</param>
            /// <param name="beginTime">blur animation delay</param>
            public static void BlurApply(this UIElement element,
                double blurRadius, TimeSpan duration, TimeSpan beginTime)
            {
                BlurEffect blur = new BlurEffect() { Radius = 0 };
                DoubleAnimation blurEnable = new DoubleAnimation(0, blurRadius, duration) { BeginTime = beginTime };
                element.Effect = blur;
                blur.BeginAnimation(BlurEffect.RadiusProperty, blurEnable);
            }
            /// <summary>
            /// Turning blur off
            /// </summary>
            /// <param name="element">bluring element</param>
            /// <param name="duration">blur animation duration</param>
            /// <param name="beginTime">blur animation delay</param>
            public static void BlurDisable(this UIElement element, TimeSpan duration, TimeSpan beginTime)
            {
                BlurEffect blur = element.Effect as BlurEffect;
                if (blur == null || blur.Radius == 0)
                {
                    return;
                }
                DoubleAnimation blurDisable = new DoubleAnimation(blur.Radius, 0, duration) { BeginTime = beginTime };
                blur.BeginAnimation(BlurEffect.RadiusProperty, blurDisable);
            }
        
    }
}
