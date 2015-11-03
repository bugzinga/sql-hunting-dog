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

        public DogConfig()
        {
            ScriptIndexies = true;
            ScriptTriggers = true;
            ScriptForeignKeys = false;

            SelectTopX = 200;

            AddNoLock = false;
            IncludeAllColumns = true;
            AddWhereClauseFor = false;
            OrderBy = EOrderBy.Descending;

            AlterOrCreate = EAlterOrCreate.Create;

            LimitSearch = 500;
        }

        [Category("SELECT")]
        [DisplayName("Add Column Names to SELECT")]
        [Description("Use 'SELECT *' or 'SELECT column1, column2..' syntax'")]
        public bool IncludeAllColumns { get; set; }

        [Category("SCRIPT")]
        [DisplayName("Script Indexes")]
        [Description("Include Indexes when scripting a Table")]
        public bool ScriptIndexies { get; set; }

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
        [Description("Add NOLOCK hint. Can lead to dirty of inconsistent data to be presented")]
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
        [Description("Retreieve only first X objects")]
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

        private string _launchingHotKey = "D";

        [Category("GENERAL")]
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