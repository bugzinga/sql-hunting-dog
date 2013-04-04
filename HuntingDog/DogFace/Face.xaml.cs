
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DatabaseObjectSearcherUI;
using HuntingDog.DogEngine;
using HuntingDog.DogFace.Items;
using System.Windows.Documents;

namespace HuntingDog.DogFace
{
    class KeywordItem : Item
    {
        public KeywordItem(Item core)
        {
            Action1 = core.Action1;
            Action1Description = core.Action1Description;
            Action1Tooltip = core.Action1Tooltip;
            Action2 = core.Action2;
            Action2Description = core.Action2Description;
            Action2Tooltip = core.Action2Tooltip;
            Action3 = core.Action3;
            Action3Description = core.Action3Description;
            Action3Tooltip = core.Action3Tooltip;
            Action3Visibility = core.Action3Visibility;
            Action4 = core.Action4;
            Action4Description = core.Action4Description;
            Action4Tooltip = core.Action4Tooltip;
            Action4Visibility = core.Action4Visibility;
            Entity = core.Entity;
            Image = core.Image;
            IsChecked = core.IsChecked;
            IsMouseOver = core.IsMouseOver;
            MainObjectTooltip = core.MainObjectTooltip;
            Name = core.Name;
            NavigationTooltip = core.NavigationTooltip;
        }

        public String Keyword { get; set; }
    }

