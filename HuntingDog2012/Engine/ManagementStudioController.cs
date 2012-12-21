using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuntingDog;
using HuntingDog.DogEngine;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;


using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.Common;
using System.Data;
using System.Data.SqlClient;


namespace DatabaseObjectSearcher
{
  

    // interact with Management studio environment
    // can open windows, exeute scripts, navgitae in object explorer
    public class ManagementStudioController
    {

        private static string CreateHeader(string sqlStr,SqlConnectionInfo connInfo)
        {
            string stars = "-- Server : " + connInfo.ServerName + " -- " + Environment.NewLine;
            return stars + sqlStr;
        }

        public static string UseDataBaseGo(Database db)
        {
            return Environment.NewLine + "USE [" + db.Name + "] "
                + Environment.NewLine + "GO "
                + Environment.NewLine;
        }

        public static void SelectFromView(View view, SqlConnectionInfo connInfo)
        {
            try
            {

                // create new document               
                string select = "";
                lock (view)
                {
                    view.Refresh();
                    select = string.Format("{0} SELECT * FROM [{1}].[{2}]", UseDataBaseGo(view.Parent),
                      view.Schema,view.Name);
                }

                CreateSQLDocumentWithHeader(select, connInfo);

                System.Windows.Forms.SendKeys.Send("{F5}");
            }
            catch (Exception ex)
            {
                MyLogger.LogError("SelectFromView failed.", ex);
            }


        }

        public static void OpenFunctionForModification(UserDefinedFunction userDefinedFunction, SqlConnectionInfo connInfo)
        {
            try
            {                             

                StringBuilder builder = new StringBuilder(1000);
                lock (userDefinedFunction)
                {
                    userDefinedFunction.Refresh();

                    builder.AppendLine(UseDataBaseGo(userDefinedFunction.Parent));


                    var indexOfCreate = userDefinedFunction.TextHeader.IndexOf(CREATE_FUNC, 0, StringComparison.OrdinalIgnoreCase);
                    var alterTextHeader = userDefinedFunction.TextHeader.Remove(indexOfCreate, CREATE_FUNC.Length);

                    builder.Append(alterTextHeader.Insert(indexOfCreate, ALTER_FUNC));
                    builder.Append(userDefinedFunction.TextBody);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);
              
            }
            catch (Exception ex)
            {
                MyLogger.LogError("OpenFunctionForModification failed.", ex);
            }
        }

        private static UIConnectionInfo CreateFrom(SqlConnectionInfo connInfo)
        {
            UIConnectionInfo ci = new UIConnectionInfo();
            ci.ServerName = connInfo.ServerName;
            ci.ServerType = new Guid("8c91a03d-f9b4-46c0-a305-b5dcc79ff907");

            ci.UserName = connInfo.UserName;
            ci.Password = connInfo.Password;
            ci.PersistPassword = true;

            ci.ApplicationName = "Microsoft SQL Server Management Studio - Query";
            if (connInfo.UseIntegratedSecurity == false)
                ci.AuthenticationType = 1;
            else
                ci.AuthenticationType = 0;


            return ci;
        }

        static Dictionary<string, UIConnectionInfo> _uiConn = new Dictionary<string, UIConnectionInfo>();

        const string CREATE_PROC = "CREATE PROC";
        const string ALTER_PROC = "ALTER PROC";

        const string CREATE_FUNC = "CREATE FUNC";
        const string ALTER_FUNC = "ALTER FUNC";

        public static void ListDependency(SqlConnectionInfo connInfo, StoredProcedure sp, Database db)
        {
            DependencyWalker w = new DependencyWalker((Server)db.Parent);
            var dpTree = w.DiscoverDependencies(new SqlSmoObject[] { sp }, false);
         
            w.WalkDependencies(dpTree);
        }

