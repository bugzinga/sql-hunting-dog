
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuntingDog.Core
{
    public class Range<T> where T : IComparable
    {
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

        public static IEnumerable<Range<T>> Merge(IEnumerable<Range<T>> me)
        {
            List<Range<T>> orderdList = me.OrderBy(r => r.Start).ToList();
            List<Range<T>> mergedList = new List<Range<T>>();

            T max = orderdList[0].End;
            T min = orderdList[0].Start;

            foreach (var item in orderdList.Skip(1))
            {
                if ((item.End.CompareTo(max) > 0) && (item.Start.CompareTo(max) > 0))
                {
                    mergedList.Add(new Range<T> { Start = min, End = max });
                    min = item.Start;
                }
                
                max = (max.CompareTo(item.End) > 0)
                    ? max
                    : item.End;
            }

            mergedList.Add(new Range<T> { Start = min, End = max });

            return mergedList;
        }
    }
}
