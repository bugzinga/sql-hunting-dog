using System;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Xml.Serialization;

namespace HuntingDog.DogEngine
{
   
    [Flags]
    public enum ObjType:int
    {
        Table = 1,
        StoredProc = 2,
        Func = 4,
        View = 8
    }

    public class DatabaseSearchResult: IComparable<DatabaseSearchResult>,IDisposable
    {

        public void Dispose()
        {
           //if(result!=null)
           //    result
        }

        private ScriptSchemaObjectBase result;
        private SqlConnectionInfo connection;
        public Database DataBase { get; private set; }

        public string SearchName { get; private set; }

        public string HighlightName { get; set; }

        public DatabaseSearchResult(ScriptSchemaObjectBase result, SqlConnectionInfo connection,Database db)
        {
            DataBase = db;    

            this.result = result;
            this.connection = connection;
            if (result is Table)
                ObjectType = ObjType.Table;
            else if (result is StoredProcedure)
                ObjectType = ObjType.StoredProc;
            else if (result is View)
                ObjectType = ObjType.View;
            else if (result is UserDefinedFunction)
                ObjectType = ObjType.Func;
            else 
                throw new NotImplementedException("Unknown object type " + result.GetType().Name);

            SearchName = (Schema + Name).ToLower();
        }

        public void Refresh()
        {
            if (ObjectType == ObjType.Table)
            {
                (Result as Table).Refresh();
            }
            else if(ObjectType == ObjType.StoredProc)
            {
                (Result as StoredProcedure).Refresh();
            }
        
        }

        public string Name
        {
            get
            {
                return Result.Name;
            }
        }

        public string SchemaAndName
        {
            get
            {
                return Schema + "." + Result.Name;
            }
        }


        public string Schema
        {
            get
            {
                return Result.Schema;
            }
        }

        public ObjType ObjectType { get; set; }

        public bool IsStoredProc
        {
            get
            {
                return ObjectType == ObjType.StoredProc;
            }
        }

        public bool IsTable
        {
            get
            {
                return ObjectType == ObjType.Table;
            }
        }

        public bool IsFunction
        {
            get
            {
                return ObjectType == ObjType.Func;
            }
        }

        public bool IsView
        {
            get
            {
                return ObjectType == ObjType.View;
            }
        }
    

        public ScriptSchemaObjectBase Result
        {
            get { return result; }
        }

        public SqlConnectionInfo Connection
        {
            get { return connection; }
        }
    
        #region IComparable<DatabaseSearchResult> Members

        public int  CompareTo(DatabaseSearchResult other)
        {
            return String.Compare(result.Name, other.Result.Name);
        }

        #endregion


    }
}
