
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using HuntingDog.Config;
using HuntingDog.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using System.Linq;

namespace DatabaseObjectSearcher
{
    // interact with Management studio environment
    // can open windows, execute scripts, navigate in object explorer
    public class ManagementStudioController
    {
        static string AlterOrCreate(HuntingDog.Config.EAlterOrCreate alterOrCreate)
        {
            if (alterOrCreate == HuntingDog.Config.EAlterOrCreate.Alter)
                return "ALTER";
            else
                return "CREATE";

        }

        private static readonly Log log = LogFactory.GetLog();

        private const String CREATE_PROC = "CREATE PROC";

        private const String ALTER_PROC = "ALTER PROC";

        private const String CREATE_FUNC = "CREATE FUNC";

        private const String ALTER_FUNC = "ALTER FUNC";

        private static Dictionary<String, UIConnectionInfo> _uiConn = new Dictionary<String, UIConnectionInfo>();

        private static IFormatProvider _us_culture = null;

        private static String CreateHeader(String sqlStr, SqlConnectionInfo connInfo)
        {
            var stars = "-- Server : " + connInfo.ServerName + " -- " + Environment.NewLine;
            return (stars + sqlStr);
        }

        public static String UseDataBaseGo(Database db)
        {
            return Environment.NewLine + "USE [" + db.Name + "]"
                + Environment.NewLine + "GO";
        }

        public static void SelectFromView(View view, SqlConnectionInfo connInfo, int selectTopX, bool includeAllCoulumnNamesForViews,bool includeWhereClause,bool addNoLockHint)
        {
            try
            {               
                var builder = new StringBuilder(1000);

                lock (view)
                {
                    view.Refresh();
                    view.Columns.Refresh();

                    builder.AppendLine(UseDataBaseGo(view.Parent));
                    var noLockHint = addNoLockHint ? " WITH(NOLOCK) " : "";
                    var selectColumns = BuildColumnNames(view.Columns, includeAllCoulumnNamesForViews);
                    builder.AppendFormat("\tSELECT TOP {0} {1}\r\n\tFROM {2}.{3} {4}", selectTopX, selectColumns, view.Schema, view.Name, noLockHint);

                    if (includeWhereClause)
                    {
                        builder.AppendFormat("\r\n{0}", BuildWhereClause(view.Columns));
                    }
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);

                System.Windows.Forms.SendKeys.Send("{F5}");
            }
            catch (Exception ex)
            {
                log.Error("SelectFromView failed.", ex);
            }
        }
          

        public static void SelectFromTable(Table tbl, SqlConnectionInfo connInfo, int selectTopXTable, bool includeAllCoulumnNamesForTables,bool includeWhereClause, bool addNoLockHint, HuntingDog.Config.EOrderBy orderBy)
        {
            try
            {
                var builder = new StringBuilder(1000);

                lock (tbl)
                {
                    tbl.Refresh();
                    tbl.Columns.Refresh(true);

                    builder.AppendLine(UseDataBaseGo(tbl.Parent));

                    var selectColumns = BuildColumnNames(tbl.Columns, includeAllCoulumnNamesForTables);
                    var noLockHint = addNoLockHint ? " WITH(NOLOCK) " : "";            
                    builder.AppendFormat("\tSELECT TOP {0} {1}\r\n\tFROM {2}.{3} {4}", selectTopXTable, selectColumns, tbl.Schema, tbl.Name, noLockHint);

                    if (includeWhereClause)
                    {
                        builder.AppendFormat("\r\n{0}", BuildWhereClause(tbl.Columns));
                    }

                    if (orderBy == EOrderBy.Ascending || orderBy == EOrderBy.Descending)
                    {
                        builder.AppendFormat("\r\n{0}", BuildOrderByPrimaryKey(tbl, orderBy == EOrderBy.Ascending));
                    }

                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);

                System.Windows.Forms.SendKeys.Send("{F5}");
            }
            catch (Exception ex)
            {
                log.Error("SelectFromTable failed.", ex);
            }
        }

