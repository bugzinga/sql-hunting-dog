using System;
using System.ComponentModel;
using System.IO;

namespace HuntingDog.Config
{
    public enum EAlterOrCreate
    {
        Alter,
        Create
    }

    public enum EOrderBy
    {
        None,
        Ascending,
        Descending
    }

    public class DogConfig
    {
        private int _selectTopXTable;
        private int _limitSearch;
        private int _fontSize;

        public DogConfig()
        {
            FontSize = 14;

            ScriptIndexes = true;
            ScriptTriggers = true;
            ScriptForeignKeys = false;

            SelectTopX = 200;

            AddNoLock = false;
            IncludeAllColumns = true;
            AddWhereClauseFor = false;
            OrderBy = EOrderBy.Descending;

            AlterOrCreate = EAlterOrCreate.Create;

            LimitSearch = 500;

            HideAfterAction = false;
        }

        [Category("SELECT")]
        [DisplayName("Add Column Names to SELECT")]
        [Description("Use 'SELECT *' or 'SELECT column1, column2..' syntax'")]
        public bool IncludeAllColumns { get; set; }

        [Category("SCRIPT")]
        [DisplayName("Script Indexes")]
        [Description("Include Indexes when scripting a Table")]
        public bool ScriptIndexes { get; set; }

        [Category("SCRIPT")]
        [DisplayName("Script Triggers")]
        [Description("Include Triggers when scripting a Table")]
        public bool ScriptTriggers { get; set; }

        [Category("SCRIPT")]
        [DisplayName("Script Foreign Keys")]
        [Description("Include Foregn Keys when scripting a Table")]
        public bool ScriptForeignKeys { get; set; }

        [Category("SELECT")]
        [DisplayName("Add WHERE Caluse")]
        [Description("Add commented WHERE clause that includes all columns and their types")]
        public bool AddWhereClauseFor { get; set; }

        [Category("SELECT")]
        [DisplayName("Add WITH(NOLOCK) Hint")]
        [Description("Add NOLOCK hint. Can lead to dirty or inconsistent data to be presented")]
        public bool AddNoLock { get; set; }

        [Category("SELECT")]
        [DisplayName("User ORDER BY")]
        [Description("Add Order by Primary Key(s) at the end of select statement")]
        public EOrderBy OrderBy { get; set; }

        [Category("SELECT")]
        [DisplayName("Select top X")]
        [Description("Override row count limitation when you select data from the table or view")]
        public int SelectTopX
        {
            get { return _selectTopXTable; }
            set
            {
                if (value <= 0)
                    throw new InvalidDataException("Must be greater than zero");
                _selectTopXTable = value;
            }
        }

        [Category("GENERAL")]
        [DisplayName("Search Limit")]
        [Description("Retrieve only first X objects")]
        public int LimitSearch
        {
            get { return _limitSearch; }
            set
            {
                if (value <= 0)
                    throw new InvalidDataException("Must be greater than zero");
                _limitSearch = value;
            }
        }

        [Category("GENERAL")]
        [DisplayName("Font Size")]
        [Description("Requires SSMS restart. Font size used for search results.")]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                if (_fontSize < 8)
                    _fontSize = 8;
                else if (_fontSize > 16)
                    _fontSize = 16;
            }
        }

        private string _launchingHotKey = "D";

        [Category("GENERAL")]
        [DisplayName("Hot Key: Ctrl+")]
        [Description("Requires SSMS restart.Launch Hunting Dog using Ctrl + this key.")]
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

        private bool _hideAfterAction = false;

        [Category("GENERAL")]
        [DisplayName("Hide window after action")]
        [Description("Hide Hunting Dog window after action is completed.")]
        public bool HideAfterAction
        {
          get { return _hideAfterAction; }
          set
          {
            _hideAfterAction = value;
          }
        }
  
        [Category("MODIFY")]
        [DisplayName("Inspect Body using")]
        [Description("When inspecting Procedure, View or Function body use ALTER or CREATE script")]
        public EAlterOrCreate AlterOrCreate { get; set; }

        public DogConfig CloneMe()
        {
            return (DogConfig)this.MemberwiseClone();
        }
    }
}