
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(String), typeof(HtmlTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public String Html
        {
            get
            {
                return (String) GetValue(HtmlProperty);
            }

            set
            {
                SetValue(HtmlProperty, value);
            }
        }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock) s;
            sender.Parse((String) e.NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        private void Parse(String html)
        {
            Inlines.Clear();
            //update in-lines
            //Inlines.Add(new Span());
        }
    }
}