        public static void ModifyView(View vw, SqlConnectionInfo connInfo)
        {
            try
            {

                StringBuilder builder = new StringBuilder(1000);
                lock (vw)
                {
                    vw.Refresh();

                    string originalSP = vw.TextBody;
                    builder.AppendLine(UseDataBaseGo(vw.Parent));

                    builder.Append(vw.ScriptHeader(true));
                    // trye to use ScriptHeader(true) to change header. !!!
                    //var indexOfCreate = sp.TextHeader.IndexOf(CREATE_PROC,0,StringComparison.OrdinalIgnoreCase);

                    // var alterTextHeader = sp.TextHeader.Remove(indexOfCreate, CREATE_PROC.Length);
                    // builder.Append( alterTextHeader.Insert(indexOfCreate,ALTER_PROC) );

                    builder.Append(originalSP);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);

            }
            catch (Exception ex)
            {
                MyLogger.LogError("OpenStoredProcedureForModification failed.", ex);
            }
        }

        public static void OpenStoredProcedureForModification(StoredProcedure sp, SqlConnectionInfo connInfo)
        {
            try
            {
               
                StringBuilder builder = new StringBuilder(1000);
                lock (sp)
                {           
                    sp.Refresh();
                   
                    string originalSP = sp.TextBody;
                    builder.AppendLine(UseDataBaseGo(sp.Parent));

                    builder.Append(sp.ScriptHeader(true));
                    // trye to use ScriptHeader(true) to change header. !!!
                    //var indexOfCreate = sp.TextHeader.IndexOf(CREATE_PROC,0,StringComparison.OrdinalIgnoreCase);
     
                   // var alterTextHeader = sp.TextHeader.Remove(indexOfCreate, CREATE_PROC.Length);
                   // builder.Append( alterTextHeader.Insert(indexOfCreate,ALTER_PROC) );

                   builder.Append(originalSP);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);

            }
            catch (Exception ex)
            {
                MyLogger.LogError("OpenStoredProcedureForModification failed.", ex);
            }



        }

        static private void CreateSQLDocumentWithHeader(string sqlText,SqlConnectionInfo connInfo)
        {
            CreateSQLDocument(CreateHeader(sqlText,connInfo), connInfo);
        }

        static private void CreateSQLDocument(string sqlText, SqlConnectionInfo connInfo)
        {
            if (!_uiConn.ContainsKey(connInfo.ServerName))
            {
                var aci = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo;
                _uiConn[connInfo.ServerName] = CreateFrom(connInfo);
            }

            var uiConn = _uiConn[connInfo.ServerName];

            ServiceCache.ScriptFactory.CreateNewBlankScript(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptType.Sql, uiConn, null);
      
            // create new document
            EnvDTE.TextDocument doc = (EnvDTE.TextDocument)ServiceCache.ExtensibilityModel.Application.ActiveDocument.Object(null);
            // insert SQL definition to document
            
            doc.EndPoint.CreateEditPoint().Insert(sqlText);

        }


        private static string TypeAndDefaultName(StoredProcedureParameter par)
        {
            return  par.DataType.Name + ", default: " + par.DefaultValue;
        }

        private static bool IsNumeric(DataType dt)
        {
            return dt.SqlDataType == SqlDataType.Int || dt.SqlDataType == SqlDataType.TinyInt ||
                dt.SqlDataType == SqlDataType.BigInt || dt.SqlDataType == SqlDataType.Bit ||
                 dt.SqlDataType == SqlDataType.Float || dt.SqlDataType == SqlDataType.Decimal || 
                  dt.SqlDataType == SqlDataType.SmallInt || dt.SqlDataType == SqlDataType.SmallMoney
                 ; 
        }

        private static bool IsDateTime(DataType dt)
        {
            return dt.SqlDataType == SqlDataType.DateTime || dt.SqlDataType == SqlDataType.DateTime2 ||
                   dt.SqlDataType == SqlDataType.SmallDateTime;
        }

        private static bool IsDate(DataType dt)
        {
            return dt.SqlDataType == SqlDataType.Date;
        }

        private static bool IsString(DataType dt)
        {
            return dt.SqlDataType == SqlDataType.VarChar || dt.SqlDataType == SqlDataType.VarCharMax ||
                dt.SqlDataType == SqlDataType.NVarChar || dt.SqlDataType == SqlDataType.NVarCharMax ||
                dt.SqlDataType == SqlDataType.Char || dt.SqlDataType == SqlDataType.NChar ||
                 dt.SqlDataType == SqlDataType.Text || dt.SqlDataType == SqlDataType.NText;
        }