        private static string BuildOrderByPrimaryKey(Table tbl, bool ascending)
        {
            var pkList = ListPrimaryKeys(tbl);
            if (!pkList.Any())
                return string.Empty;

            var order = ascending?" ASC":" DESC";

            var orderBy = new StringBuilder(200);
            orderBy.Append("\tORDER BY ");
            orderBy.Append(string.Join(",", pkList.Select(x => x.Name + order).ToArray()));

            return orderBy.ToString();
        }

        private static IEnumerable<Column> ListPrimaryKeys(Table tbl)
        {
            var listResult = new List<Column>();
            
            foreach (Column p in tbl.Columns)
            {
                if (p.InPrimaryKey)
                    listResult.Add(p);
            }

            return listResult;
        }

        private static string BuildWhereClause(ColumnCollection columns)
        {
            if (columns.Count == 0)
                return string.Empty;

            var where = new StringBuilder(200);

            where.Append("\t-- WHERE ");

            foreach (Column column in columns)
            {
                where.AppendFormat("{0}{1}{2}", Environment.NewLine, "\t\t-- ", MakeParameterWithValue(column.Name, column.DataType, true));
            }

            return where.ToString();
        }

        public static void OpenFunctionForModification(UserDefinedFunction userDefinedFunction, SqlConnectionInfo connInfo, HuntingDog.Config.EAlterOrCreate alterOrCreate)
        {
            try
            {
                var builder = new StringBuilder(1000);

                lock (userDefinedFunction)
                {
                    userDefinedFunction.Refresh();

                    builder.AppendLine(UseDataBaseGo(userDefinedFunction.Parent));
                    builder.Append(userDefinedFunction.ScriptHeader(alterOrCreate == EAlterOrCreate.Alter));
                    builder.Append(userDefinedFunction.TextBody);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);
            }
            catch (Exception ex)
            {
                log.Error("OpenFunctionForModification failed.", ex);
            }
        }

        private static UIConnectionInfo CreateFrom(SqlConnectionInfo connInfo)
        {
            var ci = new UIConnectionInfo();
            ci.ServerName = connInfo.ServerName;
            ci.ServerType = new Guid("8c91a03d-f9b4-46c0-a305-b5dcc79ff907");
            ci.UserName = connInfo.UserName;
            ci.Password = connInfo.Password;
            ci.PersistPassword = true;
            ci.ApplicationName = "Microsoft SQL Server Management Studio - Query";

            ci.AuthenticationType = !connInfo.UseIntegratedSecurity
                ? 1
                : 0;

            return ci;
        }

        public static void ListDependency(SqlConnectionInfo connInfo, StoredProcedure sp, Database db)
        {
            var w = new DependencyWalker((Server)db.Parent);
            var dpTree = w.DiscoverDependencies(new SqlSmoObject[] { sp }, false);
            w.WalkDependencies(dpTree);
        }

        public static void ModifyView(View vw, SqlConnectionInfo connInfo, EAlterOrCreate alterOrCreateView)
        {
            try
            {
                var builder = new StringBuilder(1000);

                lock (vw)
                {
                    vw.Refresh();

                    var originalSP = vw.TextBody;

                    builder.AppendLine(UseDataBaseGo(vw.Parent));
                    builder.Append(vw.ScriptHeader(alterOrCreateView == EAlterOrCreate.Alter));
                    builder.Append(originalSP);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);
            }
            catch (Exception ex)
            {
                log.Error("OpenStoredProcedureForModification failed.", ex);
            }
        }

