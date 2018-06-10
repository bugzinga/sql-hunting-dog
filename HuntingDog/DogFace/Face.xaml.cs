
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
using HuntingDog.Core;
using HuntingDog.DogEngine;
using HuntingDog.DogFace.Background;
using System.Windows.Media.Effects;

namespace HuntingDog.DogFace
{
    [ComVisible(false)]
    public partial class Face : UserControl
    {
        protected readonly Log log = LogFactory.GetLog();

        class SearchAsyncParam
        {
            public Int32 SequenceNumber
            {
                get;
                set;
            }

            public IServer Srv
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

            public override String ToString()
            {
                return String.Format("Server: {0}, Database: {1}, Text: '{2}'", Srv, Database, Text);
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

        private IServer _lastSrv;

        private String _lastDb;

        private String _lastText;

        private Boolean _isDragDropStartedFromText = false;

        private UpdateDetector UpdateDetector;

        // small hint - to use anonymous delegates in InvokeUI method
        public delegate void AnyInvoker();

        public int ResultsFontSize { get; set; }

        public IStudioController StudioController
        {
            get
            {
                if (_studio == null)
                {
                    _studio = HuntingDog.DogEngine.Impl.DiConstruct.Instance;
                }

                return _studio;
            }

            set
            {
                _studio = value;
            }
        }

        private IServer SelectedServer
        {
            get
            {
                return (cbServer.SelectedItem == null)
                    ? null
                    : (cbServer.SelectedItem as Item).Server;
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
            log.Info("XAML Face Constructed.");
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(Object sender, RoutedEventArgs e)
        {
            try
            {
                log.Info("XAML Loaded...");

                var scroll = itemsControl.FindChild<ScrollContentPresenter>();

                _userPref = UserPreferencesStorage.Load();
                _cfg = _persistor.Restore<Config.DogConfig>(_userPref);

                UpdateDetector = new UpdateDetector(_userPref);
                UpdateDetector.NewVersionFound += UpdateDetector_NewVersionFound;

                _processor.RequestFailed += new Action<Request, Exception>(_processor_RequestFailed);
                StudioController.Initialise();
                StudioController.SetConfiguration(_cfg);
                StudioController.OnServersAdded += StudioController_OnServersAdded;
                StudioController.OnServersRemoved += StudioController_OnServersRemoved;
                StudioController.ShowYourself += new System.Action(StudioController_ShowYourself);
                ReloadServers();

                ResultsFontSize = _cfg.FontSize;

                var lastSrvName = _userPref.GetByName(UserPref_LastSelectedServer);
                RestoreLastSearchTextFromUserProfile();

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



        void UpdateDetector_NewVersionFound(DogVersion detected)
        {
            InvokeInUI(() =>
            {
                popupBorder.Visibility = System.Windows.Visibility.Visible;
                popupUpdateText.Text = string.Format("New version is available ({0})", detected.Version.ToString());
            });
        }

        private void CloseUpdate_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                popupBorder.Visibility = System.Windows.Visibility.Collapsed;
                UpdateDetector.IgnoreVersion();
                _userPref.Save();
            }
            catch (Exception ex)
            {
                log.Error("Ignore new version failed", ex);
            }

        }

        private void DownloadUpdate_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateDetector.Download();
            }
            catch (Exception ex)
            {
                log.Error("Error opening download" + ex.Message, ex);
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
            log.Info("StudioController_ShowYourself");
            txtSearch.Focus();
        }

        void StudioController_OnServersAdded(List<IServer> listAdded)
        {
            log.Info("Server added: " + ((listAdded.Count) > 0 ? listAdded[0].ServerName : String.Empty));

            InvokeInUI(() =>
            {
                foreach (var item in ItemFactory.BuildServer(listAdded))
                {
                    _serverList.Add(item);
                }

                if (_serverList.Count == 1)
                {
                    cbServer.SelectedIndex = 0;
                    log.Info("Face: first server is selected.");
                }
            });
        }

        void StudioController_OnServersRemoved(List<IServer> removedList)
        {
            log.Info("Face: server removed." + (removedList.Count > 0 ? removedList[0].ServerName : ""));

            InvokeInUI(() =>
            {

                foreach (var s in _serverList.ToList())
                {
                    if (removedList.Contains(s.Server))
                    {
                        _serverList.Remove(s);
                    }
                }

                if (SelectedServer == null)
                {
                    // move selection to first available server                
                    if (_serverList.Count > 0)
                    {
                        log.Info("Face: Selected Server was removed. Move focus to first server in list");
                        cbServer.SelectedIndex = 0;
                    }
                    else
                    {
                        log.Info("Face: Selected Server was removed. Server list is empty.Clear everything");
                        cbDatabase.ItemsSource = null;
                    }
                }
            });
        }

        void StudioController_OnServersChanged()
        {
            log.Info("Face: server list changed.");

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

        void _processor_RequestFailed(Request arg1, Exception arg2)
        {
            SetStatus("Error:" + arg2.Message);
            // notify user about an error
            log.Error("Request failed:" + arg1.Argument + " type:" + arg1.RequestType, arg2);
        }

        public void ReloadServers()
        {
            log.Info("Reloading Servers - initiated by user");
            var servers = StudioController.ListServers();
            _serverList.Clear();

            foreach (var item in ItemFactory.BuildServer(servers))
            {
                _serverList.Add(item);
            }

            log.Info("Reloading Servers - loaded:" + _serverList.Count + " servers.");
            cbServer.ItemsSource = _serverList;

            if (_serverList.Count == 1)
            {
                cbServer.SelectedIndex = 0;
            }
        }

        private void SetStatus(String text)
        {
            SetStatus(text, false);
        }

        private void SetStatus(String text, Boolean showImage)
        {
            InvokeInUI(() =>
            {
                txtStatusTest.Text = text;
            });
        }

        // Reload database list
        private void Async_ReloadDatabaseList(Object arg)
        {
            if (arg is IServer)
            {

                StudioController.RefreshServer(arg as IServer);

                InvokeInUI(() =>
                {
                    ReloadDatabaseList(true);
                });
            }
        }

        // Reload objects in database
        private void Async_ReloadObjectsFromDatabase(Object arg)
        {
            var cmd = (ServerDatabaseCommand)arg;

            SetStatus("Reloading " + cmd.DatabaseName + "...", true);

            StudioController.RefreshDatabase(cmd.Server, cmd.DatabaseName);

            InvokeInUI(() =>
            {
                DoSearch(true);
            });

            SetStatus("Completed reloading " + cmd.DatabaseName);
        }

        void ReloadDatabaseList(bool keepSameDatabase)
        {
            string databaseName = string.Empty;
            try
            {
                var sel = SelectedServer;

                if (keepSameDatabase)
                    databaseName = SelectedDatabase;

                if (sel != null)
                {
                    cbDatabase.ItemsSource = ItemFactory.BuildDatabase(StudioController.ListDatabase(sel));

                    _databaseChangedByUser = false;

                    // changed server - try to restore database user worked with last time
                    if (string.IsNullOrEmpty(databaseName))
                        databaseName = _userPref.GetByName(UserPref_ServerDatabase + sel.ServerName);
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
                    _userPref.StoreByName(UserPref_LastSelectedServer, sel.ServerName);
                    _userPref.Save();

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
            ReloadDatabaseList(false);

            // TODO: keep track of last selected database on this server - need to restore it back!
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
                _userPref.Save();
            }
        }

        private void txtSearch_TextChanged(Object sender, TextChangedEventArgs e)
        {
            log.Info(String.Format("Search text changed to '{0}'", txtSearch.Text));
            var analyzer = new PerformanceAnalyzer();

            DoSearch(false);

            log.Performance("Text changed event handled", analyzer.Result);
            analyzer.Stop();
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
                    _processor.AddRequest(Async_PerformSearch, sp, RequestType.Search, true);
                    _userPref.StoreByName(UserPref_LastSearchText, txtSearch.Text);
                    _userPref.Save();
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

            var par = (SearchAsyncParam)arg;

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

            var result = StudioController.Find(par.Srv, par.Database, par.Text, _cfg.LimitSearch);

            // new request was added - this one is outdated
            if (par.SequenceNumber < _requestSequenceNumber)
            {
                log.Info("Cancelled search request because new request was added. " + par.Text);
                return;
            }

            SetStatus("Found " + result.Count + " objects");

            InvokeInUI(() =>
            {
                log.Info("Updating UI items");
                var analyzer = new PerformanceAnalyzer();

                var items = ItemFactory.BuildFromEntries(result);

                itemsControl.ItemsSource = items;
                itemsControl.SelectedIndex = -1;
                itemsControl.ScrollIntoView(itemsControl.SelectedItem);

                if (items.Count == 0)
                {
                    gridEmptyResult.Visibility = System.Windows.Visibility.Visible;
                    itemsControl.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    gridEmptyResult.Visibility = System.Windows.Visibility.Collapsed;
                    itemsControl.Visibility = System.Windows.Visibility.Visible;
                }

                log.Performance("UI items updated", analyzer.Result);
                analyzer.Stop();
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
            Dispatcher.Invoke((Delegate)invoker);
        }

        // TODO: Remove (empty method)
        private void ItemsControlSelectionChanged1(Object sender, SelectionChangedEventArgs e)
        {
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
            IEnumerable<String> result = new List<String>();

            if (item != null)
            {
                result = item.Actions.Select(a => a.Name);
            }

            return result;
        }

        void InvokeActionByName(Item item, String actionName)
        {
            if ((item == null) || String.IsNullOrEmpty(actionName))
            {
                log.Error("Action name is null or an empty string");
                return;
            }

            var action = item.Actions.FirstOrDefault(a => a.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                log.Error("Action '" + actionName + "' has not been found");
                return;
            }

            if (action.Routine == null)
            {
                log.Error("Action '" + actionName + "' handler is not defined");
                return;
            }

            action.Routine(StudioController, SelectedServer);
        }

        // TODO: Refactor with respect of Item.Action generalization
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

        // TODO: Refactor with respect of Item.Action generalization
        void InvokeAdditionalActionOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.DesignTable(SelectedServer, item.Entity);
            }
        }

        // TODO: Refactor with respect of Item.Action generalization
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

        // TODO: Refactor with respect of Item.Action generalization
        void InvokeNavigationOnItem(Item item)
        {
            StudioController.NavigateObject(SelectedServer, item.Entity);
        }

        private void btnNavigationClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeNavigationOnItem(item);
        }

        private void btnActionClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeActionOnItem(item);
        }

        private void btnAdditonalActionClick(Object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeAdditionalActionOnItem(item);
        }

        private void DefaultAction_Click(Object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeDefaultOnItem(item);
        }

        // TODO: Remove (empty method)
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

                // jump to text search box from Result View
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
                // open popup control and move focus to the first item there
                OpenContextMenu();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                if (itemsControl.SelectedIndex != -1)
                {
                    InvokeActionOnItem(itemsControl.SelectedItem as Item);
                }

                e.Handled = true;

            }
            else if (e.Key == Key.Left)
            {
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

        private void myEnityText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragDropStartedFromText = false;
        }



        private void TextBlock_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // show be treated as Enter need to move focus and open Popup
                if (itemsControl.SelectedItem != null)
                {
                    var controlSelected = itemsControl.ItemContainerGenerator.ContainerFromItem(SelectedItem);
                }
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

        // TODO: Remove (empty method)
        private void PropertiesTextBlock_MouseUp(Object sender, MouseEventArgs e)
        {
        }

        // TODO: Remove (empty method)
        private void PropertiesTextBlock_MouseDown(Object sender, MouseEventArgs e)
        {
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
                    ListViewItem listViewItem = item.FindAncestor<ListViewItem>();

                    // Find the data behind the ListViewItem
                    Item contact = (Item)itemsControl.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    if ((contact != null) && (contact.Entity != null))
                    {
                        // Initialize the drag & drop operation
                        DragDrop.DoDragDrop(listViewItem, contact.Entity.FullName, DragDropEffects.Copy);
                    }
                }
            }
        }