        static string MakeParameterType(DataType parType)
        {

            if (parType.SqlDataType == SqlDataType.NVarChar || parType.SqlDataType == SqlDataType.VarChar)
                return parType.Name + "(" + parType.MaximumLength.ToString() + ")";
            return parType.Name;

        }

        private static IFormatProvider _us_culture = null;
        private  static IFormatProvider UsCulture
        {
            get
            {
                // en-US culture, what I'd ultimately like to see the DateTime in
                if(_us_culture==null)
                    _us_culture = new System.Globalization.CultureInfo("en-US",
                       true);
                return _us_culture;
            }
        }

        private static string MakeParameterWithValue(string name, DataType parType,bool useLikeForString)
        {
            if (IsDateTime(parType))
            {
              
                return name + " = '" + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss",UsCulture) + "' -- " +
                       MakeParameterType(parType);
            }
            if (IsDate(parType))
                return name + " = '" + DateTime.Now.ToString("dd MMM yyyy",UsCulture) + "' -- " + MakeParameterType(parType);
            else if (IsNumeric(parType))
                return name + " = 0" + " -- " + MakeParameterType(parType);
            else if (useLikeForString)
                return name + " like '%%'" + " -- " + MakeParameterType(parType);
            else
                return name + " = ''" + " -- " + MakeParameterType(parType);
        }
       

        private static string MakeParameter(StoredProcedureParameter par)
        {
            if (!string.IsNullOrEmpty(par.DefaultValue))
                return " -- " + par.Name + " = " + par.DefaultValue + "  -- " + par.DataType.Name + " ,default " + par.DefaultValue;

            return MakeParameterWithValue(par.Name, par.DataType,false);
        }


        private static string MakeParameterForFunction(UserDefinedFunctionParameter parType)
        {
            if (IsDateTime(parType.DataType))
                return "'"+ DateTime.Now.ToString("dd MMM yyyy HH:mm:SS") + "'";
           if (IsDate(parType.DataType))
               return "'" + DateTime.Now.ToString("dd MMM yyyy") + "'";
           if (IsNumeric(parType.DataType))
                return "0";
            
            return  "''";
        }

        public static void ExecuteStoredProc(StoredProcedure sp,  SqlConnectionInfo connInfo)
        {
            try
            {

                string execScript = string.Format("{0}\r\n EXECUTE [{2}].[{3}] {1}",
                UseDataBaseGo(sp.Parent), Environment.NewLine, sp.Schema, sp.Name);

             
                lock (sp)
                {
                    sp.Refresh();
                    sp.Parameters.Refresh();
                    string parameterList = "";
                    for(int i = 0; i<sp.Parameters.Count;i++)
                    {
                        // make a proper padding and add a comma if it not first line
                        parameterList += "\t\t" + (i > 0 ? "," : "");
             
                        parameterList += MakeParameter(sp.Parameters[i]) + Environment.NewLine;
                        
                    }

                    if (sp.Parameters.Count > 0)
                    {
                        execScript += parameterList;
                    }
                }

                CreateSQLDocumentWithHeader(execScript, connInfo);
               
            }
            catch (Exception ex)
            {
                MyLogger.LogError("ExecuteStoredProc failed.", ex);
            }
        }


        public static void ExecuteFunction(UserDefinedFunction func, SqlConnectionInfo connInfo)
        {
            try
            {

                string execScript = "";
               
                var builder = new StringBuilder(1000);
                lock (func)
                {

                    func.Refresh();
                    func.Parameters.Refresh();


                    string execTemplate = func.FunctionType == UserDefinedFunctionType.Scalar ? "SELECT " : "SELECT * FROM ";
                  
                    execScript = string.Format("{0}\r\n {1} [{2}].[{3}]",
                                 UseDataBaseGo(func.Parent), execTemplate, func.Schema, func.Name);

                    string parameterList = "";
                    if(func.Parameters.Count>0)
                    {
                       
                        for(int i = 0; i<func.Parameters.Count;i++)
                        {
                            if(i>0)
                                parameterList += " , ";

                            parameterList += MakeParameterForFunction(func.Parameters[0]);
                        }                      

                    }
                    execScript += " ( " + parameterList + " ) ";

            
                }

                CreateSQLDocumentWithHeader(execScript, connInfo);

            }
            catch (Exception ex)
            {
                MyLogger.LogError("ExecuteFunctionfailed.", ex);
            }
        }