        public static void OpenStoredProcedureForModification(StoredProcedure sp, SqlConnectionInfo connInfo, EAlterOrCreate alterOrCreateSp)
        {
            try
            {
                var builder = new StringBuilder(1000);

                lock (sp)
                {
                    sp.Refresh();

                    var originalSP = sp.TextBody;

                    builder.AppendLine(UseDataBaseGo(sp.Parent));
                    builder.Append(sp.ScriptHeader(alterOrCreateSp == EAlterOrCreate.Alter));
                    builder.Append(originalSP);
                }

                CreateSQLDocumentWithHeader(builder.ToString(), connInfo);
            }
            catch (Exception ex)
            {
                log.Error("OpenStoredProcedureForModification failed.", ex);
            }
        }

        static private void CreateSQLDocumentWithHeader(String sqlText, SqlConnectionInfo connInfo)
        {
            CreateSQLDocument(CreateHeader(sqlText, connInfo), connInfo);
        }

        static private void CreateSQLDocument(String sqlText, SqlConnectionInfo connInfo)
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

        private static String TypeAndDefaultName(StoredProcedureParameter par)
        {
            return (par.DataType.Name + ", default: " + par.DefaultValue);
        }

        private static Boolean IsNumeric(DataType dt)
        {
            return (dt.SqlDataType == SqlDataType.Int) || (dt.SqlDataType == SqlDataType.TinyInt) ||
                (dt.SqlDataType == SqlDataType.BigInt) || (dt.SqlDataType == SqlDataType.Bit) ||
                (dt.SqlDataType == SqlDataType.Float) || (dt.SqlDataType == SqlDataType.Decimal) ||
                (dt.SqlDataType == SqlDataType.SmallInt) || (dt.SqlDataType == SqlDataType.SmallMoney);
        }

        private static Boolean IsDateTime(DataType dt)
        {
            return (dt.SqlDataType == SqlDataType.DateTime) || (dt.SqlDataType == SqlDataType.DateTime2) ||
                (dt.SqlDataType == SqlDataType.SmallDateTime);
        }

        private static Boolean IsDate(DataType dt)
        {
            return (dt.SqlDataType == SqlDataType.Date);
        }

        private static Boolean IsBinary(DataType dt)
        {
            return (dt.SqlDataType == SqlDataType.Binary || dt.SqlDataType == SqlDataType.VarBinary || dt.SqlDataType == SqlDataType.VarBinaryMax);
        }

        private static Boolean IsString(DataType dt)
        {
            return (dt.SqlDataType == SqlDataType.VarChar) || (dt.SqlDataType == SqlDataType.VarCharMax) ||
                (dt.SqlDataType == SqlDataType.NVarChar) || (dt.SqlDataType == SqlDataType.NVarCharMax) ||
                (dt.SqlDataType == SqlDataType.Char) || (dt.SqlDataType == SqlDataType.NChar) ||
                (dt.SqlDataType == SqlDataType.Text) || (dt.SqlDataType == SqlDataType.NText);
        }

        static String MakeParameterType(DataType parType)
        {
            return ((parType.SqlDataType == SqlDataType.NVarChar) || (parType.SqlDataType == SqlDataType.VarChar))
                ? (parType.Name + "(" + parType.MaximumLength.ToString() + ")")
                : parType.Name;
        }

        private static IFormatProvider UsCulture
        {
            get
            {
                // en-US culture, what I'd ultimately like to see the DateTime in
                if (_us_culture == null)
                    _us_culture = new System.Globalization.CultureInfo("en-US",
                       true);
                return _us_culture;
            }
        }



        private static string MakeParameterWithValue(string name, DataType parType, bool useLikeForString)
        {
            if (IsDateTime(parType))
            {

                return name + " = '" + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", UsCulture) + "' -- " +
                       MakeParameterType(parType);
            }
            if (IsDate(parType))
                return name + " = '" + DateTime.Now.ToString("dd MMM yyyy", UsCulture) + "' -- " + MakeParameterType(parType);
            else if (IsNumeric(parType))
                return name + " = 0" + " -- " + MakeParameterType(parType);
            else if (useLikeForString)
                return name + " like '%%'" + " -- " + MakeParameterType(parType);
            else if (IsBinary(parType))
                return name + " = 0x00" + " -- " + MakeParameterType(parType);
            else
                return name + " = ''" + " -- " + MakeParameterType(parType);
        }


