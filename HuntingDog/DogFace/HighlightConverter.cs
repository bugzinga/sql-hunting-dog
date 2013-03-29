using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Media;
using System.Windows;

namespace HuntingDog.DogFace
{
    class HighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            HighlightableItem item = value as HighlightableItem;

            var text = item.Name;
            var upperText = text.ToUpperInvariant();
            var keyword = item.keyword.ToUpperInvariant();

            var start = upperText.IndexOf(keyword);

            var para = new Paragraph();
            para.Margin = new Thickness(0);
            para.Padding = new Thickness(0);

            if (start != -1)
            {
                var length = keyword.Length;

                var run = new Run(text.Substring(0, start));
                para.Inlines.Add(run);

                run = new Run(text.Substring(start, length));

                if (item.IsChecked)
                {
                    run.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    run.Background = new SolidColorBrush(Colors.LightYellow);
                }

                para.Inlines.Add(run);

                run = new Run(text.Substring(start + length));
                para.Inlines.Add(run);
            }
            else
            {
                var run = new Run(text);
                para.Inlines.Add(run);
            }

            FlowDocument doc = new FlowDocument(para);
            doc.PagePadding = new Thickness(0);
            doc.FontFamily = new System.Windows.Media.FontFamily("Tahoma");

            return doc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
