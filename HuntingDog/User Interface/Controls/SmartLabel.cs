using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace WinForms.test
{
    public class SmartLabel:DevExpress.XtraEditors.LabelControl
    {
        class Substring
        {
            internal static Substring ExtractFromText(string text, int startpos, int endpos, bool bold)
            {
                return new Substring(){ StartPos = startpos, EndPos = endpos, Bold = bold, Text = text.Substring(startpos,endpos - startpos+1)};
            }

            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public string Text{get;set;}
            public bool Bold{get;set;}
        }

        List<Substring> FormattedString = new List<Substring>();

        public static readonly string BoldOpenTag = "<b>";
        public static readonly string BoldCloseTag = "</b>";
        Brush b = new SolidBrush(Color.Black);

        Font _boldFond = null;
        Font GetBoldFond()
        {
            if (_boldFond != null)
                return _boldFond;

            _boldFond = new Font(Font, FontStyle.Bold);
            return _boldFond;
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                RecalcFormat();
            }
        }

        private void RecalcFormat()
        {
            FormattedString.Clear();

            int currentPos = 0;
            while (currentPos < Text.Length)
            {
                var startTage = Text.IndexOf(BoldOpenTag, currentPos);
                if (startTage == -1)
                {
                    // add tail non bold string
                    FormattedString.Add(Substring.ExtractFromText(Text, currentPos, Text.Length - 1, false));
                    break;
                }

                if (startTage > currentPos)
                {
                    // read non bold string
                    FormattedString.Add(Substring.ExtractFromText(Text, currentPos, startTage - 1, false));
                    currentPos = startTage;
                }
                else
                {
                    var endTage = Text.IndexOf(BoldCloseTag, startTage);
                    if (endTage == -1)
                    {
                        // error - no end tag

                        // read non bold string
                        FormattedString.Add(Substring.ExtractFromText(Text, currentPos, startTage + BoldOpenTag.Length, false));
                        currentPos = startTage + BoldOpenTag.Length;
                    }
                    else
                {
                        // create Bold substring
                        FormattedString.Add(Substring.ExtractFromText(Text, startTage + BoldOpenTag.Length, endTage-1, true));
                        currentPos = endTage + BoldCloseTag.Length;
                    }
                }

            }          

        }

        static float  GetOffset(Font font)
        {
            if(font.Size >= 8 || font.Size <= 8.25)
                return 3.5F;

            if (font.Size < 8)
                return 2;

            return 3;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float offset = 0;

            foreach (var sub in FormattedString)
            {
                //TextFormatFlags fl = TextFormatFlags.LeftAndRightPadding;
               Font fontToUse = sub.Bold? GetBoldFond(): Font;
             
                //offset += MeasureDisplayStringWidth2(e.Graphics, sub.Text, fontToUse);
                //var widthText = TextRenderer.MeasureText(sub.Text, fontToUse, Size.Empty, fl).Width;

                //DrawText(dc, VAW2A(text), len, &rect, DT_CALCRECT);
                var widthText = e.Graphics.MeasureString(sub.Text, fontToUse).Width - GetOffset(fontToUse);

                //TextRenderer.DrawText(e.Graphics, sub.Text, fontToUse, new Point((int)offset, 0), Color.Black, fl);

                e.Graphics.DrawString(sub.Text, fontToUse, b, offset, 0);
                
                offset += widthText;
            }

            //e.Graphics.DrawEllipse(Pens.Blue, e.ClipRectangle);
        }



        static public int MeasureDisplayStringWidth(Graphics graphics, string text,
                                            Font font)
        {
            const int width = 32;

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, 1,
                                                                        graphics);
            System.Drawing.SizeF size = graphics.MeasureString(text, font);
            System.Drawing.Graphics anagra = System.Drawing.Graphics.FromImage(bitmap);

            int measured_width = (int)size.Width;

            if (anagra != null)
            {
                anagra.Clear(Color.White);
                anagra.DrawString(text + "|", font, Brushes.Black,
                                   width - measured_width, -font.Height / 2);

                for (int i = width - 1; i >= 0; i--)
                {
                    measured_width--;
                    if (bitmap.GetPixel(i, 0).R != 255)    // found a non-white pixel ?

                        break;
                }
            }

            return measured_width;
        }


        static public int MeasureDisplayStringWidth2(Graphics graphics, string text,
                                            Font font)
        {
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            System.Drawing.RectangleF rect = new System.Drawing.RectangleF(0, 0,
                                                                          1000, 1000);
            System.Drawing.CharacterRange[] ranges = 
                                       { new System.Drawing.CharacterRange(0, 
                                                               text.Length) };
            System.Drawing.Region[] regions = new System.Drawing.Region[1];

            format.SetMeasurableCharacterRanges(ranges);

            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

    }
}
