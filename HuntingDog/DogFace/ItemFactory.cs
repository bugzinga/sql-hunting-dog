
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using HuntingDog.Core;
using HuntingDog.DogEngine;
using HuntingDog.Properties;

namespace HuntingDog.DogFace
{
    public static class ItemFactory
    {
        private static Log log = LogFactory.GetLog();

        private static BitmapImage TableIcon = Resources.table_sql.ToBitmapImage();

        private static BitmapImage StoredProcedureIcon = Resources.scroll.ToBitmapImage();

        private static BitmapImage FunctionIcon = Resources.text_formula.ToBitmapImage();

        private static BitmapImage ViewIcon = Resources.text_align_center.ToBitmapImage();

        private static BitmapImage DatabaseIcon = Resources.database.ToBitmapImage();

        private static BitmapImage ComputerIcon = Resources.workplace2.ToBitmapImage();

        public static List<Item> BuildFromEntries(IEnumerable<Entity> entities)
        {
            var items = new List<Item>();

            foreach (var entity in entities)
            {
                var item = new Item
                {
                    Name = entity.FullName,
                    Entity = entity
                };

                item.Keywords = entity.Keywords;

                if (entity.IsTable)
                {
                    item.Image = TableIcon;
                    item.Actions.Add(new Action
                    {
                        Name = "Select Data",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.SelectFromTable(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Edit Data",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.EditTableData(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Design Table",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.DesignTable(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Script Table",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ScriptTable(selectedServer, item.Entity);
                        }
                    });
                    item.MainObjectTooltip = "Enter or Double Click to Select from Table";
                }
                else if (entity.IsProcedure)
                {
                    item.Image = StoredProcedureIcon;
                    item.Actions.Add(new Action
                    {
                        Name = "Modify",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyProcedure(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Execute",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ExecuteProcedure(selectedServer, item.Entity);
                        }
                    });
                    item.MainObjectTooltip = "Enter or Double Click to Modify Procedure";
                }
                else if (entity.IsView)
                {
                    item.Image = ViewIcon;
                    item.Actions.Add(new Action
                    {
                        Name = "Select Data",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.SelectFromView(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Modify View",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyView(selectedServer, item.Entity);
                        }
                    });
                    item.MainObjectTooltip = "Enter or Double Click to Select from View";
                }
                else if (entity.IsFunction)
                {
                    item.Image = FunctionIcon;
                    item.Actions.Add(new Action
                    {
                        Name = "Modify",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ModifyFunction(selectedServer, item.Entity);
                        }
                    });
                    item.Actions.Add(new Action
                    {
                        Name = "Execute",
                        Routine = (studioController, selectedServer) =>
                        {
                            studioController.ExecuteFunction(selectedServer, item.Entity);
                        }
                    });
                    item.MainObjectTooltip = "Enter or Double Click to Modify Function";
                }
                else
                {
                    log.Error(String.Format("Cannot determine the type of the '{0}' entity", entity.Name));
                }

                item.Actions.Add(new Action
                {
                    Name = "Locate",
                    Routine = (studioController, selectedServer) =>
                    {
                        studioController.NavigateObject(selectedServer, item.Entity);
                    }
                });

                items.Add(item);
            }

            return items;
        }

        public static List<Item> BuildDatabase(IEnumerable<String> databaseNames)
        {
            var items = new List<Item>();

            foreach (var databaseName in databaseNames)
            {
                items.Add(new Item
                {
                    Name = databaseName,
                    Image = DatabaseIcon
                });
            }

            items.Sort((x, y) => String.Compare(x.Name, y.Name));

            return items;
        }

        public static List<Item> BuildServer(IEnumerable<IServer> serverNames)
        {
            var items = new List<Item>();

            foreach (var server in serverNames)
            {
                items.Add(new Item
                {
                    Name = server.ServerName,
                    Image = ComputerIcon,
                    Server = server
                });
            }

            items.Sort((x, y) => String.Compare(x.Name, y.Name));

            return items;
        }
    }
}
