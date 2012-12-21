using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace HuntingDog.DogFace
{
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(string),
        typeof(HtmlTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public string Html { get { return (string)GetValue(HtmlProperty); } set { SetValue(HtmlProperty, value); } }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        private void Parse(string html)
        {
            Inlines.Clear();
           //update inlines

            //Inlines.Add(new Span());
        }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;
            sender.Parse((string)e.NewValue);
        }

        public HtmlTextBlock()
        {

        }

    } 
 
}
