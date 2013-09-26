using System;
using System.IO;
using System.ComponentModel;

namespace HuntingDog.Config
{
    public enum EAlterOrCreate
    {
        Alter,
        Create
    }

    public class DogConfig 
    {
        private int _selectTopXTable;
        private int _selectTopXView;

        public DogConfig()
        {
            ScriptIndexies = true;
            ScriptTriggers = true;
            ScriptForeignKeys = false;

            SelectTopXTable = 200;
            SelectTopXView = 200;

            AlterOrCreateSp = EAlterOrCreate.Alter;
            AlterOrCreateFunction = EAlterOrCreate.Alter;
            AlterOrCreateView = EAlterOrCreate.Alter;

            IncludeAllCoulumnNamesForTables = false;
        }

        public void Persist(DogEngine.IStorage storage)
        {
            storage.StoreByName("ScriptIndexies", ScriptIndexies.ToString());
            storage.StoreByName("ScriptTriggers", ScriptTriggers.ToString());
            storage.StoreByName("ScriptForeignKeys", ScriptForeignKeys.ToString());

            storage.StoreByName("SelectTopXTable", SelectTopXTable.ToString());
            storage.StoreByName("SelectTopXView", SelectTopXView.ToString());

            storage.StoreByName("AlterOrCreateSp", AlterOrCreateSp.ToString());
            storage.StoreByName("AlterOrCreateFunction", AlterOrCreateFunction.ToString());
            storage.StoreByName("AlterOrCreateView", AlterOrCreateView.ToString());

            storage.StoreByName("LaunchingHotKey", LaunchingHotKey);

            storage.StoreByName("IncludeAllCoulumnNamesForTables", IncludeAllCoulumnNamesForTables.ToString());
            storage.StoreByName("IncludeAllCoulumnNamesForViews", IncludeAllCoulumnNamesForViews.ToString());
        }

        public static DogConfig ReadFromStorage(HuntingDog.DogEngine.IStorage storage)
        {
            var cfg = new DogConfig();
            cfg.Restore(storage);
            return cfg;
        }

        private void Restore(DogEngine.IStorage storage)
        {
            try
            {
                if (storage.Exists("LaunchingHotKey"))
                {
                    LaunchingHotKey = storage.GetByName("LaunchingHotKey");
                }

                if (storage.Exists("ScriptIndexies"))
                {
                    ScriptIndexies = bool.Parse(storage.GetByName("ScriptIndexies"));
                }


                if (storage.Exists("IncludeAllCoulumnNamesForTables"))
                {
                    IncludeAllCoulumnNamesForTables = bool.Parse(storage.GetByName("IncludeAllCoulumnNamesForTables"));
                }

                if (storage.Exists("IncludeAllCoulumnNamesForViews"))
                {
                    IncludeAllCoulumnNamesForViews = bool.Parse(storage.GetByName("IncludeAllCoulumnNamesForViews"));
                }



                if (storage.Exists("ScriptTriggers"))
                {
                    ScriptTriggers = bool.Parse(storage.GetByName("ScriptTriggers"));
                }

                if (storage.Exists("ScriptForeignKeys"))
                {
                    ScriptForeignKeys = bool.Parse(storage.GetByName("ScriptForeignKeys"));
                }

                if (storage.Exists("SelectTopXTable"))
                {
                    SelectTopXTable = int.Parse(storage.GetByName("SelectTopXTable"));
                }

                if (storage.Exists("SelectTopXView"))
                {
                    SelectTopXView = int.Parse(storage.GetByName("SelectTopXView"));
                }

                if (storage.Exists("AlterOrCreateSp"))
                {
                    AlterOrCreateSp = (EAlterOrCreate)Enum.Parse(typeof(EAlterOrCreate), storage.GetByName("AlterOrCreateSp"));
                }

                if (storage.Exists("AlterOrCreateView"))
                {
                    AlterOrCreateView = (EAlterOrCreate)Enum.Parse(typeof(EAlterOrCreate), storage.GetByName("AlterOrCreateView"));
                }


                if (storage.Exists("AlterOrCreateFunction"))
                {
                    AlterOrCreateFunction = (EAlterOrCreate)Enum.Parse(typeof(EAlterOrCreate), storage.GetByName("AlterOrCreateFunction"));
                }

            }
            catch (Exception)
            {

            }
        }

        [Category("Tables")]
        [DisplayName("Add Column Names to SELECT")]
        [Description("Use 'SELECT *' or 'SELECT column1, column2..' syntax'")]   
        public bool IncludeAllCoulumnNamesForTables { get; set; }

        [Category("Views")]
        [DisplayName("Add Column Names to SELECT")]
        [Description("Use 'SELECT *' or 'SELECT column1, column2..' syntax'")]   
        public bool IncludeAllCoulumnNamesForViews { get; set; }

        [Category("Tables")]
        [DisplayName("Script Indexes")]
        [Description("Include Indexes when scripting a Table")]
        public bool ScriptIndexies { get; set; }

        [Category("Tables")]
        [DisplayName("Script Triggers")]
        [Description("Include Triggers when scripting a Table")]
        public bool ScriptTriggers { get; set; }

        [Category("Tables")]
        [DisplayName("Script Foreign Keys")]
        [Description("Include Foregn Keys when scripting a Table")]
        public bool ScriptForeignKeys { get; set; }


        [Category("Tables")]
        [DisplayName("Select top X")]
        [Description("Override row count limitation when you select data from the table")]
        public int SelectTopXTable
        {
            get { return _selectTopXTable; }
            set
            {
                if(value<=0)
                    throw  new InvalidDataException("Must be greater than zero");
                _selectTopXTable = value;
            }
        }


        //[Category("General")]
        //[Description("Change Hunting Dog default font size")]   
        //public int DefaultFontSize { get; set; }

        private string _launchingHotKey = "D";
        [Category("General")]
        [DisplayName("Hot Key: Ctrl+")]
        [Description("Launch Hunting Dog using Ctrl + this key. Will be effective after SSMS is restarted")]
        public string LaunchingHotKey
        {
            get { return _launchingHotKey; }
            set
            {
                if (value.Length != 1)
                    throw new InvalidDataException("Must be one character");
                _launchingHotKey = value;
            }
        }

        [Category("Views")]
        [DisplayName("Select top X ")]
        [Description("Override row count limitation when you select data from the view")]
        public int SelectTopXView
        {
            get { return _selectTopXView; }
            set
            {
                if (value <= 0)
                    throw new InvalidDataException("Must be greater than zero");
                _selectTopXView = value;
            }
        }


        [Category("Views")]
        [DisplayName("Inspect Body using")]
        [Description("When inspecting View body use ALTER or CREATE script")]
        public EAlterOrCreate AlterOrCreateView { get; set; }

        [Category("Stored Procedures")]
        [DisplayName("Inspect Body using")]
        [Description("When inspecting Procedures body use ALTER or CREATE script")]
        public EAlterOrCreate AlterOrCreateSp { get; set; }

        [Category("Functions")]
        [DisplayName("Inspect Body using")]
        [Description("When inspecting Functions body use ALTER or CREATE script")]
        public EAlterOrCreate AlterOrCreateFunction { get; set; }

   


        public DogConfig CloneMe()
        {
            return (DogConfig)this.MemberwiseClone();
        }
    }
}
