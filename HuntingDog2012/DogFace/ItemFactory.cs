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

        //static BitmapImage imageDb1 = new BitmapImage(new Uri(@"Images/server.png", UriKind.Relative));
        //static BitmapImage imageSer = new BitmapImage(new Uri(@"Images/cpu.png", UriKind.Relative));

        static BitmapImage imageDb1 = new BitmapImage(new Uri(@"Art/database.png", UriKind.Relative));
        static BitmapImage imageSer = new BitmapImage(new Uri(@"Art/computer.png", UriKind.Relative));

        static BitmapImage imageSearch = new BitmapImage(new Uri(@"Art/search.png", UriKind.Relative));

        static BitmapImage imageRightArrow = new BitmapImage(new Uri(@"Art/next.png", UriKind.Relative));
        static BitmapImage imageEdit = new BitmapImage(new Uri(@"Art/edit.png", UriKind.Relative));
        static BitmapImage imageProcess = new BitmapImage(new Uri(@"Art/process.png", UriKind.Relative));
        static BitmapImage imagePageEdit = new BitmapImage(new Uri(@"Art/page_edit.png", UriKind.Relative));

        static BitmapImage imageForwardBlue = new BitmapImage(new Uri(@"Art/forward_blue.png", UriKind.Relative));


        static BitmapImage imageFoot = new BitmapImage(new Uri(@"Resources/footprint.bmp", UriKind.Relative));

        static BitmapImage imageWorkplace = new BitmapImage(new Uri(@"Images/workplace2.png", UriKind.Relative));

        public static List<Item> BuildFromEntries(IEnumerable<HuntingDog.DogEngine.Entity> sourceList)
        {


            var res = new List<Item>();
            foreach (var source in sourceList)
            {
                var uiEntry = new Item() { Name = source.FullName, Entity = source };
                uiEntry.Action3Visibility = System.Windows.Visibility.Collapsed;
                uiEntry.Action4Visibility = System.Windows.Visibility.Collapsed;

                //uiEntry.Action3 = imageProcess;
                // uiEntry.Action3Description = "  ";

                if (source.IsTable)
                {
                    uiEntry.Image = imageT;

                    uiEntry.Action1 = imageRightArrow;
                    uiEntry.Action1Description = "Select";
                    uiEntry.Action1Tooltip = "Select Data from Table";

                    uiEntry.Action2 = imageProcess;
                    uiEntry.Action2Description = "Edit";
                    uiEntry.Action2Tooltip = "Edit Table Data";

                    uiEntry.Action3Visibility = System.Windows.Visibility.Visible;
                    uiEntry.Action3 = imageWrench;
                    uiEntry.Action3Description = "Design";
                    uiEntry.Action3Tooltip = "Design Table";

                    uiEntry.MainObjectTooltip = "Enter or Double Click to Select from Table.";
                }
                else if (source.IsProcedure)
                {
                    uiEntry.Image = imageS;
                    uiEntry.Action1 = imageEdit;
                    uiEntry.Action1Description = "Modify";
                    uiEntry.Action1Tooltip = "Modify Stored Procedure";

                    uiEntry.Action2 = imagePageEdit;
                    uiEntry.Action2Description = "Execute";
                    uiEntry.Action2Tooltip = "Execute Stored Proc";

                    uiEntry.MainObjectTooltip = "Enter or Double Click to Modify Procedure.";
                }
                else if (source.IsView)
                {
                    uiEntry.Image = imageV;
                    uiEntry.Action1 = imageRightArrow;
                    uiEntry.Action1Description = "Select";
                    uiEntry.Action1Tooltip = "Select Data from View";

                    uiEntry.Action2 = imageWrench;
                    uiEntry.Action2Description = "Design";
                    uiEntry.Action2Tooltip = "Design View";

                    uiEntry.MainObjectTooltip = "Enter or Double Click to Select from View.";
                }
                else
                {
                    uiEntry.Image = imageF;
                    uiEntry.Action1 = imageEdit;
                    uiEntry.Action1Description = "Modify";
                    uiEntry.Action1Tooltip = "Modify Function";

                    uiEntry.Action2 = imagePageEdit;
                    uiEntry.Action2Description = "Execute";
                    uiEntry.Action2Tooltip = "Execute Function";

                    uiEntry.MainObjectTooltip = "Enter or Double Click to Modify Function.";
                }

                uiEntry.Action2 = imageForwardBlue;


                res.Add(uiEntry);
            }


            return res;
        }

    

        public static List<Item> BuildDatabase(IEnumerable<string> sources)
        {
            var res = new List<Item>();
            foreach (var dbName in sources)
            {
                res.Add(new Item() { Name = dbName, Image = imageDb1 });
            }
            return res;
        }

        public static List<Item> BuildServer(IEnumerable<string> sources)
        {
            var res = new List<Item>();
            foreach (var srvName in sources)
            {
                res.Add(new Item() { Name = srvName, Image = imageWorkplace });
            }

            res.Sort((x,y) => string.Compare(x.Name,y.Name));

            return res;
        }

     

   


        public static List<ProcedureParamItem> BuildProcedureParmeters(IEnumerable<HuntingDog.DogEngine.ProcedureParameter> paramList)
        {
            var res = new List<ProcedureParamItem>();
            foreach (var par in paramList)
            {
                var viewParam = new ProcedureParamItem();
                viewParam.Name = par.Name;
                viewParam.Type = par.Type;
                if (par.IsOut)
                    viewParam.Out = "OUT";
                res.Add(viewParam);
            }

            return res;
        }


        internal static List<TableParamItem> BuildTableColumns(List<DogEngine.TableColumn> columns)
        {
            var res = new List<TableParamItem>();
            foreach (var par in columns)
            {
                var viewParam = new TableParamItem();
                viewParam.Name = par.Name;
                viewParam.Type = par.Type;

                viewParam.IsPrimaryKey = par.IsPrimaryKey;
                viewParam.IsForeignKey = par.IsForeignKey;    
                res.Add(viewParam);
            }

            return res;
        }




        internal static List<FuncParamItem> BuildProcedureParmeters(List<DogEngine.FuncParameter> funcParameters)
        {
            return funcParameters.ConvertAll<FuncParamItem>(x => new FuncParamItem() { Name = x.Name, Type = x.Type });
                
        }

        internal static List<ViewParamItem> BuildViewColumns(List<DogEngine.TableColumn> columns)
        {
            return columns.ConvertAll<ViewParamItem>(x => new ViewParamItem() { 
                Name = x.Name,
                Type = x.Type, 
                IsForeignKey = x.IsForeignKey, 
                IsPrimaryKey = x.IsPrimaryKey });
        }
    }
}