        public class ManagedConn : IManagedConnection
        {

            public void Close()
            {
                
            }

            public SqlOlapConnectionInfoBase Connection { get; set; }


            public void Dispose()
            {
             
            }
        }


        public static string CreateFile(string script)
        {
            string str = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", new object[] { System.IO.Path.GetTempFileName(), "dtq" });
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("[D035CF15-9EDB-4855-AF42-88E6F6E66540, 2.00]\r\n");
            builder.Append("Begin Query = \"Query1.dtq\"\r\n");
            builder.Append("Begin RunProperties =\r\n");
            builder.AppendFormat("{0}{1}{2}", "SQL = \"", script, "\"\r\n");
            builder.Append("ParamPrefix = \"@\"\r\n");
            builder.Append("ParamSuffix = \"\"\r\n");
            builder.Append("ParamSuffix = \"\\\"\r\n");
            builder.Append("End\r\n");
            builder.Append("End\r\n");
            System.IO.StreamWriter writer = new System.IO.StreamWriter(str, false, System.Text.Encoding.Unicode);
            writer.Write(builder.ToString());
            writer.Close();
            return str;
        }



        public void Open2()
        {
                //open this object Microsoft.SqlServer.Management.UI.VSIntegration.Editors.VirtualProject

                //var asm = Assembly.LoadFile(@"d:\sql\SQLEditors.dll");
                ////var vpType = asm.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.Editors.VirtualProject");

                //// static method internal static VirtualProject TheVirtualProject { get; }
                ////var projProp = vpType.GetProperty("TheVirtualProject", BindingFlags.Static | BindingFlags.NonPublic);

                ////var virtualProject = projProp.GetValue(vpType, null);
                //// should work for designing table

                //var vpType= ScriptFactory.Instance.GetType();
                //var createDesMethod = vpType.GetMethod("CreateDesigner", BindingFlags.Instance | BindingFlags.NonPublic);
                //// will try for editing table as well - can work as well not sure...

                //var enumType = asm.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.Editors.DocumentOptions");

                //var infos = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                //object enumValue = null;
                //foreach (FieldInfo fi in infos)
                //{
                //    if (fi.Name == "None")
                //    {
                //        enumValue = fi.GetValue(null);

                //        var mc = new ManagedConn();
                //        mc.Connection = connInfo;
                //        //void ISqlVirtualProject.CreateDesigner(Urn origUrn, DocumentType editorType, DocumentOptions aeOptions, IManagedConnection con)
                //        createDesMethod.Invoke(ScriptFactory.Instance, new object[] { DocumentType.OpenTable, enumValue, new Urn(tbl.Urn.ToString() + "/Data"), mc });

                //    }
                //}
        }

        public static void DesignTable(Table tbl, SqlConnectionInfo connInfo)
        {
            if(tbl.State == SqlSmoState.Dropped)
            {
                MyLogger.LogMessage("trying to design dropped table.");
                return;
            }

            var mc = new ManagedConn();
            mc.Connection = connInfo;
            ServiceCache.ScriptFactory.DesignTableOrView(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.DocumentType.Table,
                      DocumentOptions.ManageConnection,
                      tbl.Urn.ToString(), mc);
        }


        public static void OpenTable(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            if (objectToSelect.State == SqlSmoState.Dropped)
            {
                MyLogger.LogMessage("trying to open dropped table.");
                return;
            }

            ObjectExplorerManager _manager = new ObjectExplorerManager();
            _manager.OpenTable(objectToSelect, connection);
        }