        private static string MakeParameter(StoredProcedureParameter par, out bool hasDefaultValue)
        {
            if (!string.IsNullOrEmpty(par.DefaultValue))
            {
                hasDefaultValue = true;
                return par.Name + " = " + par.DefaultValue + "  -- " + par.DataType.Name + " ,default " + par.DefaultValue;
            }

            hasDefaultValue = false;
            return MakeParameterWithValue(par.Name, par.DataType, false);
        }

        private static string MakeParameterForFunction(UserDefinedFunctionParameter parType)
        {
            if (IsDateTime(parType.DataType))
                return "'" + DateTime.Now.ToString("dd MMM yyyy HH:mm:SS") + "'";
            if (IsDate(parType.DataType))
                return "'" + DateTime.Now.ToString("dd MMM yyyy") + "'";
            if (IsNumeric(parType.DataType))
                return "0";

            return "''";
        }

        public static void ExecuteStoredProc(StoredProcedure sp, SqlConnectionInfo connInfo)
        {
            try
            {

                string execScript = string.Format("{0}\r\n EXECUTE [{2}].[{3}] {1}",
                UseDataBaseGo(sp.Parent), Environment.NewLine, sp.Schema, sp.Name);


                lock (sp)
                {
                    sp.Refresh();
                    sp.Parameters.Refresh(true);    // refresh all parameters and their types and default values
                    string parameterList = "";

                    bool hasAtLeastOneNondefaultParameter = false;
                    for (int i = 0; i < sp.Parameters.Count; i++)
                    {

                        bool hasDefaultValue = false;
                        string parameterValue = MakeParameter(sp.Parameters[i], out hasDefaultValue);

                        string commaOrComment = string.Empty;

                        // add comma only to second or subsequent line and only if parameter does not have default value
                        if (i > 0 && !hasDefaultValue && hasAtLeastOneNondefaultParameter)
                        {
                            commaOrComment = ",";
                        }

                        if (hasDefaultValue)
                            commaOrComment = " -- ";
                        else
                            hasAtLeastOneNondefaultParameter = true;

                        parameterList += string.Format("\t\t{0}{1}\r\n", commaOrComment, parameterValue);

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
                log.Error("ExecuteStoredProc failed.", ex);
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
                    func.Parameters.Refresh(true);


                    string execTemplate = func.FunctionType == UserDefinedFunctionType.Scalar ? "SELECT " : "SELECT * FROM ";

                    execScript = string.Format("{0}\r\n {1} [{2}].[{3}]",
                                 UseDataBaseGo(func.Parent), execTemplate, func.Schema, func.Name);

                    string parameterList = "";
                    if (func.Parameters.Count > 0)
                    {

                        for (int i = 0; i < func.Parameters.Count; i++)
                        {
                            if (i > 0)
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
                log.Error("ExecuteFunctionfailed.", ex);
            }
        }

        public static String CreateFile(String script)
        {
            var str = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", new Object[] { Path.GetTempFileName(), "dtq" });
            var builder = new StringBuilder();

            builder.Append("[D035CF15-9EDB-4855-AF42-88E6F6E66540, 2.00]\r\n");
            builder.Append("Begin Query = \"Query1.dtq\"\r\n");
            builder.Append("Begin RunProperties =\r\n");
            builder.AppendFormat("{0}{1}{2}", "SQL = \"", script, "\"\r\n");
            builder.Append("ParamPrefix = \"@\"\r\n");
            builder.Append("ParamSuffix = \"\"\r\n");
            builder.Append("ParamSuffix = \"\\\"\r\n");
            builder.Append("End\r\n");
            builder.Append("End\r\n");

            using (var writer = new StreamWriter(str, false, Encoding.Unicode))
            {
                writer.Write(builder.ToString());
            }

            return str;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000")]
        public static void DesignTable(Table tbl, SqlConnectionInfo connInfo)
        {
            if (tbl.State == SqlSmoState.Dropped)
            {
                log.Info("trying to design dropped table.");
                return;
            }

            var mc = new ManagedConnection();
            mc.Connection = connInfo;

            ServiceCache.ScriptFactory.DesignTableOrView(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.DocumentType.Table,
                DocumentOptions.ManageConnection, tbl.Urn.ToString(), mc);
        }

        public static void OpenTable(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            if (objectToSelect.State == SqlSmoState.Dropped)
            {
                log.Info("trying to open dropped table.");
                return;
            }

            var _manager = new ObjectExplorerManager();
            _manager.OpenTable(objectToSelect, connection);
        }

        public static void ScriptTable(Table tbl, SqlConnectionInfo connInfo, bool scriptIndexies, bool scriptForeignKeys, bool scriptTriggers)
        {
            try
            {
                // create new document               
                var select = new StringBuilder(2000);

                lock (tbl)
                {
                    tbl.Refresh();
                    tbl.Columns.Refresh();

                    select.AppendLine(UseDataBaseGo(tbl.Parent));

                    foreach (var line in tbl.Script())
                    {
                        select.AppendLine("\t" + line);
                    }

                    if (scriptIndexies && tbl.Indexes.Count > 0)
                    {
                        tbl.Indexes.Refresh();
                        select.AppendLine();
                        select.AppendLine(" -- Indexes --");
                        select.AppendLine();


                        foreach (Index indexDef in tbl.Indexes)
                        {

                            foreach (var line in indexDef.Script())
                            {
                                select.AppendLine(line);
                            }

                            select.AppendLine();
                            select.AppendLine();
                        }

                    }


                    if (scriptForeignKeys && tbl.ForeignKeys.Count > 0)
                    {
                        tbl.ForeignKeys.Refresh();
                        select.AppendLine();
                        select.AppendLine(" -- Foreign Keys --");
                        select.AppendLine();

                        foreach (ForeignKey fk in tbl.ForeignKeys)
                        {
                            foreach (var line in fk.Script())
                            {
                                select.AppendLine(line);
                            }


                            select.AppendLine();
                            select.AppendLine();
                        }

                    }


                    if (scriptTriggers && tbl.Triggers.Count > 0)
                    {
                        tbl.Triggers.Refresh();
                        select.AppendLine();
                        select.AppendLine(" -- Triggers --");
                        select.AppendLine();

                        foreach (Trigger tr in tbl.Triggers)
                        {
                            bool hasAddedGoStatement = false;
                            foreach (var line in tr.Script())
                            {
                                // for a triggers we need GO statement before CREATE script
                                if (!hasAddedGoStatement && line.ToUpper().Trim() == "SET QUOTED_IDENTIFIER ON" )
                                {
                                    hasAddedGoStatement = true;
                                    select.AppendLine(line);
                                    select.AppendLine("GO");
                                }
                                else
                                 select.AppendLine(line);
                            }


                            select.AppendLine();
                            select.AppendLine();
                        }

                    }
                }

                CreateSQLDocumentWithHeader(select.ToString(), connInfo);
            }
            catch (Exception ex)
            {
                log.Error("Script Table failed.", ex);
            }
        }

        static StringBuilder BuildColumnNames(ColumnCollection columns, bool includeAllNames)
        {
            var selectColumns    = new StringBuilder(500);

            if (includeAllNames)          
            {
                selectColumns.AppendLine();

                bool needToAddComma = false;
                foreach (Column p in columns)
                {
                    if (needToAddComma)
                        selectColumns.AppendLine(",");

                    needToAddComma = true;

                    selectColumns.Append("\t\t");
                    selectColumns.Append("[" + p.Name + "]");
                }

            }
            else           
                 selectColumns.Append("*");
           
        

            return selectColumns;
        }
    


    }
}
