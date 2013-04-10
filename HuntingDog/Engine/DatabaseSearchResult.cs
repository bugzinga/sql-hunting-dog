
using System;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace HuntingDog.DogEngine
{
    [Flags]
    public enum ObjType : int
    {
        Table = 1,
        StoredProc = 2,
        Func = 4,
        View = 8
    }

    public class DatabaseSearchResult : IComparable<DatabaseSearchResult>
    {
        private ScriptSchemaObjectBase result;

        private SqlConnectionInfo connection;

        public Database DataBase
        {
            get;
            private set;
        }

        public String SearchName
        {
            get;
            private set;
        }

        public String HighlightName
        {
            get;
            set;
        }

        public ObjType ObjectType
        {
            get;
            set;
        }

        public String Name
        {
            get
            {
                return Result.Name;
            }
        }

        public String SchemaAndName
        {
            get
            {
                return Schema + "." + Result.Name;
            }
        }

        public String Schema
        {
            get
            {
                return Result.Schema;
            }
        }

        public Boolean IsStoredProc
        {
            get
            {
                return (ObjectType == ObjType.StoredProc);
            }
        }

        public Boolean IsTable
        {
            get
            {
                return (ObjectType == ObjType.Table);
            }
        }

        public Boolean IsFunction
        {
            get
            {
                return (ObjectType == ObjType.Func);
            }
        }

        public Boolean IsView
        {
            get
            {
                return (ObjectType == ObjType.View);
            }
        }

        public ScriptSchemaObjectBase Result
        {
            get
            {
                return result;
            }
        }

        public SqlConnectionInfo Connection
        {
            get
            {
                return connection;
            }
        }

        public DatabaseSearchResult(ScriptSchemaObjectBase result, SqlConnectionInfo connection, Database db)
        {
            DataBase = db;

            this.result = result;
            this.connection = connection;

            if (result is Table)
            {
                ObjectType = ObjType.Table;
            }
            else if (result is StoredProcedure)
            {
                ObjectType = ObjType.StoredProc;
            }
            else if (result is View)
            {
                ObjectType = ObjType.View;
            }
            else if (result is UserDefinedFunction)
            {
                ObjectType = ObjType.Func;
            }
            else
            {
                throw new NotImplementedException("Unknown object type " + result.GetType().Name);
            }

            SearchName = SchemaAndName.ToLower();
        }

        public void Refresh()
        {
            if (ObjectType == ObjType.Table)
            {
                (Result as Table).Refresh();
            }
            else if (ObjectType == ObjType.StoredProc)
            {
                (Result as StoredProcedure).Refresh();
            }
        }

        #region IComparable<DatabaseSearchResult> Members

        public Int32 CompareTo(DatabaseSearchResult other)
        {
            return String.Compare(result.Name, other.Result.Name);
        }

        #endregion
    }
}
