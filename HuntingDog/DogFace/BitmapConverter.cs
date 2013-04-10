
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using HuntingDog.Core;

namespace HuntingDog.DogFace
{
    class BitmapConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return (value is Bitmap)
                ? (value as Bitmap).ToBitmapSource()
                : null;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
