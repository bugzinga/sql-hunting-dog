
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(String), typeof(HtmlTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public static DependencyProperty DoHighlighProperty = DependencyProperty.Register("DoHighlight", typeof(bool), typeof(HtmlTextBlock), new UIPropertyMetadata(true, new PropertyChangedCallback(OnHighlightChanged)));



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

        public bool DoHighlight
        {
            get
            {
                return (bool)GetValue(DoHighlighProperty);
            }

            set
            {
                SetValue(DoHighlighProperty, value);
            }
        }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock) s;
            sender.Parse((String) e.NewValue);
        }

        public static void OnHighlightChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;
            sender.ChangeHighlight((bool)e.NewValue);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Parse(Html);
        }

        private void ChangeHighlight(bool isHighlight)
        {
            // change brush accroding to selection - I recommend to remove highligh at all if item is selected
            var brush = isHighlight ? new SolidColorBrush(Colors.LightCyan):new SolidColorBrush(Colors.LightYellow);          
            foreach (Run run in _runs)
            {
                    
                run.Background = brush;
            }
          
        }

        List<Run> _runs = new List<Run>();

        private void Parse(String html)
        {
           
            Inlines.Clear();

            _runs.Clear();

            if (html.Length > 5)
            {
                var run = new Run(html.Substring(0, 5));
                Inlines.Add(run);

                run = new Run(html.Substring(5));
                run.Background = new SolidColorBrush(Colors.LightYellow);
                Inlines.Add(run);

                // list of runs with different background
                _runs.Add(run);
                //update in-lines
            }
            else
            {
                var run = new Run(html);
                Inlines.Add(run);

            }
            //Inlines.Add(new Span());
        }
    }
}
