
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using HuntingDog.DogFace.Items;
using HuntingDog.Core;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public class HtmlTextBlock : TextBlock
    {
        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(String), typeof(HtmlTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(OnHtmlChanged)));

        public static DependencyProperty ResultItemProperty = DependencyProperty.Register("ResultItem", typeof(HuntingDog.DogFace.Items.Item), typeof(HtmlTextBlock), new UIPropertyMetadata(null, new PropertyChangedCallback(OnResultItemChanged)));

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

        public  HuntingDog.DogFace.Items.Item ResultItem
        {
            get
            {
                return (HuntingDog.DogFace.Items.Item)GetValue(ResultItemProperty);
            }

            set
            {
                SetValue(ResultItemProperty, value);
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
            //sender.Parse((String) e.NewValue);
        }

        public static void OnResultItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;
            sender.DisplayResultItem(sender.ResultItem);
        }

        public static void OnHighlightChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlTextBlock sender = (HtmlTextBlock)s;
            sender.ChangeHighlight((bool)e.NewValue);
        }


        public override void OnApplyTemplate()
        {
            //base.OnApplyTemplate();
            //Parse(Html);
        }

        List<Run> runs = new List<Run>();

        private void DisplayResultItem(HuntingDog.DogFace.Items.Item resultItem)
        {
            Inlines.Clear();
            runs.Clear();

            if (resultItem.Keywords == null)
            {

                var run = new Run(resultItem.Name);
                Inlines.Add(run);
            }
            else
            {
                try
                {
                    var ranges = FindMatchingKeyworkRanges(resultItem);
                    if (ranges.Count != 0)
                    {
                        // merge overlapping intervals in dictionary
                        var mergedResult = Range<int>.Merge(ranges);


                        BuildRunsFromMergedResults(resultItem, mergedResult);
                    }
                    else
                    {
                        var run = new Run(resultItem.Name);
                        Inlines.Add(run);
                    }
                }
                catch (Exception ex)
                {
                    // soemhting terrible happened
                      var run = new Run(resultItem.Name);
                        Inlines.Add(run);

                }
            }

               
        }

        private List<Range<int>> FindMatchingKeyworkRanges(Item resultItem)
        {
            List<Range<int>> ranges = new List<Range<int>>();
            foreach (var keyWord in resultItem.Keywords)
            {
                int startIndex = resultItem.UpperCaseNames.IndexOf(keyWord);
                if (startIndex != -1)
                {
                    int endIndex = startIndex + keyWord.Length;
                    if (endIndex > resultItem.UpperCaseNames.Length)
                        endIndex = resultItem.UpperCaseNames.Length;

                    //add start index and lenght to dictionary
                    ranges.Add(new Range<int>() {Start = startIndex, End = endIndex});
                }
            }
            return ranges;
        }
  

        private void BuildRunsFromMergedResults(Item resultItem, IEnumerable<Range<int>> mergedResult)
        {
            int currentIndex = 0;
            foreach (Range<int> range in mergedResult)
            {
                if (currentIndex < range.Start)
                {
                    // add normal run
                    var normalRun = new Run(resultItem.Name.Substring(currentIndex, range.Start - currentIndex));
                    Inlines.Add(normalRun);
                }

                // add highlighted run
                var highlightedRun = new Run(resultItem.Name.Substring(range.Start, range.End - range.Start));
                Inlines.Add(highlightedRun);
                runs.Add(highlightedRun);

                currentIndex = range.End;
            }

            if (currentIndex < resultItem.Name.Length)
            {
                // add final normal run if necessary
                var normalRun = new Run(resultItem.Name.Substring(currentIndex));
                Inlines.Add(normalRun);
            }
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

 

        //private void Parse(String html)
        //{
        //    Inlines.Clear();
        //    runs.Clear();

        //    var upperText = Html.ToUpperInvariant();
        //    var keyword = Keyword.ToUpperInvariant();

        //    var start = upperText.IndexOf(keyword);

        //    if (start != -1)
        //    {
        //        var length = keyword.Length;

        //        var run = new Run(html.Substring(0, start));
        //        Inlines.Add(run);

        //        run = new Run(html.Substring(start, length));
        //        run.Background = new SolidColorBrush(Colors.Yellow);
        //        Inlines.Add(run);
        //        runs.Add(run);

        //        run = new Run(html.Substring(length));
        //        Inlines.Add(run);
        //    }
        //    else
        //    {
        //        var run = new Run(html);
        //        Inlines.Add(run);
        //    }
        //}
    }
}
