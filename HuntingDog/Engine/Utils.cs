
using System;
using System.Collections.Generic;

namespace StringUtils
{
    public class Utils
    {
        public const String OpenTag = "<b>";

        public const String CloseTag = "</b>";

        class Range : IComparable
        {
            public Int32 StartPos
            {
                get;
                set;
            }

            public Int32 EndPos
            {
                get;
                set;
            }

            public Boolean IntersectsOrConsequitive(Range another)
            {
                if (Intersects(another))
                {
                    return true;
                }

                return (another.EndPos == (StartPos - 1)) || (another.StartPos == (EndPos + 1));
            }

            public Boolean Intersects(Range another)
            {
                return Intersects(another.StartPos, another.EndPos);
            }

            public Boolean Intersects(Int32 start, Int32 end)
            {
                return !((EndPos < start) || (StartPos > end));
            }

            public Int32 CompareTo(Object obj)
            {
                Range other = (Range) obj;

                if (other.StartPos == StartPos)
                {
                    return 0;
                }

                return (StartPos < other.StartPos)
                    ? -1
                    : 1;
            }
        }

        class RangeList
        {
            List<Range> ranges = new List<Range>();

            public List<Range> GetSortedRanges()
            {
                ranges.Sort();
                return ranges;
            }

            public void Add(Range newRange)
            {
                ranges.Add(newRange);
                var mergedIn = Merge(newRange);

                while (mergedIn != null)
                {
                    mergedIn = Merge(mergedIn);
                }
            }

            private Range Merge(Range newRange)
            {
                foreach (var r in ranges)
                {
                    if ((r != newRange) && r.IntersectsOrConsequitive(newRange))
                    {
                        r.StartPos = Math.Min(r.StartPos, newRange.StartPos);
                        r.EndPos = Math.Max(r.EndPos, newRange.EndPos);
                        ranges.Remove(newRange);
                        return r;
                    }
                }

                return null;
            }
        }

        public static String ReplaceString(String strInput, String[] boldSubstr)
        {
            var tags = new RangeList();
            var result = strInput;
            var lowerInput = strInput.ToLower();

            foreach (var bs in boldSubstr)
            {
                int firstIndex = 0;

                while (true)
                {
                    int startIndex = lowerInput.IndexOf(bs, firstIndex, StringComparison.OrdinalIgnoreCase);

                    if (startIndex != -1)
                    {
                        tags.Add(new Range() { StartPos = startIndex, EndPos = (startIndex + bs.Length - 1) });
                        firstIndex = startIndex + bs.Length;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // there could be done some optimization - in order to merge consecutive ranges and overlapping ranges...
            int offset = 0;

            foreach (var range in tags.GetSortedRanges())
            {
                result = result.Insert(offset + range.StartPos, OpenTag);
                offset += OpenTag.Length;
                result = result.Insert(offset + range.EndPos + 1, CloseTag);
                offset += CloseTag.Length;
            }

            return result;
        }
    }
}