    /// <summary>
    /// Interaction logic for Face.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class Face : UserControl
    {
        private static readonly Log log = LogFactory.GetLog(typeof(Face));

        class SearchAsyncParam
        {
            public Int32 SequenceNumber
            {
                get;
                set;
            }

            public String Srv
            {
                get;
                set;
            }

            public String Text
            {
                get;
                set;
            }

            public String Database
            {
                get;
                set;
            }

            public Boolean ForceSearch
            {
                get;
                set;
            }
        }

        enum ERquestType : int
        {
            Server,
            RefreshSearch,
            Search,
            Details
        }

        // these keys are used to save/load user preferences
        public const String UserPref_LastSearchText = "Last Search Text";

        public const String UserPref_ServerDatabase = "[database]:";

        public const String UserPref_LastSelectedServer = "Last Selected Server";

        public const String ConnectNewServerString = "Connect New Server...";

        private static IStudioController _studio;

        private BackgroundProcessor _processor = new BackgroundProcessor();

        private UserPreferencesStorage _userPref;

        private ObservableCollection<Item> _serverList = new ObservableCollection<Item>();

        private Boolean _databaseChangedByUser = true;

        private Brush _borderBrush = new SolidColorBrush(Color.FromRgb(0x64, 0x95, 0xed));

        private Brush _blurBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x64, 0x95, 0xed));

        public Int64 LastTicks = 0;

        private volatile int _requestSequenceNumber = 0;

        private String _lastSrv;

        private String _lastDb;

        private String _lastText;

        private Boolean _isDragDropStartedFromText = false;

        // small hint - to use anonymous delegates in InvokeUI method
        public delegate void AnyInvoker();

        public IStudioController StudioController
        {
            get
            {
                if (_studio == null)
                {
                    _studio = HuntingDog.DogEngine.Impl.StudioController.Instance;
                }

                return _studio;
            }

            set
            {
                _studio = value;
            }
        }

        private Boolean IsConnectNewServerSellected
        {
            get
            {
                return (cbServer.SelectedItem == null)
                    ? false
                    : ((cbServer.SelectedItem as Item).Name == ConnectNewServerString);
            }
        }

        private String SelectedServer
        {
            get
            {
                return (cbServer.SelectedItem == null)
                    ? null
                    : (cbServer.SelectedItem as Item).Name;
            }
        }

        private ListViewItem SelectedListViewItem
        {
            get
            {
                return (itemsControl.SelectedItem == null)
                    ? null
                    : itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.SelectedItem) as ListViewItem;
            }
        }

        private Item SelectedItem
        {
            get
            {
                return (itemsControl.SelectedItem == null)
                    ? null
                    : itemsControl.SelectedItem as Item;
            }
        }

        private String SelectedDatabase
        {
            get
            {
                return (cbDatabase.SelectedItem == null)
                    ? null
                    : (cbDatabase.SelectedItem as Item).Name;
            }
        }

        public Face()
        {
            log.Message("XAML Face Constructed.");
            InitializeComponent();
        }

        private void UserControl_Loaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                log.Message("XAML Loaded...");

                var scroll = WpfUtil.FindChild<ScrollContentPresenter>(itemsControl);
                //scroll.SizeChanged += new SizeChangedEventHandler(scroll_SizeChanged);

                _userPref = UserPreferencesStorage.Load();

                _processor.RequestFailed += new Action<BackgroundProcessor.Request, Exception>(_processor_RequestFailed);
                StudioController.Initialise();
                StudioController.OnServersAdded += new Action<List<String>>(StudioController_OnServersAdded);
                StudioController.OnServersRemoved += new Action<List<String>>(StudioController_OnServersRemoved);
                StudioController.ShowYourself += new Action(StudioController_ShowYourself);
                ReloadServers();

                var lastSrvName = _userPref.GetByName(UserPref_LastSelectedServer);
                RestoreLastSearchTextFromUserProfile();

                //cbServer.SelectedValue = lastSrvName;
                //cbServer.SelectedItem = 

                // select first server
                if ((cbServer.SelectedIndex == -1) && (cbServer.Items.Count > 1))
                {
                    cbServer.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error("Fatal error loading main control:" + ex.Message, ex);
            }
        }

        void RestoreLastSearchTextFromUserProfile()
        {
            var lastSearch = _userPref.GetByName(UserPref_LastSearchText);

            if (String.IsNullOrEmpty(lastSearch))
            {
                txtSearch.Text = String.Empty;
            }
            else
            {
                // if search text contained same criteria TextChanged event will not be fired therefore search will not occur.
                // we need to force a search if search text is the same
                var prevText = txtSearch.Text;
                txtSearch.Text = lastSearch;

                if (prevText == txtSearch.Text)
                {
                    DoSearch(true);
                }
            }
        }

        void StudioController_ShowYourself()
        {
            txtSearch.Focus();
        }

        void StudioController_OnServersAdded(List<String> listAdded)
        {
            log.Message("Face: server added." + (listAdded.Count > 0 ? listAdded[0] : String.Empty));

            InvokeInUI(() =>
            {
                foreach (var item in ItemFactory.BuildServer(listAdded))
                {
                    _serverList.Add(item);
                }

                if (_serverList.Count == 1)
                {
                    cbServer.SelectedIndex = 0;
                    log.Message("Face: first server is selected.");
                }
            });
        }

        void StudioController_OnServersRemoved(List<String> removedList)
        {
            log.Message("Face: server removed." + (removedList.Count > 0 ? removedList[0] : ""));

            InvokeInUI(() =>
            {

                foreach (var s in _serverList.ToList())
                {
                    if (removedList.Contains(s.Name))
                    {
                        _serverList.Remove(s);
                    }
                }

                if (SelectedServer == null)
                {
                    // move selection to first available server                
                    if (_serverList.Count > 0)
                    {
                        log.Message("Face: Selected Server was removed. Move focus to first server in list");
                        cbServer.SelectedIndex = 0;
                    }
                    else
                    {
                        log.Message("Face: Selected Server was removed. Server list is empty.Clear everything");
                        cbDatabase.ItemsSource = null;
                    }
                }
            });
        }

        void StudioController_OnServersChanged()
        {
            log.Message("Face: server list changed.");

            InvokeInUI(() =>
            {
                ReloadServers();

                if ((cbServer.SelectedIndex == -1) && (cbServer.Items.Count > 1))
                {
                    cbServer.SelectedIndex = 0;
                }
                else if (cbServer.Items.Count == 0)
                {
                    cbDatabase.ItemsSource = null;
                }
            });
        }

        void _processor_RequestFailed(BackgroundProcessor.Request arg1, Exception arg2)
        {
            SetStatus("Error:" + arg2.Message);
            // notify user about an error
            log.Error("Request failed:" + arg1.Argument + " type:" + arg1.RequestType, arg2);
        }

        public void ReloadServers()
        {
            log.Message("Reloading Servers - initiated by user");
            var servers = StudioController.ListServers();
            _serverList.Clear();

            foreach (var item in ItemFactory.BuildServer(servers))
            {
                _serverList.Add(item);
            }

            //srv.Add(new Item() { Name = ConnectNewServerString });
            log.Message("Reloading Servers - loaded:" + _serverList.Count + " servers.");
            cbServer.ItemsSource = _serverList;

            if (_serverList.Count == 1)
            {
                cbServer.SelectedIndex = 0;
            }

            // _processor.AddRequest(Async_ReloadServers, servers, (int)ERquestType.Server,true);
        }

        private void SetStatus(String text)
        {
            SetStatus(text, false);
        }

        private void SetStatus(String text, Boolean showImage)
        {
            InvokeInUI(() =>
            {
                imgWorking.Visibility = showImage
                    ? Visibility.Visible
                    : Visibility.Hidden;
                txtStatusTest.Text = text;
            });
        }

        // Reload database list
        private void Async_ReloadDatabaseList(Object arg)
        {
            if (arg is String)
            {
                var server = (String) arg;
                StudioController.RefreshServer(server);

                InvokeInUI(() =>
                {
                    ReloadDatabaseList();
                });
            }
        }

        // Reload objects in database
        private void Async_ReloadObjectsFromDatabase(Object arg)
        {
            var pair = (KeyValuePair<String, String>) arg;

            var server = pair.Key;
            var database = pair.Value;

            SetStatus("Reloading " + database + "...", true);

            StudioController.RefreshDatabase(server, database);

            InvokeInUI(() =>
            {
                DoSearch(true);
            });

            SetStatus("Completed reloading " + database);
        }

        void ReloadDatabaseList()
        {
            try
            {
                var sel = cbServer.SelectedItem as Item;

                if (sel != null)
                {
                    cbDatabase.ItemsSource = ItemFactory.BuildDatabase(StudioController.ListDatabase(sel.Name));

                    _databaseChangedByUser = false;

                    // changed server - try to restore database user worked with last time
                    var databaseName = _userPref.GetByName(UserPref_ServerDatabase + sel.Name);
                    var previousDatabaseWasFound = false;

                    if ((databaseName != null) && (cbDatabase.Items != null))
                    {
                        foreach (Item item in cbDatabase.ItemsSource)
                        {
                            if (item.Name == databaseName)
                            {
                                cbDatabase.SelectedValue = item;
                                previousDatabaseWasFound = true;
                                break;
                            }
                        }
                    }

                    // select the first database if a database previously chosen by user does not exist on a server any more
                    if (!previousDatabaseWasFound && (cbDatabase.Items != null) && (cbDatabase.Items.Count > 0))
                    {
                        cbDatabase.SelectedIndex = 0;
                    }

                    _databaseChangedByUser = true;
                    _userPref.StoreByName(UserPref_LastSelectedServer, sel.Name);

                    //if(SelectedDatabase==null)
                    if (previousDatabaseWasFound)
                    {
                        // we managed to find our database - restore search text
                        RestoreLastSearchTextFromUserProfile();
                        txtSearch.Focus();
                    }
                    else
                    {
                        ClearSearchText();
                        cbDatabase.Focus();
                    }

                    //cbDatabase.IsDropDownOpen = true;
                }
                else
                {
                    ClearSearchText();
                }
            }
            catch (Exception ex)
            {
                log.Error("Server Selection:" + ex.Message, ex);
            }
        }

        private void cbServer_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ReloadDatabaseList();

            // keep track of last selected database on this server - need to restore it back!
            //DoSearch();
        }

        private void cbDatabase_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (_databaseChangedByUser)
            {
                if ((SelectedServer != null) && (SelectedDatabase != null))
                {
                    StoreSelectedDatabaseInUserProfile();
                    txtSearch.Focus();
                    DoSearch(false);
                }
            }
        }

        private void StoreSelectedDatabaseInUserProfile()
        {
            if ((SelectedServer != null) && (SelectedDatabase != null))
            {
                _userPref.StoreByName(UserPref_ServerDatabase + SelectedServer, SelectedDatabase);
            }
        }

        // TODO: Commented as never used. Check whether the existance of the files
        //       is actually needed.
        //bool _isDragDropStartedPropeties = false;

        private void txtSearch_TextChanged(Object sender, TextChangedEventArgs e)
        {
            DoSearch(false);
        }

        void DoSearch(Boolean forceSearch)
        {
            try
            {
                if (!String.IsNullOrEmpty(txtSearch.Text) && (SelectedServer != null) && (SelectedDatabase != null))
                {
                    var sp = new SearchAsyncParam();
                    sp.SequenceNumber = ++_requestSequenceNumber;
                    sp.Srv = SelectedServer;
                    sp.Text = txtSearch.Text;
                    sp.Database = SelectedDatabase;
                    sp.ForceSearch = forceSearch;
                    _processor.AddRequest(Async_PerformSearch, sp, (int) ReqType.Search, true);
                    _userPref.StoreByName(UserPref_LastSearchText, txtSearch.Text);
                }
                else
                {
                    ClearSearchResult();
                }
            }
            catch (Exception ex)
            {
                log.Error("Face - Do Search:" + ex.Message, ex);
            }
        }

        void ClearSearchText()
        {
            txtSearch.Text = string.Empty;
        }

        void ClearSearchResult()
        {
            itemsControl.ItemsSource = null;
            ClearPreviousSearch();
            SetStatus(String.Empty);
        }

        private void ClearPreviousSearch()
        {
            _lastText = null;
        }

        private Boolean SameAsPreviousSearch(SearchAsyncParam arg)
        {
            if (arg.ForceSearch)
            {
                return false;
            }

            if ((arg.Srv == _lastSrv) && (arg.Database == _lastDb) && (_lastText != null) && (arg.Text.TrimEnd(' ') == _lastText.TrimEnd(' ')))
            {
                return true;
            }

            _lastSrv = arg.Srv;
            _lastDb = arg.Database;
            _lastText = arg.Text;

            return false;
        }

        private void Async_PerformSearch(Object arg)
        {
            if (arg == null)
            {
                return;
            }

            var par = (SearchAsyncParam) arg;

            // new request was added - this one is outdated
            if (par.SequenceNumber < _requestSequenceNumber)
            {
                return;
            }

            if (SameAsPreviousSearch(par))
            {
                return;
            }

            SetStatus("Searching '" + par.Text + "' in " + par.Database, true);

            var result = StudioController.Find(par.Srv, par.Database, par.Text);

            //MyLogger.LogMessage("Searching " + par.Text + " in server:" + par.Srv + " database:" + par.Database);

            // new request was added - this one is outdated
            if (par.SequenceNumber < _requestSequenceNumber)
            {
                log.Message("Cancelled search request because new request was added. " + par.Text);
                return;
            }

            SetStatus("Found " + result.Count + " objects");

            InvokeInUI(() =>
            {
                var items = ItemFactory.BuildFromEntries(result);
                var keywordItems = new ObservableCollection<KeywordItem>();

                foreach (var item in items)
                {
                    keywordItems.Add(new KeywordItem(item) { Keyword = par.Text });
                }

                itemsControl.ItemsSource = keywordItems;
                itemsControl.SelectedIndex = -1;
                itemsControl.ScrollIntoView(itemsControl.SelectedItem);

                if (items.Count == 0)
                {
                    gridEmptyResult.Visibility = System.Windows.Visibility.Visible;
                    itemsControl.Visibility = System.Windows.Visibility.Collapsed;

                    // txtEmptyLine1.Text =  par.Text;
                    //txtEmptyLine2.Text =  par.Database ;
                    //txtEmptyLine3.Text = par.Srv;
                }
                else
                {
                    gridEmptyResult.Visibility = System.Windows.Visibility.Collapsed;
                    itemsControl.Visibility = System.Windows.Visibility.Visible;
                }
            });

            if (result.Count == 0)
            {
                SetStatus("Found nothing. Try to refresh");
            }
            else if (result.Count == 1)
            {
                SetStatus("Found exactly one object");
            }
            else
            {
                SetStatus("Found " + result.Count + " objects ");
            }
        }

        private void InvokeInUI(AnyInvoker invoker)
        {
            Dispatcher.Invoke((Delegate) invoker);
        }

        /*
                private void Async_ShowProperties(object arg)
                {
                    //SystemColors.ControlLightBrushKey
                    var ent = arg as Entity;
                    if (ent == null)
                        return;
        
         
                    SetStatus("Retreveing details...",true);
                    if (ent.IsProcedure)
                    {
                        var procedureParameters = StudioController.ListProcParameters(ent);

                          InvokeInUI(delegate {
                              txtPropertiesParameter.Text = "Parameter";
                             listViewProperties.ItemsSource = ItemFactory.BuildProcedureParmeters(procedureParameters);  });
                    }
                    else if (ent.IsTable)
                    {
                        var columns = StudioController.ListColumns(ent);
                
                         InvokeInUI(delegate {
                             txtPropertiesParameter.Text = "Column";
                         listViewProperties.ItemsSource =  ItemFactory.BuildTableColumns(columns);});
                    }
                    else if (ent.IsFunction)
                    {
                        var funcParameters = StudioController.ListFuncParameters(ent);
                
                          InvokeInUI(delegate {
                              txtPropertiesParameter.Text = "Parameter";
                         listViewProperties.ItemsSource =  ItemFactory.BuildProcedureParmeters(funcParameters);});
                    }
                    else if (ent.IsView)
                    {
                        var columns = StudioController.ListViewColumns(ent);
                
                          InvokeInUI(delegate {
                              txtPropertiesParameter.Text = "Column";
                         listViewProperties.ItemsSource =  ItemFactory.BuildViewColumns(columns);});
                    }

                    SetStatus("");

                }*/

        private void ItemsControlSelectionChanged1(Object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                //var removedItem = (e.RemovedItems[0] as Item);
                //var previousPopup = GetPopupFromItem(removedItem);
                //if (previousPopup != null)
                //    previousPopup.IsOpen = false;
            }

            //listViewProperties.ItemsSource = null;
            if (itemsControl.SelectedIndex == -1)
            {
                // clear properties
                //listViewProperties.ItemsSource = null;
            }
            else
            {
                var item = (itemsControl.SelectedItem as Item);

                if (item != null)
                {
                    // DO NOT SHOW PROPERTIES
                    //_processor.AddRequest(Async_ShowProperties, item.Entity, (int)ReqType.Details, true); 
                }

            }
        }

        public void Stop()
        {
            _userPref.Save();

            if (_processor != null)
            {
                _processor.Stop();
                _processor = null;
            }
        }

        private void UserControl_Unloaded(Object sender, RoutedEventArgs e)
        {
            Stop();
        }

        internal IEnumerable<String> BuilsAvailableActions(Item item)
        {
            var res = new List<String>();

            if (item.Entity.IsProcedure)
            {
                return new List<String> { "Modify", "Execute", "Locate", };
            }

            if (item.Entity.IsFunction)
            {
                return new List<String> { "Modify", "Execute", "Locate", };
            }
            else if (item.Entity.IsTable)
            {
                return new List<String> { "Select Data", "Edit Data", "Design Table", "Script Table", "Locate", };
            }
            else if (item.Entity.IsView)
            {
                return new List<String> { "Select Data", "Modify View", "Locate", };
            }

            return res;
        }

        void InvokeActionByName(Item item, String actionName)
        {
            if ((item == null) || String.IsNullOrEmpty(actionName))
            {
                return;
            }

            if (actionName == "Locate")
            {
                StudioController.NavigateObject(SelectedServer, item.Entity);
                return;
            }

            if (actionName == "Show Dependencies")
            {
                // show dependencies in a new window
            }

            if (item.Entity.IsTable)
            {
                switch (actionName)
                {
                    case "Select Data":
                        StudioController.SelectFromTable(SelectedServer, item.Entity);
                        break;
                    case "Edit Data":
                        StudioController.EditTableData(SelectedServer, item.Entity);
                        break;
                    case "Design Table":
                        StudioController.DesignTable(SelectedServer, item.Entity);
                        break;
                    case "Script Table":
                        StudioController.ScriptTable(SelectedServer, item.Entity);
                        break;
                }
            }
            else if (item.Entity.IsView)
            {
                switch (actionName)
                {
                    case "Select Data":
                        StudioController.SelectFromView(SelectedServer, item.Entity);
                        break;
                    case "Modify View":
                        StudioController.ModifyView(SelectedServer, item.Entity);
                        break;
                }
            }
            else if (item.Entity.IsProcedure)
            {
                switch (actionName)
                {
                    case "Modify":
                        StudioController.ModifyProcedure(SelectedServer, item.Entity);
                        break;
                    case "Execute":
                        StudioController.ExecuteProcedure(SelectedServer, item.Entity);
                        break;

                }
            }
            else if (item.Entity.IsFunction)
            {
                switch (actionName)
                {
                    case "Modify":
                        StudioController.ModifyFunction(SelectedServer, item.Entity);
                        break;
                    case "Execute":
                        StudioController.ExecuteFunction(SelectedServer, item.Entity);
                        break;
                }
            }
        }

        void InvokeDefaultOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.SelectFromTable(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ModifyProcedure(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ModifyFunction(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsView)
            {
                StudioController.SelectFromView(SelectedServer, item.Entity);
            }
        }

        void InvokeAdditionalActionOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.DesignTable(SelectedServer, item.Entity);
            }
        }

        void InvokeActionOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.EditTableData(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ExecuteProcedure(SelectedServer, item.Entity);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ExecuteFunction(SelectedServer, item.Entity);
            }
        }

        void InvokeNavigationOnItem(Item item)
        {
            StudioController.NavigateObject(SelectedServer, item.Entity);
        }

        private void btnNavigationClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item) ((FrameworkElement) sender).Tag;
            InvokeNavigationOnItem(item);
        }

        private void btnActionClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item) ((FrameworkElement) sender).Tag;
            InvokeActionOnItem(item);
        }

        private void btnAdditonalActionClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item) ((FrameworkElement) sender).Tag;
            InvokeAdditionalActionOnItem(item);
        }

        private void DefaultAction_Click(Object sender, RoutedEventArgs e)
        {
            var item = (Item) ((FrameworkElement) sender).Tag;
            InvokeDefaultOnItem(item);
        }

        private void txtSearch_KeyDown(Object sender, KeyEventArgs e)
        {
        }

        private void txtSearch_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Tab) || (e.Key == Key.Enter) || (e.Key == Key.Down))
            {
                if (itemsControl.Items.Count > 0)
                {
                    // move focus to result list view
                    //txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                    MoveFocusItemsControl(false);

                }

                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Up));
                e.Handled = true;
            }
        }

        private Boolean MoveFocusItemsControl(Boolean isLast)
        {
            if (itemsControl.Items.Count > 0)
            {
                var index = isLast
                    ? (itemsControl.Items.Count - 1)
                    : 0;

                itemsControl.SelectedIndex = index;

                var it = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as Control;

                if (it != null)
                {
                    return it.Focus();
                }
                else
                {
                    itemsControl.ScrollIntoView(itemsControl.Items[index]);

                    var it1 = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as Control;

                    if (it1 != null)
                    {
                        return it1.Focus();
                    }
                }
            }

            return false;
        }

        private void itemsControl_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (!IsItemFocused(itemsControl))
            {
                return;
            }

            if (e.Key == Key.Tab)
            {
                itemsControl.SelectedIndex = -1;

                // move focus to result text
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }
            else if ((e.Key == Key.Up) && (itemsControl.SelectedIndex == 0))
            {
                itemsControl.SelectedIndex = -1;

                // jump to text search box from Result View - 
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }
            else if ((e.Key == Key.Down) && (itemsControl.SelectedIndex == (itemsControl.Items.Count - 1)))
            {
                // last item - do nothing
                e.Handled = true;
            }
            else if (((e.Key == Key.Enter) || (e.Key == Key.Space)) && itemsControl.SelectedIndex != -1)
            {
                // open popup control and move focus to teh first item there
                OpenContextMenu();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                if (itemsControl.SelectedIndex != -1)
                {
                    //MoveFocusItemsControl(true);
                    InvokeActionOnItem(itemsControl.SelectedItem as Item);
                }

                e.Handled = true;

            }
            else if (e.Key == Key.Left)
            {
                //MoveFocusItemsControl(false);
                e.Handled = true;
            }
        }

        private void cbDatabase_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (((e.Key == Key.Enter) || (e.Key == Key.Space)) && !cbDatabase.IsDropDownOpen)
            {
                cbDatabase.IsDropDownOpen = true;
                e.Handled = true;
            }

            if (((e.Key == Key.Down) || (e.Key == Key.Right)) && !cbDatabase.IsDropDownOpen)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if (((e.Key == Key.Up) || (e.Key == Key.Left)) && !cbDatabase.IsDropDownOpen)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }

            if ((e.Key == Key.Space) && cbDatabase.IsDropDownOpen)
            {
                foreach (var item in cbDatabase.Items)
                {
                    var it = cbDatabase.ItemContainerGenerator.ContainerFromItem(item);

                    if ((it as ComboBoxItem).IsHighlighted)
                    {
                        cbDatabase.SelectedItem = item;
                        cbDatabase.IsDropDownOpen = false;
                    }
                }
            }
        }

        private void cbServer_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (((e.Key == Key.Enter) || (e.Key == Key.Space)) && !cbServer.IsDropDownOpen)
            {
                cbServer.IsDropDownOpen = true;
                e.Handled = true;
            }

            if ((e.Key == Key.Down) && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Down));
                e.Handled = true;
            }

            if ((e.Key == Key.Right) && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if (((e.Key == Key.Up) || (e.Key == Key.Left)) && !cbServer.IsDropDownOpen)
            {
                e.Handled = true;
            }

            if ((e.Key == Key.Space) && cbServer.IsDropDownOpen)
            {
                foreach (var item in cbServer.Items)
                {
                    var it = cbServer.ItemContainerGenerator.ContainerFromItem(item);

                    if ((it as ComboBoxItem).IsHighlighted)
                    {
                        cbServer.SelectedItem = item;
                        cbServer.IsDropDownOpen = false;
                    }
                }
            }
        }

        private void TextBlock_MouseUp(Object sender, MouseEventArgs e)
        {
            _isDragDropStartedFromText = false;
        }

        private void TextBlock_MouseDown_1(Object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // show be treated as Enter need to move focus and open Popup
                if (itemsControl.SelectedItem != null)
                {
                    var controlSelected = itemsControl.ItemContainerGenerator.ContainerFromItem(SelectedItem);
                }

                //OpenPopup();
            }
            else
            {
                _isDragDropStartedFromText = true;

                if ((DateTime.Now.Ticks - LastTicks) < 3000000)
                {
                    if (itemsControl.SelectedItem != null)
                    {
                        InvokeDefaultOnItem(itemsControl.SelectedItem as Item);
                    }

                }

                LastTicks = DateTime.Now.Ticks;
            }
        }

        private void PropertiesTextBlock_MouseUp(Object sender, MouseEventArgs e)
        {
            //_isDragDropStartedPropeties = false;
        }

        private void PropertiesTextBlock_MouseDown(Object sender, MouseEventArgs e)
        {
            //_isDragDropStartedPropeties = true;           
        }

        private void TextBlock_MouseMove(Object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && _isDragDropStartedFromText)
            {
                _isDragDropStartedFromText = false;

                // Get the dragged ListViewItem
                FrameworkElement item = sender as FrameworkElement;

                if (item != null)
                {
                    ListViewItem listViewItem = WpfUtil.FindAncestor<ListViewItem>(item);

                    // Find the data behind the ListViewItem
                    Item contact = (Item) itemsControl.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    if ((contact != null) && (contact.Entity != null))
                    {
                        // Initialize the drag & drop operation
                        DragDrop.DoDragDrop(listViewItem, contact.Entity.FullName, DragDropEffects.Copy);
                    }
                }
            }
        }

        private void PropertiesTextBlock_MouseMove(Object sender, MouseEventArgs e)
        {
            /*if (e.LeftButton == MouseButtonState.Pressed && _isDragDropStartedPropeties == true)
            {
                _isDragDropStartedPropeties = false;

                try
                {
                    // Get the dragged ListViewItem
                    FrameworkElement item = sender as FrameworkElement;

                    if (item != null)
                    {
                        ListViewItem listViewItem = WpfUtil.FindAncestor<ListViewItem>(item);

                        // Find the data behind the ListViewItem
                        BaseParamItem pr = (BaseParamItem)listViewProperties.ItemContainerGenerator.ItemFromContainer(listViewItem);

                        if (pr != null)
                        {
                            // Initialize the drag & drop operation
                            DragDrop.DoDragDrop(listViewItem, pr.Name, DragDropEffects.Copy);
                        }
                    }
                }
                catch
                {
                    // LOG
                }
            }*/
        }

        private void cbDatabase_GotFocus(Object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = _borderBrush;
            cbDatabase.BorderBrush = _borderBrush;
        }

        private void cbDatabase_LostFocus(Object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = Brushes.Transparent;
            //cbDatabase.BorderBrush = _blurBrush;
        }

        private void cbServer_GotFocus(Object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = _borderBrush;
            //cbServer.BorderBrush = _borderBrush;
        }

        private void cbServer_LostFocus(Object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = Brushes.Transparent;
            //cbServer.BorderBrush = _blurBrush;
        }

        private void txtSearch_GotFocus(Object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _borderBrush;
            imgSearch.Opacity = 1;
            txtSearch.SelectAll();
        }


        private void txtSearch_SelectSearchText(object sender, RoutedEventArgs e)
        {

            TextBox tb = (sender as TextBox);

            if (tb != null)
            {

                tb.SelectAll();

            }

        }



        private void txtSearch_SelectivelyIgnoreMouseButton(object sender,

            MouseButtonEventArgs e)
        {

            TextBox tb = (sender as TextBox);

            if (tb != null)
            {

                if (!tb.IsKeyboardFocusWithin)
                {

                    e.Handled = true;

                    tb.Focus();

                }

            }

        }

        private void txtSearch_LostFocus(Object sender, RoutedEventArgs e)
        {
            imgSearch.Opacity = 0.5;
            borderText.BorderBrush = _blurBrush;
        }

        private void borderText_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            txtSearch.Focus();
        }

        private void Refresh_Click(Object sender, RoutedEventArgs e)
        {
            ReloadObjectsFromDatabase();
        }

        private void ReloadObjectsFromDatabase()
        {
            if (SelectedServer != null)
            {
                if (SelectedDatabase != null)
                {
                    _processor.AddRequest(Async_ReloadObjectsFromDatabase, new KeyValuePair<String, String>(SelectedServer, SelectedDatabase), (Int32) ERquestType.RefreshSearch, true);
                }
            }
        }

        private void RefreshDatabaseList_Click(Object sender, RoutedEventArgs e)
        {
            StoreSelectedDatabaseInUserProfile();
            ReloadObjectsFromDatabase();

            if (SelectedServer != null)
            {
                _processor.AddRequest(Async_ReloadDatabaseList, SelectedServer, (Int32) ERquestType.Server, true);
            }
        }

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        //private void itemsControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    OpenPopup();
        //}

        ContextMenu GetCtxMenuFromItem(Item si)
        {
            if (si == null)
            {
                return null;
            }

            var cont = itemsControl.ItemContainerGenerator.ContainerFromItem(si);

            if (cont != null)
            {
                return (cont as ListViewItem).ContextMenu;
            }

            return null;
        }

        private void ListViewContextMenuClosing(Object sender, ContextMenuEventArgs e)
        {
        }

        private void UnsubscribeToAction(ContextMenu ctx)
        {
            foreach (var item in ctx.Items)
            {
                var menuItem = ctx.ItemContainerGenerator.ContainerFromItem(item) as MenuItem;

                if (menuItem != null)
                {
                    menuItem.Click -= menuItem_Click;
                }
            }
        }

        private void ListViewContextMenuOpening(Object sender, ContextMenuEventArgs e)
        {
            var li = sender as ListViewItem;

            if (li != null)
            {
                var ctx = li.ContextMenu;

                // happens only when user does a righ click
                ctx.Placement = PlacementMode.MousePoint;
                ctx.HorizontalOffset = 0;
                ctx.VerticalOffset = 4;
                ctx.ItemsSource = BuilsAvailableActions(SelectedItem);
            }
        }

        private void SubscribeToAction(ContextMenu ctx)
        {
            var focused = false;

            foreach (var item in ctx.Items)
            {
                var menuItem = ctx.ItemContainerGenerator.ContainerFromItem(item) as MenuItem;

                if (menuItem != null)
                {
                    menuItem.Click += menuItem_Click;

                    if (!focused)
                    {
                        menuItem.Focus();
                    }

                    focused = true;
                }
            }
        }

        void menuItem_Click(Object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem != null)
            {
                var cmd = menuItem.Header.ToString();
                InvokeActionByName(SelectedItem, cmd);
            }
        }

        void PrepareContextMenu(ContextMenu ctx, Item item)
        {
        }

        void OpenContextMenu()
        {
            var ctx = GetCtxMenuFromItem(SelectedItem);

            if (ctx != null)
            {
                ctx.Placement = PlacementMode.Left;
                ctx.PlacementTarget = SelectedListViewItem;
                ctx.HorizontalOffset = itemsControl.ActualWidth / 2;
                ctx.VerticalOffset = SelectedListViewItem.ActualHeight / 2;

                //var listViewItem = SelectedListViewItem;
                //var txt = WpfUtil.FindChild<TextBlock>(SelectedListViewItem);

                //ctx.PlacementTarget = txt;

                ctx.ItemsSource = BuilsAvailableActions(SelectedItem);
                ctx.IsOpen = true;

                // var lv = (popup.Child as Border).Child as ListView;
                //  lv.ItemsSource = BuilsAvailableActions(SelectedItem);
            }
        }

        private Boolean IsItemFocused(ListView lv)
        {
            var it = GetSelectedItem(lv);

            return (it != null)
                ? it.IsFocused
                : false;
        }

        private Control GetSelectedItem(ListView lv)
        {
            return (lv.SelectedIndex >= 0)
                ? lv.ItemContainerGenerator.ContainerFromIndex(lv.SelectedIndex) as Control
                : null;
        }

        private void ContextMenu_Closed(Object sender, RoutedEventArgs e)
        {
            UnsubscribeToAction(sender as ContextMenu);
        }

        private void ContextMenu_Opened(Object sender, RoutedEventArgs e)
        {
            SubscribeToAction(sender as ContextMenu);
        }

        /*private void popupListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Left  || e.Key == Key.Right)
            {
                var lv = sender as ListView;
                var popup = (lv.Parent as Border).Parent as Popup;
  
                if(popup!=null)
                    popup.IsOpen = false;

                if (itemsControl.SelectedItem != null)
                {
                    var it1 = GetSelectedItem(itemsControl);
                    if (it1 != null)
                    {
                        it1.Focus();
                    }
                }
                            
                e.Handled = true;
            }
            else if(e.Key == Key.Up)
            {
                var lv = sender as ListView;
                if(lv.SelectedIndex == 0)
                    e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                 var lv = sender as ListView;
                 if (lv.SelectedIndex == lv.Items.Count-1)
                     e.Handled = true;

            }
            else if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                // invoke action
                var lv = sender as ListView;
                if (lv != null && lv.SelectedItem!=null)
                {
                    InvokeActionByName(SelectedItem, lv.SelectedItem.ToString());
                }

                var popup = (lv.Parent as Border).Parent as Popup;
                if (popup != null)
                    popup.IsOpen = false;

                //if (itemsControl.SelectedItem != null)
                //{
                //    var it1 = GetSelectedItem(itemsControl);
                //    if (it1 != null)
                //    {
                //        it1.Focus();
                //    }
                //}

                e.Handled = true;
            }
        }*/

        //private void myPopup_Opened(object sender, EventArgs e)
        //{
        //    var popup = sender as Popup;

        //    var lv = (popup.Child as Border).Child as ListView;
        //    if (lv != null)
        //    {
        //        var selectedItem = SelectedItem;
        //        if (selectedItem != null)
        //        {
        //            //lv.ItemsSource = BuilsAvailableActions(selectedItem);
        //            lv.SelectedIndex = 0;
        //            var firstElement = lv.ItemContainerGenerator.ContainerFromIndex(0) as UIElement;
        //            if(firstElement!=null)
        //             firstElement.Focus();
        //        }
        //    }
        //    //this.Child.MoveFocus(new TraversalRequest(
        //    //        FocusNavigationDirection.Next));
        //}

        //private void itemsControl_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    borderItems.BorderBrush = _borderBrush;
        //}

        //private void itemsControl_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    borderItems.BorderBrush = Brushes.Transparent;
        //}
    }
}