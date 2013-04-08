
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HuntingDog.Core;
using HuntingDog.DogFace.Items;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public class HtmlTextBlock : TextBlock
    {
        private readonly Log log = LogFactory.GetLog(typeof(HtmlTextBlock));

        private List<Run> runs = new List<Run>();

        public static DependencyProperty ResultItemProperty = DependencyProperty.Register(
            "ResultItem",
            typeof(Item),
            typeof(HtmlTextBlock),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnResultItemChanged))
        );

        public static DependencyProperty DoHighlighProperty = DependencyProperty.Register(
            "DoHighlight",
            typeof(Boolean),
            typeof(HtmlTextBlock),
            new UIPropertyMetadata(true, new PropertyChangedCallback(OnHighlightChanged))
        );

        public Item ResultItem
        {
            get
            {
                return (Item) GetValue(ResultItemProperty);
            }

            set
            {
                SetValue(ResultItemProperty, value);
            }
        }

        public Boolean DoHighlight
        {
            get
            {
                return (Boolean) GetValue(DoHighlighProperty);
            }

            set
            {
                SetValue(DoHighlighProperty, value);
            }
        }

        public static void OnResultItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var htmlTextBlock = (s as HtmlTextBlock);

            if (htmlTextBlock != null)
            {
                htmlTextBlock.DisplayResultItem(htmlTextBlock.ResultItem);
            }
        }

        public static void OnHighlightChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var htmlTextBlock = (s as HtmlTextBlock);

            if ((htmlTextBlock != null) && (e != null) && (e.NewValue is Boolean))
            {
                htmlTextBlock.ChangeHighlight((Boolean) e.NewValue);
            }
        }

        private void DisplayResultItem(Item resultItem)
        {
            Inlines.Clear();
            runs.Clear();

            if ((resultItem.Keywords == null) || resultItem.Keywords.IsEmpty())
            {
                // TODO: get rid of code duplication (lines 89, 106, 116)
                var run = new Run(resultItem.Name);
                Inlines.Add(run);
            }
            else
            {
                try
                {
                    var ranges = FindMatchingKeyworkRanges(resultItem);

                    if (ranges.Any())
                    {
                        var mergedResult = Range<Int32>.Merge(ranges);
                        BuildRunsFromMergedResults(resultItem, mergedResult);
                    }
                    else
                    {
                        // TODO: get rid of code duplication (lines 89, 106, 116)
                        var run = new Run(resultItem.Name);
                        Inlines.Add(run);
                    }
                }
                // TODO: What is it for? How can any exception happen here?
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);

                    // TODO: get rid of code duplication (lines 89, 106, 116)
                    var run = new Run(resultItem.Name);
                    Inlines.Add(run);
                }
            }
        }

        private List<Range<Int32>> FindMatchingKeyworkRanges(Item resultItem)
        {
            var ranges = new List<Range<Int32>>();

            foreach (var keyword in resultItem.Keywords)
            {
                var startIndex = 0;

                while ((startIndex = resultItem.UpperCaseNames.IndexOf(keyword, startIndex)) != -1)
                {
                    int endIndex = (startIndex + keyword.Length);

                    // TODO: How is it possible?
                    if (endIndex > resultItem.UpperCaseNames.Length)
                    {
                        endIndex = resultItem.UpperCaseNames.Length;
                    }

                    ranges.Add(new Range<Int32>() { Start = startIndex++, End = endIndex });
                }
            }

            return ranges;
        }

        private void BuildRunsFromMergedResults(Item resultItem, IEnumerable<Range<Int32>> mergedResult)
        {
            var currentIndex = 0;

            foreach (var range in mergedResult)
            {
                if (currentIndex < range.Start)
                {
                    // add normal run
                    var normalRun = new Run(resultItem.Name.Substring(currentIndex, (range.Start - currentIndex)));
                    Inlines.Add(normalRun);
                }

                // add highlighted run
                var highlightedRun = new Run(resultItem.Name.Substring(range.Start, (range.End - range.Start)));
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

        private void ChangeHighlight(Boolean selectedItem)
        {
            Brush foregroundBrush = null;
            Brush backgroundBrush = null;

            if (selectedItem)
            {
                foregroundBrush = new SolidColorBrush(Colors.White);
                backgroundBrush = new SolidColorBrush(Colors.Transparent);
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
    }
}
