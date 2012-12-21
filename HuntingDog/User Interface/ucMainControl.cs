using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using DatabaseObjectSearcher;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Linq;
using System.Reflection;
using System.Collections;
using DevExpress.XtraEditors.Controls;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.SqlServer.Management.Common;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DatabaseObjectSearcherUI
{

    public enum ReqType : int
    {
        LoadObjects,
        Search,
        Details,
        Dependencies,
        Refresh,
        Navigate
    }



    [Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b9")]
    public partial class ucMainControl : System.Windows.Forms.UserControl
    {
        BackgroundProcessor _processor = new BackgroundProcessor();
        DatabaseSearchResult _details = null;
        List<ServerViewState> _serverViewList = new List<ServerViewState>();
        const int SearchLimit = 100;

        public ucMainControl()
        {
    
            DevExpress.Skins.SkinManager.EnableFormSkins();
            //This set the style to use skin technology
            DevExpress.LookAndFeel.UserLookAndFeel.Default.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Skin;

            //Here we specify the skin to use by its name                   
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("iMaginary");

            DevExpress.Utils.AppearanceObject.DefaultFont = new Font("Bitstream Vera Sans Mono",9);
        
            InitializeComponent();
            _processor.RequestFailed += new Action<BackgroundProcessor.Request, Exception>(_processor_RequestFailed);

            ucListView1.OnFocusedItemChanged += new ListHandler(ucListView1_OnFocusedItemChanged);
            ucListView1.OnDoubleClicked += new ListHandler(ucListView1_OnDoubleClicked);
            ucListView1.OnAction += new ListHandler<ActionArgs>(ucListView1_OnAction);
            try
            {
                MSSQLController.Current.LoadExistingServers();
            }
            catch (Exception ex)
            {
                memoStatus.Text = ex.ToString(); 
            }            

        }

        void ucListView1_OnAction(IListViewItem sender, ActionArgs args)
        {
            if (args.Action == EAction.Execute)
            {

                var res = (DatabaseSearchResult)sender.BoundObject;
                if (res.IsStoredProc)
                    ManagementStudioController.ExecuteStoredProc(res.Result as StoredProcedure, res.DataBase, res.Connection);
                else
                    ManagementStudioController.DesignTable(res.Result as Table, res.DataBase, res.Connection);
            }
            else if (args.Action == EAction.Locate)
            {
                LocateSMOObject(sender.BoundObject);
            }
        }

        void ucListView1_OnDoubleClicked(IListViewItem sender, EventArgs args)
        {
            OpenObject((DatabaseSearchResult)sender.BoundObject);
        }

        void ucListView1_OnFocusedItemChanged(IListViewItem sender, EventArgs args)
        {
            if (sender != null)
                // show details
                ShowDetails((DatabaseSearchResult)sender.BoundObject);
            else
                ShowDetails(null);
        }

        private ServerViewState SelectedServer
        {
            get
            {
                if (icbServer.SelectedItem == null)
                    return null;

                var item = (ImageComboBoxItem)icbServer.SelectedItem;
                return (ServerViewState)item.Value;
            }
        }

        private void txtSearchLine_TextChanged(object sender, EventArgs e)
        {
            DoSearch();
        }
       

        private void icbServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ServerViewState srvViewState = SelectedServer;
                if (srvViewState == null)
                    return;

                cbDataBase.Properties.Items.Clear();
                // populate databases

                System.Windows.Forms.CheckState defaultSState = srvViewState.SelectedDataBase == null ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
                foreach (var dbName in srvViewState.NavServer.DbSearcher.GetAvailableDataBases())
                {
                    cbDataBase.Properties.Items.Add(dbName);
                }

                // restore selected databases
                if (srvViewState.SelectedDataBase != null)
                {

                    foreach (string cbi in cbDataBase.Properties.Items)
                    {
                        if (cbi == srvViewState.SelectedDataBase)
                        {
                            cbDataBase.SelectedItem = cbi;
                            break;
                        }
                    }
                }

                if(cbDataBase.SelectedItem!=null)
                       DoSearch();
                
            }
            catch (Exception ex)
            {
                memoStatus.Text = ex.ToString();
            }

        }


        private void ucMainControl_Load(object sender, EventArgs e)
        {
            ucListView1.Factory = new SearchObjectFactory();
            ucDetails.Factory = new SearchObjectFactory();
            ucDependecyList.Factory = new SearchObjectFactory();
            listIndexies.Factory = new SearchObjectFactory();

            icbServer.Properties.Items.Clear();
            _serverViewList.Clear();
          

            Hashtable userSettings = LoadUserSettings();

            foreach (var srv in MSSQLController.Current.ServerList)
            {
                var serverState = new ServerViewState();
                serverState.NavServer = srv;

                _serverViewList.Add(serverState);

                if (userSettings.ContainsKey(srv.Name))
                {
                    serverState.SelectedDataBase = userSettings[srv.Name].ToString();
                }

                var item = new ImageComboBoxItem(srv.Name, serverState);
                icbServer.Properties.Items.Add(item);
            }

            foreach (var srv in MSSQLController.Current.ServerList)
            {
                _processor.AddRequest(LoadDataBaseObjectsAsync, srv, (int)ReqType.LoadObjects, false);
            }

            // select last server if possible???? 
            if (icbServer.Properties.Items.Count > 0)
                icbServer.SelectedItem = icbServer.Properties.Items[0];
        }

        // show exception in  status panel
        void _processor_RequestFailed(BackgroundProcessor.Request arg1, Exception ex)
        {
            Invoke(delegate() { 
                memoStatus.Text = ex.ToString(); 
                //progressBar.Visible = false;
            });
        }


        // Load objects from database using background thread BackgroundProcessor
        public void RefreshDataBaseAsync(object arg)
        {
            var src = (NavigatorServer)arg;
            Invoke(delegate() { SetStatus("Refreshing " + src.Name + "...", true); });

            src.DbSearcher.RefresBaseDictionary();
            src.DbSearcher.BuildDBObjectDictionary();

            // notify UI that db object have been read from DB
            Invoke(delegate() { SetStatus("Refreshed " + src.Name + ".", false); });
        }


        private void OpenObject(DatabaseSearchResult result)
        {
            if (result.ObjectType == ObjType.StoredProc)
            {

                ManagementStudioController.OpenStoredProcedureForModification(result.Connection, result.Result as StoredProcedure, result.DataBase);

            }
            else if (result.ObjectType == ObjType.Table)
            {
              
                ManagementStudioController.OpenTable(result.Result as Table, result.Connection);
            
            }
            else if (result.ObjectType == ObjType.Func)
            {
                ManagementStudioController.OpenFunctionForModification(result.Result as UserDefinedFunction, result.DataBase, result.Connection);
            }
            else if (result.ObjectType == ObjType.View)
            {
                ManagementStudioController.OpenView(result.Result as View, result.DataBase, result.Connection);

                try
                {
                    System.Windows.Forms.SendKeys.Send("{F5}");
                }
                catch { }
            }

        }

        private void LocateSMOObject(object arg)
        {

            DatabaseSearchResult res = (DatabaseSearchResult)arg;
            Invoke(delegate() { SetStatus("Locating" + res.Name, true); });

            MSSQLController.Current.SelectSMOObject(res.Result, res.Connection);

            Invoke(delegate() { SetStatus("Located " + res.Name, false); });
        }

        public void LoadDataBaseObjectsAsync(object arg)
        {
            var src = (NavigatorServer)arg;

            // load objects only once!
            if (!src.DbSearcher.HasObjectDictionary)
                RefreshDataBaseAsync(arg);
        }

        private void ShowDetails(DatabaseSearchResult res)
        {
            var sp = new DependencyParam() { Srv = SelectedServer.NavServer, Result = res };
        
            _processor.AddRequest(ShowDependeciesAsync, sp, (int)ReqType.Dependencies, true);
            _processor.AddRequest(ShowDetailsAsync, res, (int)ReqType.Details, true);
        }
        

        // Search for an object using a name - run in background thread
        public void SearchObjectAsync(object arg)
        {

            var sp = (SearchAsyncParam)arg;

            var textToSearch = sp.Text;
            Invoke(delegate() { SetStatus("Searching " + textToSearch, true); });

            sp.Srv.DbSearcher.SetFilter(sp.DBFilter);
            bool isMoreThanLimit, foundHitObject;
            var searchResult = sp.Srv.DbSearcher.FindMatchingObjects(textToSearch,SearchLimit,EResultBehaviour.ByUsage,
                _objectFilter,
                out isMoreThanLimit,out foundHitObject );

            Invoke(delegate() { SetStatus("Found " + searchResult.Count, false); });
            Invoke(delegate() { ucListView1.SetDataSource(searchResult); });

        }

        private void btnRefreshDbStructure_Click(object sender, EventArgs e)
        {
            if (SelectedServer == null)
                return;

            _processor.AddRequest(RefreshDataBaseAsync, SelectedServer.NavServer, (int)ReqType.Refresh, true);

            DoSearch();
        }

        class DependencyParam
        {
            public DatabaseSearchResult Result { get; set; }
            public NavigatorServer Srv { get; set; }
        }

        class SearchAsyncParam
        {
            public void SetDBFilter(string commaSeparatedNames)
            {
                DBFilter = new List<string>();
                foreach (var dbName in commaSeparatedNames.Split(','))
                {
                    DBFilter.Add(dbName.Trim());
                }
            }

            public NavigatorServer Srv { get; set; }
            public string Text { get; set; }
            public List<string> DBFilter;
        }

        private void DoSearch()
        {
            if (SelectedServer != null)
            {
                var sp = new SearchAsyncParam();
                sp.Srv = SelectedServer.NavServer;
                sp.Text = txtSearchLine.Text;
                sp.SetDBFilter(cbDataBase.EditValue.ToString());
                _processor.AddRequest(SearchObjectAsync, sp, (int)ReqType.Search, true);
            }
        }


        private void SetStatus(string status, bool showProgress)
        {
            memoStatus.Text = status;
            //progressBar.Visible = showProgress;
        }

        // helper to call:
        // Invoke(delegate() {  some code }) inside UI thread
        public void Invoke(System.Windows.Forms.MethodInvoker invoker)
        {
            Invoke((Delegate)invoker);
        }


        Hashtable LoadUserSettings()
        {
            Hashtable htSettings = new Hashtable();

            var dirName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Brave Energy Systems Pty Ltd\";
            var fullName = Path.Combine(dirName, _settingFileName);
            if (!File.Exists(fullName))
                return htSettings;

            try
            {
                using (TextReader reader = new StreamReader(fullName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
                    List<Entry> listSettings = (List<Entry>)serializer.Deserialize(reader);
                    if (listSettings != null)
                    {
                        foreach (var entry in listSettings)
                        {
                            htSettings.Add(entry.Key, entry.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.ToString(), false);
            }

            return htSettings;

        }

        private void ShowDependeciesAsync(object arg)
        {

            var param = (DependencyParam)arg;

            if (param.Result == null)
            {
                Invoke(delegate() { ucDependecyList.SetDataSource(null); });
                //Invoke(delegate() { lblDependencyTitle.Text = txt; });
                return;
            }

            Invoke(delegate() { progressDependency.Visible = true; progressDependency.Start(); });

            var dependencies = ManagementStudioController.GetDependencies(param.Result.Result, param.Result.Connection,param.Result.DataBase);
            var finalDependencies = param.Srv.DbSearcher.FindDependencyObjects(param.Result, dependencies);

            string txt = "";
            if(param.Result.IsStoredProc)
            {
                txt = "Stored Procedure "+param.Result.SchemaAndName+" uses the following objects:";
            }
            else if(param.Result.IsTable)
            {
                txt  ="Table "+param.Result.SchemaAndName+" is used by the following stored procs/functions/views:";
            }

            Invoke(delegate() { ucDependecyList.SetDataSource(finalDependencies); });
            //Invoke(delegate() { lblDependencyTitle.Text = txt; });
            Invoke(delegate() { progressDependency.Visible = false; progressDependency.Stop(); });
     
        }

        private void ShowDetailsAsync(object arg)
        {
            if (arg == null)
            {
                DiplayDetails(null, "", null, false);
                return;
            }

            var res = (DatabaseSearchResult)arg;
            res.Result.Refresh();

            List<Detail> detailList = new List<Detail>();
            string header = "";
            bool showIndexies = false;
            List<Detail> indexList = new List<Detail>();
            if (res.ObjectType == ObjType.Table)
            {
                showIndexies = true;
                header = "Columns";
                Table tbl = (Table)res.Result;
                lock (tbl)
                {
                    tbl.Columns.Refresh(true);
                    foreach (Column clm in tbl.Columns)
                    {
                        var d = new ColumnDetail() { PropertyName = clm.Name, PropertyValue = clm.DataType.Name };
                        d.isFK = clm.IsForeignKey;
                        d.isPK = clm.InPrimaryKey;
                        detailList.Add(d);
                       
                    }

                    foreach (Index i in tbl.Indexes)
                    {
                        var iDetails = new IndexDetail() { PropertyName = i.Name, IsClustered = i.IsClustered, FillFactor = i.FillFactor };
                        foreach (IndexedColumn clm in i.IndexedColumns)
                            iDetails.AddColumn(clm.Name);

                        indexList.Add(iDetails);
                    }

                    indexList.Sort();
                }

                _details = res;

            }
            else if (res.ObjectType == ObjType.StoredProc)
            {
                header = "Parameters";
                StoredProcedure sp = (StoredProcedure)res.Result;

                lock (sp)
                {
                    sp.Parameters.Refresh();
                    foreach (StoredProcedureParameter par in sp.Parameters)
                    {
                        var d = new ParamDetail() { PropertyName = par.Name, PropertyValue = par.DataType.Name };
                        d.Out = par.IsOutputParameter;
                        d.In = !d.Out;
                        d.DefaultValue = par.DefaultValue;
                        detailList.Add(d);

                    }
                }

                _details = res;

            }
            else if (res.ObjectType == ObjType.Func)
            {
                header = "Parameters";
                UserDefinedFunction func = (UserDefinedFunction)res.Result;

                lock (func)
                {
                    foreach (UserDefinedFunctionParameter par in func.Parameters)
                    {
                        detailList.Add(new Detail() { PropertyName = par.Name, PropertyValue = par.DataType.Name });
                    }
                }

                _details = res;
            }
            else if (res.ObjectType == ObjType.View)
            {
                header = "Columns";
                View v = (View)res.Result;

                lock (v)
                {
                    foreach (Column par in v.Columns)
                    {
                        detailList.Add(new Detail() { PropertyName = par.Name, PropertyValue = par.DataType.Name });
                    }
                }

                _details = res;
            }

            if (detailList != null)
                detailList.Sort();

            if (indexList != null)
                indexList.Sort();

            // show details in Details data grid
            DiplayDetails(detailList, header, indexList, showIndexies);
          
           
        }

        private void DiplayDetails(IEnumerable details, string header, IEnumerable indexies, bool showIndexies)
        {
                if (showIndexies)
                    Invoke(delegate() { listIndexies.SetDataSource(indexies); });

            Invoke(delegate() { xtraTabPage1.PageVisible = showIndexies; });
            Invoke(delegate() { pageObjDetails.Text = header; });
            Invoke(delegate() { ucDetails.SetDataSource(details); });
        }


        public const string _settingFileName = "SQLNavigator.xml";
        void SaveUserSettings()
        {
            var dirName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Brave Energy Systems Pty Ltd\";

            try
            {

                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);



                var fullName = Path.Combine(dirName, _settingFileName);

                List<Entry> htSelectedDbs = new List<Entry>();
                foreach (var srv in _serverViewList)
                {
                    htSelectedDbs.Add(new Entry(srv.NavServer.Name, srv.SelectedDataBase));
                }

                using (TextWriter textWriter = new StreamWriter(fullName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
                    serializer.Serialize(textWriter, htSelectedDbs);
                    textWriter.Flush();
                    textWriter.Close();
                }

            }
            catch (Exception ex1)
            {
                SetStatus(ex1.ToString(), false);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (SelectedServer == null)
                return;

            _processor.AddRequest(RefreshDataBaseAsync, SelectedServer.NavServer, (int)ReqType.Refresh, true);

            DoSearch();
        }

        private void cbDataBase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbDataBase.IsModified)
            {
                // perform search using new selected databases
                DoSearch();

                if (SelectedServer != null)
                    SelectedServer.SelectedDataBase = cbDataBase.EditValue.ToString();

                SaveUserSettings();
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            ManagementStudioController.ConnectNew();
           
        }



        ObjectFilter _objectFilter = new ObjectFilter() { ShowFunctions = true, ShowSP = true, ShowViews = true, ShowTables = true };
        private void FilterObjects()
        {
            DoSearch();
        }


        private void btnTable_CheckedStateChanged()
        {
            _objectFilter.ShowTables = btnTable.Checked;
            FilterObjects();
        }

        private void btnSP_CheckedStateChanged()
        {
            _objectFilter.ShowSP = btnSP.Checked;
            FilterObjects();
        }

        private void btnViews_CheckedStateChanged()
        {
            _objectFilter.ShowViews = btnViews.Checked;
            FilterObjects();
        }

        private void btnFunc_CheckedStateChanged()
        {
            _objectFilter.ShowFunctions = btnFunc.Checked;
            FilterObjects();
        }


        private void btnTable_CheckedStateChanged_1()
        {
            _objectFilter.ShowTables = btnTable.Checked;
            FilterObjects();
        }


        private void txtSearchLine_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Down || e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                ucListView1.SetFocus();
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                cbDataBase.Focus();
                e.Handled = true;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                cbDataBase.Focus();
                e.Handled = true;
            }
        }

        private void ucListView1_OnEscapePressed()
        {
            txtSearchLine.Focus();
        }

        private void ucListView1_OnLeaveSelection()
        {
            txtSearchLine.Focus();
        }

        private void cbDataBase_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Escape)
            {
                icbServer.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                txtSearchLine.Focus();
                e.Handled = true;
            }
        }

        private void icbServer_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                cbDataBase.Focus();
                e.Handled = true;
            }
        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }

 


    }
}
