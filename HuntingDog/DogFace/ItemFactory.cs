
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using HuntingDog.DogFace.Items;

namespace HuntingDog.DogFace
{
    public class ItemFactory
    {
        static BitmapImage imageT = CreateBitmapImage(HuntingDog.Properties.Resources.table_sql);

        static BitmapImage imageS = CreateBitmapImage(HuntingDog.Properties.Resources.scroll);

        static BitmapImage imageF = CreateBitmapImage(HuntingDog.Properties.Resources.text_formula);

        static BitmapImage imageV = CreateBitmapImage(HuntingDog.Properties.Resources.text_align_center);

        static BitmapImage imageRightBlue = CreateBitmapImage(HuntingDog.Properties.Resources.arrow_right_blue);

        static BitmapImage imageRightGreen = CreateBitmapImage(HuntingDog.Properties.Resources.arrow_right_green);

        static BitmapImage imageRow = CreateBitmapImage(HuntingDog.Properties.Resources.row);

        static BitmapImage imageWrench = CreateBitmapImage(HuntingDog.Properties.Resources.wrench);

        static BitmapImage imageDb1 = CreateBitmapImage(HuntingDog.Properties.Resources.database);

        static BitmapImage imageSer = CreateBitmapImage(HuntingDog.Properties.Resources.computer);

        static BitmapImage imageSearch = CreateBitmapImage(HuntingDog.Properties.Resources.search);

        static BitmapImage imageRightArrow = CreateBitmapImage(HuntingDog.Properties.Resources.next);

        static BitmapImage imageEdit = CreateBitmapImage(HuntingDog.Properties.Resources.edit);

        static BitmapImage imageProcess = CreateBitmapImage(HuntingDog.Properties.Resources.process);

        static BitmapImage imagePageEdit = CreateBitmapImage(HuntingDog.Properties.Resources.page_edit);

        static BitmapImage imageForwardBlue = CreateBitmapImage(HuntingDog.Properties.Resources.forward_blue);

        static BitmapImage imageFoot = CreateBitmapImage(HuntingDog.Properties.Resources.footprint);

        static BitmapImage imageWorkplace = CreateBitmapImage(HuntingDog.Properties.Resources.workplace2);

        private static BitmapImage CreateBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static List<Item> BuildFromEntries(IEnumerable<HuntingDog.DogEngine.Entity> sourceList)
        {
            var items = new List<Item>();

            foreach (var source in sourceList)
            {
                var uiEntry = new Item() { Name = source.FullName, Entity = source };

                uiEntry.Keywords = source.Keywords;

                if (source.IsTable)
                {
                    uiEntry.Image = imageT;
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageRightArrow,
                        Description = "Select Data",
                        Tooltip = "Select Data from Table",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.SelectFromTable(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageProcess,
                        Description = "Edit Data",
                        Tooltip = "Edit Table Data",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.EditTableData(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageWrench,
                        Description = "Design Table",
                        Tooltip = "Design Table",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.DesignTable(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageEdit,
                        Description = "Script Table",
                        Tooltip = "Script Table",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ScriptTable(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.MainObjectTooltip = "Enter or Double Click to Select from Table";
                }
                else if (source.IsProcedure)
                {
                    uiEntry.Image = imageS;
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageEdit,
                        Description = "Modify",
                        Tooltip = "Modify Stored Procedure",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyProcedure(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imagePageEdit,
                        Description = "Execute",
                        Tooltip = "Execute Stored Procedure",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ExecuteProcedure(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.MainObjectTooltip = "Enter or Double Click to Modify Procedure";
                }
                else if (source.IsView)
                {
                    uiEntry.Image = imageV;
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageRightArrow,
                        Description = "Select Data",
                        Tooltip = "Select Data from View",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.SelectFromView(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageWrench,
                        Description = "Modify View",
                        Tooltip = "Design View",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyView(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.MainObjectTooltip = "Enter or Double Click to Select from View";
                }
                else
                {
                    uiEntry.Image = imageF;
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imageEdit,
                        Description = "Modify",
                        Tooltip = "Modify Function",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyFunction(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                    {
                        Image = imagePageEdit,
                        Description = "Execute",
                        Tooltip = "Execute Function",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ExecuteFunction(selectedServer, uiEntry.Entity);
                        }
                    });
                    uiEntry.MainObjectTooltip = "Enter or Double Click to Modify Function";
                }

                uiEntry.Actions.Add(new HuntingDog.DogFace.Items.Action
                {
                    Image = imageForwardBlue,
                    Description = "Locate",
                    Tooltip = "Locate Object",
                    Routine = (studioController, selectedServer) =>
                    {
                        studioController.NavigateObject(selectedServer, uiEntry.Entity);
                    }
                });

                items.Add(uiEntry);
            }

            return items;
        }

        public static List<Item> BuildDatabase(IEnumerable<String> sources)
        {
            var res = new List<Item>();

            foreach (var dbName in sources)
            {
                res.Add(new Item() { Name = dbName, Image = imageDb1 });
            }

            return res;
        }

        public static List<Item> BuildServer(IEnumerable<String> sources)
        {
            var res = new List<Item>();

            foreach (var srvName in sources)
            {
                res.Add(new Item() { Name = srvName, Image = imageWorkplace });
            }

            res.Sort((x, y) => String.Compare(x.Name, y.Name));

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
                {
                    viewParam.Out = "OUT";
                }

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

        internal static List<FunctionParamItem> BuildProcedureParmeters(List<DogEngine.FunctionParameter> funcParameters)
        {
            return funcParameters.ConvertAll<FunctionParamItem>(x => new FunctionParamItem() { Name = x.Name, Type = x.Type });
        }

        internal static List<ViewParamItem> BuildViewColumns(List<DogEngine.TableColumn> columns)
        {
            return columns.ConvertAll<ViewParamItem>(x => new ViewParamItem()
            {
                Name = x.Name,
                Type = x.Type,
                IsForeignKey = x.IsForeignKey,
                IsPrimaryKey = x.IsPrimaryKey
            });
        }
    }
}
