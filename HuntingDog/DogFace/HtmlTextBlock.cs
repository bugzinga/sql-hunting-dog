
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HuntingDog.Core;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public class HtmlTextBlock : TextBlock
    {
        protected readonly Log log = LogFactory.GetLog();

        private List<Run> runs = new List<Run>();

        public static DependencyProperty ResultItemProperty = DependencyProperty.Register(
            "ResultItem",
            typeof(IHighlightedItem),
            typeof(HtmlTextBlock),
            new UIPropertyMetadata(null, new PropertyChangedCallback(OnResultItemChanged))
        );

        public static DependencyProperty DoHighlighProperty = DependencyProperty.Register(
            "DoHighlight",
            typeof(Boolean),
            typeof(HtmlTextBlock),
            new UIPropertyMetadata(true, new PropertyChangedCallback(OnHighlightChanged))
        );

        public IHighlightedItem ResultItem
        {
            get
            {
                return (IHighlightedItem)GetValue(ResultItemProperty);
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
                htmlTextBlock.DisplayResultItem(htmlTextBlock.ResultItem.Name, htmlTextBlock.ResultItem.Keywords);
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

        private void AddInline(string text)
        {
            Inlines.Add(new Run(text));
        }

        private void DisplayResultItem(string name,IEnumerable<String> keywords)
        {
            log.Info("Updating item UI ranges");
            var analyzer = new PerformanceAnalyzer();

            Inlines.Clear();
            runs.Clear();

            if ((keywords == null) || keywords.IsEmpty())
            {
                // TODO: get rid of code duplication (lines 89, 106, 116)
                AddInline(name);             
            }
            else
            {
                try
                {
                    var ranges = FindMatchingKeyworkRanges(name, keywords);

                    if (ranges.Any())
                    {
                        var mergedResult = Range<Int32>.Merge(ranges);
                        BuildRunsFromMergedResults(name, mergedResult);
                    }
                    else
                    {                    
                        AddInline(name);
     
                    }
                }
                // TODO: What is it for? How can any exception happen here?
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);

                 
                    AddInline(name);

                }
            }

            log.Performance("Item UI ranges updated", analyzer.Result);
            analyzer.Stop();
        }

        private List<Range<Int32>> FindMatchingKeyworkRanges(string name, IEnumerable<string> keywords)
        {
            var ranges = new List<Range<Int32>>();

            var upperCaseName = name.ToUpper();

            log.Info(String.Format("Looking for keyword ranges: item = {0}, keywords = [ {1} ]", name, String.Join(", ", keywords.ToArray())));
            var analyzer = new PerformanceAnalyzer();

            foreach (var keyword in keywords)
            {
                var startIndex = 0;

                while ((startIndex = upperCaseName.IndexOf(keyword, startIndex)) != -1)
                {
                    int endIndex = (startIndex + keyword.Length);

                    // TODO: How is it possible?
                    if (endIndex > upperCaseName.Length)
                    {
                        endIndex = upperCaseName.Length;
                    }

                    ranges.Add(new Range<Int32>() { Start = startIndex++, End = endIndex });
                }
            }

            log.Performance("Search time", analyzer.Result);
            analyzer.Stop();

            return ranges;
        }

        private void BuildRunsFromMergedResults(string name, IEnumerable<Range<Int32>> mergedResult)
        {
            var currentIndex = 0;

            foreach (var range in mergedResult)
            {
                if (currentIndex < range.Start)
                {
                    // add normal run
                    AddInline(name.Substring(currentIndex, (range.Start - currentIndex)));
                }

                // add highlighted run
                var highlightedRun = new Run(name.Substring(range.Start, (range.End - range.Start)));
                Inlines.Add(highlightedRun);
                runs.Add(highlightedRun);

                currentIndex = range.End;
            }

            if (currentIndex < name.Length)
            {
                // add final normal run if necessary
                AddInline(name.Substring(currentIndex));
            }
        }

        static SolidColorBrush SelectedForeground = new SolidColorBrush(Colors.White);
        static SolidColorBrush SelectedBackground = new SolidColorBrush(Colors.Transparent);
        static SolidColorBrush NotSelectedForeground = new SolidColorBrush(Colors.Black);
        static SolidColorBrush NotSelectedBackground = new SolidColorBrush(Colors.Yellow);


        private void ChangeHighlight(Boolean selectedItem)
        {
            Brush foregroundBrush = null;
            Brush backgroundBrush = null;

            // TODO: Cache all brushes!!!!
            if (selectedItem)
            {
                foregroundBrush = SelectedForeground;
                backgroundBrush = SelectedBackground;
            }
            else
            {
                foregroundBrush = NotSelectedBackground;
                backgroundBrush = NotSelectedBackground;
            }

            foreach (var run in runs)
            {
                run.Foreground = selectedItem ? SelectedForeground : NotSelectedForeground;
                run.Background = selectedItem? SelectedBackground:NotSelectedBackground;
            }
        }
    }
}
