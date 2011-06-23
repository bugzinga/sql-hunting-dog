using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace HuntingDog.DogFace
{
    public class ItemFactory
    {
        static BitmapImage imageT = new BitmapImage(new Uri(@"Images/table_sql.png", UriKind.Relative));
        static BitmapImage imageS = new BitmapImage(new Uri(@"Images/scroll.png", UriKind.Relative));
        static BitmapImage imageF = new BitmapImage(new Uri(@"Images/text_formula.png", UriKind.Relative));
        static BitmapImage imageV = new BitmapImage(new Uri(@"Images/text_align_center.png", UriKind.Relative));


        static BitmapImage imageRightBlue = new BitmapImage(new Uri(@"Images/arrow_right_blue.png", UriKind.Relative));
        static BitmapImage imageRightGreen = new BitmapImage(new Uri(@"Images/arrow_right_green.png", UriKind.Relative));
        static BitmapImage imageRow = new BitmapImage(new Uri(@"Images/row.png", UriKind.Relative));
        static BitmapImage imageWrench = new BitmapImage(new Uri(@"Images/wrench.png", UriKind.Relative));

        public static List<Item> BuildFromEntries(IEnumerable<HuntingDog.DogEngine.Entity> sourceList)
        {
        

            var res = new List<Item>();
            foreach (var source in sourceList)
            {
                var uiEntry = new Item() { Name = source.Name, Entity = source };
                if (source.IsTable)
                {
                    uiEntry.Image = imageT;
                    uiEntry.Action = imageRightBlue;
                }
                else if (source.IsProcedure)
                {
                    uiEntry.Image = imageS;
                    uiEntry.Action = imageRightGreen;
                }
                else if (source.IsView)
                {
                    uiEntry.Image = imageV;
                    uiEntry.Action = imageRightGreen;
                }
                else
                {
                    uiEntry.Image = imageF;
                   uiEntry.Action = imageRightGreen;
                }

                res.Add(uiEntry);
            }


            return res;
        }

    }
}
