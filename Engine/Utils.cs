using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringUtils
{

    public class Utils
    {
        public const string OpenTag = "<b>";
        public const string CloseTag = "</b>";

        class Range : IComparable
        {
            public int StartPos { get; set; }
            public int EndPos { get; set; }

            public bool IntersectsOrConsequitive(Range another)
            {
                if (Intersects(another))
                    return true;

                return (another.EndPos == this.StartPos - 1) || (another.StartPos == this.EndPos + 1);
            }

            public bool Intersects(Range another)
            {
                return Intersects(another.StartPos, another.EndPos);
            }

            public bool Intersects(int start, int end)
            {
                return !(EndPos < start || StartPos > end);
            }

            public int CompareTo(object obj)
            {
                Range other = (Range)obj;
                if (other.StartPos == this.StartPos)
                    return 0;

                return this.StartPos < other.StartPos ? -1 : 1;
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
                    mergedIn = Merge(mergedIn);
            }

            Range Merge(Range newRange)
            {
                foreach (var r in ranges)
                {
                    if (r != newRange && r.IntersectsOrConsequitive(newRange))
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


        static public string ReplaceString(string strInput, string[] boldSubstr)
        {
            var tags = new RangeList();

            string result = strInput;


            var lowerInput = strInput.ToLower();
            foreach (var bs in boldSubstr)
            {
                int firstIndex = 0;
                while (true)
                {
                    int startIndex = lowerInput.IndexOf(bs,firstIndex, StringComparison.OrdinalIgnoreCase);
                    if (startIndex != -1)
                    {
                        tags.Add(new Range() { StartPos = startIndex, EndPos = startIndex + bs.Length - 1 });
                        firstIndex = startIndex + bs.Length;
                    }
                    else
                        break;
                }
            }

            // there could be done some optimisation - in order to merge consequitive ranges and overlapping ranges...
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
