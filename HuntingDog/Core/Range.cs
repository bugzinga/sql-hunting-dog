
using System;
using System.Collections.Generic;
using System.Linq;

namespace HuntingDog.Core
{
    public class Range<T> where T : IComparable
    {
        private static readonly Log log = LogFactory.GetLog();

        public T Start
        {
            get;
            set;
        }

        public T End
        {
            get;
            set;
        }

        public static IEnumerable<Range<T>> Merge(IEnumerable<Range<T>> ranges)
        {
            List<Range<T>> mergedList = new List<Range<T>>();

            if ((ranges == null) || ranges.IsEmpty())
            {
                return mergedList;
            }

            var analyzer = new PerformanceAnalyzer();

            // TODO: This will fail if some element from the list is null.
            //       C# collections generally accepts nulls as their elements.
            //       If you decided to make this method generic, I firmly believe
            //       you should think through all possible cases then.
            List<Range<T>> orderdList = ranges.OrderBy(range => range.Start).ToList();

            T start = orderdList[0].Start;
            T end = orderdList[0].End;

            foreach (var item in orderdList.Skip(1))
            {
                // TODO: Start and End properties are of the generic T type which means
                //       they can have null values. The code can be broken in such a case.
                //       It concerns both the code belows and the sorting routine above.
                if ((item.End.CompareTo(end) > 0) && (item.Start.CompareTo(end) > 0))
                {
                    mergedList.Add(new Range<T> { Start = start, End = end });
                    start = item.Start;
                }
                
                end = (end.CompareTo(item.End) > 0)
                    ? end
                    : item.End;
            }

            mergedList.Add(new Range<T> { Start = start, End = end });

            analyzer.Stop();

            return mergedList;
        }
    }
}