        // TODO: Remove (empty method)
        private void PropertiesTextBlock_MouseMove(Object sender, MouseEventArgs e)
        {
        }

        private void cbDatabase_GotFocus(Object sender, RoutedEventArgs e)
        {
            cbDatabase.BorderBrush = _borderBrush;
        }

        // TODO: Remove (empty method)
        private void cbDatabase_LostFocus(Object sender, RoutedEventArgs e)
        {
        }

        // TODO: Remove (empty method)
        private void cbServer_GotFocus(Object sender, RoutedEventArgs e)
        {
        }

        // TODO: Remove (empty method)
        private void cbServer_LostFocus(Object sender, RoutedEventArgs e)
        {
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

        private void txtSearch_SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
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



        private void ReloadObjectsFromDatabase()
        {
            if (SelectedServer != null)
            {
                if (SelectedDatabase != null)
                {
                    _processor.AddRequest(Async_ReloadObjectsFromDatabase, new ServerDatabaseCommand() { Server = SelectedServer, DatabaseName = SelectedDatabase }, (RequestType)(Int32)ERquestType.RefreshSearch, true);
                }
            }
        }

        private void Refresh_Click(Object sender, RoutedEventArgs e)
        {
            ReloadObjectsFromDatabase();
        }


        Config.DogConfig _cfg = new Config.DogConfig();
        Config.ConfigPersistor _persistor = new Config.ConfigPersistor();

        private void Options_Click(Object sender, RoutedEventArgs e)
        {

            var cfgWindow = new DialogWindow();

            cfgWindow.ShowConfiguration(_cfg);

            this.BlurApply(10, new TimeSpan(0, 0, 0, 500), TimeSpan.Zero);

            var result = cfgWindow.ShowDialog();


            if (result != null && result == true)
            {
                _cfg = cfgWindow.DogConfig;
                if (_studio != null)
                    _studio.SetConfiguration(_cfg);

                if (_userPref != null)
                {
                    _persistor.Persist(_cfg, _userPref);
                    _userPref.Save();
                }
            }

            this.BlurDisable(new TimeSpan(0, 0, 0, 500), TimeSpan.Zero);
        }

        void userCtrol_OnNewConfig(Config.DogConfig obj)
        {
            if (obj == null)
                return;

            _cfg = obj;
            if (_studio != null)
            {
                _studio.SetConfiguration(_cfg);
                ResultsFontSize = _cfg.FontSize;
            }

            if (_userPref != null)
            {
                _persistor.Persist(_cfg, _userPref);
                _userPref.Save();
            }
        }

        private void RefreshDatabaseList_Click(Object sender, RoutedEventArgs e)
        {
            StoreSelectedDatabaseInUserProfile();
            ReloadObjectsFromDatabase();

            if (SelectedServer != null)
            {
                _processor.AddRequest(Async_ReloadDatabaseList, SelectedServer, (Int32)ERquestType.Server, true);
            }
        }

        // TODO: Remove (empty method)
        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
        }

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

        // TODO: Remove (empty method)
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

        // TODO: Remove (empty method)
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
                ctx.ItemsSource = BuilsAvailableActions(SelectedItem);
                ctx.IsOpen = true;
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

    }
}