        public static void ScriptTable(Table tbl, SqlConnectionInfo connInfo)
        {

            try
            {

                // create new document               
                string select = "";
                lock (tbl)
                {
                    tbl.Refresh();
                    tbl.Columns.Refresh();
                    tbl.Indexes.Refresh();

                    select += UseDataBaseGo(tbl.Parent);

                    foreach (var line in tbl.Script())
                    {
                        select += "\r\n\t";
                        select += line;
                    }

                    if(tbl.Indexes.Count>0)
                    {
                        select += "\r\n  \r\n  -- Indexes --\r\n";
                        foreach (Index indexDef in tbl.Indexes)
                        {                         
                            foreach (var line in indexDef.Script())
                            {
                                select += line;
                            }
                            select += "\r\n \r\n";
                        }
                    }
                   

                }

                CreateSQLDocumentWithHeader(select, connInfo);

            }
            catch (Exception ex)
            {
                MyLogger.LogError("Script Table failed.", ex);
            }



        }


        public static void SelectFromTable(Table tbl, SqlConnectionInfo connInfo)
        {
           
            try
            {

                // create new document               
                string select = "";
                lock (tbl)
                {
                    tbl.Refresh();
                    tbl.Columns.Refresh();
                    select = string.Format("{0}\r\n SELECT TOP 200 * FROM [{1}].[{2}]",
                       UseDataBaseGo(tbl.Parent), tbl.Schema, tbl.Name);

                    string where = Environment.NewLine + "\t\t\t-- WHERE ";
                    foreach (Column p in tbl.Columns)
                        where += Environment.NewLine + "\t\t\t\t-- " + MakeParameterWithValue(p.Name, p.DataType,true);

                    if (tbl.Columns.Count > 0)
                        select += where;
                }

                CreateSQLDocumentWithHeader(select, connInfo);

                System.Windows.Forms.SendKeys.Send("{F5}");
            }
            catch (Exception ex)
            {
                MyLogger.LogError("SelectFromTable failed.",ex);
            }

           

        }


        public static void SelectServerInObjectExplorer()
        {


        }



        public static void LocateInObjectExplorer(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            ObjectExplorerManager _manager = new ObjectExplorerManager();
            _manager.SelectSMOObjectInObjectExplorer(objectToSelect, connection);
        }


        public static DependecyResults GetDependencies(ScriptSchemaObjectBase obj, SqlConnectionInfo connection,Database db )
        {
            string dependentCmd = string.Format("EXEC sp_MSdependencies N'{0}.{1}', null, 1315327", obj.Schema, obj.Name);
            string dependensOnCmd = string.Format("EXEC sp_MSdependencies N'{0}.{1}', null, 1053183", obj.Schema, obj.Name);

            var result = new DependecyResults();
            
            //
            //EXEC sp_MSdependencies N'dbo.V2_BREC_Scenario', null, 1315327
            // EXEC sp_MSdependencies N'dbo.V2_BREC_Scenario', null, 1053183
            SqlConnectionStringBuilder cntString = new SqlConnectionStringBuilder(connection.ConnectionString);
            cntString.InitialCatalog = db.Name;
            using (var sq = new SqlConnection(cntString.ConnectionString))
            {
                sq.Open();

                using (var r = new SqlCommand(dependentCmd, sq))
                {
                   
                    using (var reader = r.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var type = reader["oType"].ToString();
                            var name = reader["oObjName"].ToString();
                             var schema = reader["oOwner"].ToString();
                             result.DependantUpon.Add(new Link() { Name = name, Schema = schema, Type = type });
                        }
                    }
                }

                using (var r = new SqlCommand(dependensOnCmd, sq))
                {

                    using (var reader = r.ExecuteReader())
                    {
                        // go to directly to the second table

                        do
                        {
                            while (reader.Read())
                            {
                                var type = reader["oType"].ToString();
                                if (type != "4096")
                                {
                                    var name = reader["oObjName"].ToString();
                                    var schema = reader["oOwner"].ToString();

                                     // filter out User Defined Types...
                                     if(type!= "4096")
                                    result.DependsOn.Add(new Link() { Name = name, Schema = schema, Type = type });
                                }
                               
                            }

                        }
                        while (reader.NextResult());
                      
                    }
                }

            }

            return result;
            

        }

  
    }
}
