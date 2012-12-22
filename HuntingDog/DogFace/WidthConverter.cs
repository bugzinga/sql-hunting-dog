
using System;
using System.Globalization;
using System.Windows.Data;

namespace HuntingDog.DogFace
{
    public class WidthConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            var diff = Double.Parse(parameter.ToString());

            // var desiredWidth =  ((double)value - 8);
            var desiredWidth = ((Double) value - diff);
            //desiredWidth -= 80;

            if (desiredWidth < 100)
            {
                desiredWidth = 100;
            }

            return desiredWidth;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
