
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

        public static DependencyProperty KeywordProperty = DependencyProperty.Register("Keyword", typeof(String), typeof(HtmlTextBlock), new UIPropertyMetadata("Keyword", new PropertyChangedCallback(OnKeywordChanged)));

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

        public String Keyword
        {
            get
            {
                return (String)GetValue(KeywordProperty);
            }

            set
            {
                SetValue(KeywordProperty, value);
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

        public static void OnKeywordChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;
            sender.Parse(sender.Html);
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
            Brush foregroundBrush = null;
            Brush backgroundBrush = null;

            if (isHighlight)
            {
                foregroundBrush = new SolidColorBrush(Colors.White);
                backgroundBrush = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            }
            else
            {
                foregroundBrush = new SolidColorBrush(Colors.Black);
                backgroundBrush = new SolidColorBrush(Colors.Yellow);
            }

            foreach (var run in runs)
            {
                run.Foreground = foregroundBrush;
                run.Background = backgroundBrush;
            }
        }

        List<Run> runs = new List<Run>();

        private void Parse(String html)
        {
            Inlines.Clear();
            runs.Clear();

            var upperText = Html.ToUpperInvariant();
            var keyword = Keyword.ToUpperInvariant();

            var start = upperText.IndexOf(keyword);

            if (start != -1)
            {
                var length = keyword.Length;

                var run = new Run(html.Substring(0, start));
                Inlines.Add(run);

                run = new Run(html.Substring(start, length));
                run.Background = new SolidColorBrush(Colors.Yellow);
                Inlines.Add(run);
                runs.Add(run);

                run = new Run(html.Substring(length));
                Inlines.Add(run);
            }
            else
            {
                var run = new Run(html);
                Inlines.Add(run);
            }
        }
    }
}
