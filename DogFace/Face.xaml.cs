using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DatabaseObjectSearcher;
using DatabaseObjectSearcherUI;
using HuntingDog.DogEngine;

namespace HuntingDog.DogFace
{
    /// <summary>
    /// Interaction logic for Face.xaml
    /// </summary>
    public partial class Face : UserControl
    {
        // these keys are used to save/load user preferences
        public const string UserPref_LastSearchText = "Last Search Text";
        public const string UserPref_ServerDatabase= "[database]:";
        public const string UserPref_LastSelectedServer = "Last Selected Server";

        BackgroundProcessor _processor = new BackgroundProcessor();

        static IStudioController _studio;
        public IStudioController StudioController
        {
            get
            {
                if (_studio == null)
                {
                    _studio = HuntingDog.DogEngine.StudioController.Current;
                }

                return _studio;
            }
            set
            {
                _studio = value;
            }
        }

        enum ERquestType:int
        {
            Server,
            Search ,
            Details
        }

        public Face()
        {
            InitializeComponent();
        }

        UserPreferencesStorage _userPref;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            var scroll = WpfUtil.FindChild<ScrollContentPresenter>(itemsControl);
            //scroll.SizeChanged += new SizeChangedEventHandler(scroll_SizeChanged);

            _processor.RequestFailed += new Action<BackgroundProcessor.Request, Exception>(_processor_RequestFailed);
            StudioController.Initialise();
            StudioController.OnServersChanged += new Action(StudioController_OnServersChanged);
            StudioController.ShowYourself += new Action(StudioController_ShowYourself);
            ReloadServers();

            _userPref = UserPreferencesStorage.Load();
            string lastSrvName = _userPref.GetByName(UserPref_LastSelectedServer);
            string lastSearch = _userPref.GetByName(UserPref_LastSearchText);

            if(string.IsNullOrEmpty(lastSearch))
                  txtSearch.Text = "HAHAHAH";
            else
                txtSearch.Text = lastSearch;

            //cbServer.SelectedValue = lastSrvName;
            //cbServer.SelectedItem = 

            // select first server
            if (cbServer.SelectedIndex==-1 && cbServer.Items.Count>1)
            {
                cbServer.SelectedIndex = 0;
            }    
          
        }

        void StudioController_ShowYourself()
        {
            txtSearch.Focus();
          
        }

     

        void StudioController_OnServersChanged()
        {
            ReloadServers();

            if (cbServer.SelectedIndex == -1 && cbServer.Items.Count > 1)
                cbServer.SelectedIndex = 0;
        }

        void _processor_RequestFailed(BackgroundProcessor.Request arg1, Exception arg2)
        {
            // notify user about an error

        }

        public const string ConnectNewServerString = "Connect New Server...";

        public void ReloadServers()
        {   
            var servers = StudioController.ListServers();
            var srv = ItemFactory.BuildServer(servers);
            srv.Add(new Item() { Name = ConnectNewServerString });
            cbServer.ItemsSource = srv;

            _processor.AddRequest(Async_ReloadServers, servers, (int)ERquestType.Server,true);
        }

        private void SetStatus(string text)
        {
            SetStatus(text, false);
        }
        private void SetStatus(string text,bool showImage)
        {
            InvokeInUI(delegate {
                imgWorking.Visibility = showImage ? Visibility.Visible : Visibility.Hidden;
                txtStatusTest.Text = text; });
        }

        // Reload all servers ad read all business object for fast search and access
        private void Async_ReloadServers(object arg)
        {
            foreach (var server in (List<string>)arg)
            {
                SetStatus("Refreshing " + server + "...",true);

                StudioController.RefreshServer(server);

                SetStatus("Completed " + server);
            }
        }


        
        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsConnectNewServerSellected)
            {
                StudioController.ConnectNewServer();
                
                return;
            }

            var sel = cbServer.SelectedItem as Item;
            if (sel != null)
            {

                cbDatabase.ItemsSource = ItemFactory.BuildDatabase(StudioController.ListDatabase(sel.Name));

                _databaseChangedByUser = false;
                // changed server - try to restore database user worked with last time
                var databaseName = _userPref.GetByName(UserPref_ServerDatabase + sel.Name);
                //cbDatabase.SelectedValue= databaseName;

                _databaseChangedByUser = true;

                // if we failed to select database (for example it deos not exsit any more - select first one...)
                if(cbDatabase.SelectedIndex == -1 && cbDatabase.Items.Count>0)
                {
                    cbDatabase.SelectedIndex = 0;
                }

                _userPref.StoreByName(UserPref_LastSelectedServer, sel.Name);

                cbDatabase.Focus();
                //cbDatabase.IsDropDownOpen = true;
            }

            // keep track of last selected database on this server - need to restore it back!
            //DoSearch();
        }

        bool _databaseChangedByUser = true;

        bool IsConnectNewServerSellected
        {
            get
            {
                if (cbServer.SelectedItem == null)
                    return false;

                return (cbServer.SelectedItem as Item).Name == ConnectNewServerString;
            }
        }

        string SelectedServer
        {
            get{
                if(cbServer.SelectedItem==null)
                    return null;

                return (cbServer.SelectedItem as Item).Name;
            }
        }

        ListViewItem SelectedListViewItem
        {
            get
            {
                if (itemsControl.SelectedItem == null)
                    return null;

                return itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.SelectedItem) as ListViewItem;
            }
        }

        Item SelectedItem
        {
            get
            {
                if(itemsControl.SelectedItem==null)
                    return null;

                return itemsControl.SelectedItem as Item; 
            }
        }

        string SelectedDatabase
        {
            get
            {
                if (cbDatabase.SelectedItem == null)
                    return null;

                return (cbDatabase.SelectedItem as Item).Name;
            }
        }

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_databaseChangedByUser)
            {
                if (SelectedServer != null && SelectedDatabase != null)
                {
                    _userPref.StoreByName(UserPref_ServerDatabase + SelectedServer,
                        SelectedDatabase);

                    txtSearch.Focus();  

                    DoSearch();
                }
            }


        }

      
        bool _isDragDropStartedFromText = false;

        bool _isDragDropStartedPropeties = false;

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoSearch();
        }

        void DoSearch()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text) && SelectedServer!=null && SelectedDatabase!=null)
            {
                var sp = new SearchAsyncParam();
                sp.SequenceNumber = ++_requestSequenceNumber;
                sp.Srv = SelectedServer;
                sp.Text = txtSearch.Text;
                sp.Database = SelectedDatabase;
                _processor.AddRequest(Async_PerformSearch, sp, (int)ReqType.Search, true);

                _userPref.StoreByName(UserPref_LastSearchText, txtSearch.Text);
            
            }
            else
            {
                itemsControl.ItemsSource = null;
            }
        }

        volatile int _requestSequenceNumber = 0;

        string _lastSrv, _lastDb, _lastText;

        private bool SameAsPreviousSearch(SearchAsyncParam arg)
        {
            if (arg.Srv == _lastSrv && arg.Database == _lastDb && _lastText!=null && arg.Text.TrimEnd(' ') == _lastText.TrimEnd(' '))
            {
                return true;
            }

            _lastSrv = arg.Srv;
            _lastDb = arg.Database;
            _lastText = arg.Text;
            return false;
        }


        private void Async_PerformSearch(object arg)
        {
            if (arg == null)
                return;

            var par = (SearchAsyncParam)arg;

            // new request was added - this one is outdated
            if (par.SequenceNumber < _requestSequenceNumber)
                return;

            if (SameAsPreviousSearch(par))
            {
                return;
            }

            SetStatus("Searching '" + par.Text + "' in " + par.Database,true);

            var result = StudioController.Find(par.Srv, par.Database, par.Text);

            // new request was added - this one is outdated
            if (par.SequenceNumber < _requestSequenceNumber)
                return;

            SetStatus("Found " + result.Count + " objects ");

            InvokeInUI(delegate {

                var items = ItemFactory.BuildFromEntries(result);
                itemsControl.ItemsSource = items;
            
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

            SetStatus("Finish " + result.Count + " objects ");

   
         }

        // small hint - to use anonomys delegates in InvokeUI method
        public delegate void AnyInvoker();
        private void InvokeInUI(AnyInvoker invoker)
        {
            Dispatcher.Invoke((Delegate)invoker);
        }


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

        }



        private void ItemsControlSelectionChanged1(object sender, SelectionChangedEventArgs e)
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
                // clear propertties
                listViewProperties.ItemsSource = null;
            }
            else
            {
                var item = (itemsControl.SelectedItem as Item);
                if(item!=null)
                {
                    _processor.AddRequest(Async_ShowProperties, item.Entity, (int)ReqType.Details, true); 
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

            Stop();
        }


        internal IEnumerable<string> BuilsAvailableActions(Item item)
        {
            var res = new List<string>();

            if (item.Entity.IsProcedure)
                return new List<string> { "Modify", "Execute", "Locate",  };
            if (item.Entity.IsFunction)
                return new List<string> { "Modify", "Execute", "Locate",  };
            else if (item.Entity.IsTable)
                return new List<string> { "Select Data", "Edit Data", "Design Table", "Locate", };
            else if (item.Entity.IsView)
                return new List<string> { "Select Data","Modify View", "Locate",  };

            return res;
        }

        void InvokeActionByName(Item item, string actionName)
        {
            if(item==null || string.IsNullOrEmpty(actionName))
                return;

            if(actionName=="Locate")
            {
                StudioController.NavigateObject(SelectedServer,item.Entity);
                return;
            }

            if (actionName == "Show Dependencies")
            {
                // show dependecies in a new window
            }


            if (item.Entity.IsTable)
            {
                switch (actionName)
                {
                    case "Select Data": StudioController.SelectFromTable(SelectedServer, item.Entity); break;
                    case "Edit Data": StudioController.EditTableData(SelectedServer, item.Entity); break;
                    case "Design Table": StudioController.DesignTable(SelectedServer, item.Entity); break;
                }
            }
            else if(item.Entity.IsView)
            {
                switch (actionName)
                {
                    case "Select Data": StudioController.SelectFromView(SelectedServer, item.Entity); break;  
                    case "Modify View": StudioController.ModifyView(SelectedServer, item.Entity); break;     
                }
            }
            else if (item.Entity.IsProcedure)
            {
                switch (actionName)
                {
                    case "Modify": StudioController.ModifyProcedure(SelectedServer, item.Entity); break;
                    case "Execute": StudioController.ExecuteProcedure(SelectedServer, item.Entity); break;
                  
                }
            }
            else if (item.Entity.IsFunction)
            {
                switch (actionName)
                {
                    case "Modify": StudioController.ModifyFunction(SelectedServer, item.Entity); break;
                    case "Execute": StudioController.ExecuteFunction(SelectedServer, item.Entity); break;
                  
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


        private void btnNavigationClick(object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeNavigationOnItem(item);
  
  
        
        }


        private void btnActionClick(object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeActionOnItem(item);
  
        }

        private void btnAdditonalActionClick(object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeAdditionalActionOnItem(item);
  
        }


        private void DefaultAction_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item)((FrameworkElement)sender).Tag;
            InvokeDefaultOnItem(item);
        }
 

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
 
        }


        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Down)
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




        private bool MoveFocusItemsControl(bool isLast)
        {
            if (itemsControl.Items.Count > 0)
            {
                var index = isLast ? itemsControl.Items.Count - 1 : 0;
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

        

        private void itemsControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsItemFocused(itemsControl))
                return;

            if (e.Key == Key.Tab)
            {
                itemsControl.SelectedIndex = -1;
                            
                // move focus to result text
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
              
                e.Handled = true;
            }
            else if (e.Key == Key.Up && itemsControl.SelectedIndex == 0)
            {
              
                    itemsControl.SelectedIndex = -1;

                    // jump to text search box from Result View - 
                    itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));

                    e.Handled = true;
            }
            else if (e.Key == Key.Down && itemsControl.SelectedIndex == itemsControl.Items.Count-1)
            {
            
                // last item - do nothing
                    e.Handled = true;

               
            }
            else if ((e.Key == Key.Enter || e.Key == Key.Space) && itemsControl.SelectedIndex != -1)
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


        private void cbDatabase_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Space) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.IsDropDownOpen = true;
                e.Handled = true;
            }

            if (( e.Key == Key.Down || e.Key == Key.Right) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if (( e.Key == Key.Up || e.Key == Key.Left) && cbDatabase.IsDropDownOpen == false)
            {
                cbDatabase.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }

            if (e.Key == Key.Space && cbDatabase.IsDropDownOpen)
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

        private void cbServer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Space) && !cbServer.IsDropDownOpen)
            {
                cbServer.IsDropDownOpen = true;
                e.Handled = true;
            }

            if ((e.Key == Key.Down ) && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Down));
                e.Handled = true;
            }

            if ( e.Key == Key.Right && !cbServer.IsDropDownOpen)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if ((e.Key == Key.Up || e.Key == Key.Left) && !cbServer.IsDropDownOpen )
            {         
                e.Handled = true;
            }

            if (e.Key == Key.Space && cbServer.IsDropDownOpen)
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

        public long LastTicks = 0;

        private void TextBlock_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragDropStartedFromText = false;
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
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
                        InvokeDefaultOnItem(itemsControl.SelectedItem as Item);

                }
                LastTicks = DateTime.Now.Ticks;
            }
        }

        private void PropertiesTextBlock_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragDropStartedPropeties = false;
        }
        private void PropertiesTextBlock_MouseDown(object sender, MouseEventArgs e)
        {
            _isDragDropStartedPropeties = true;           
        }


        private void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isDragDropStartedFromText == true)
            {
                _isDragDropStartedFromText = false;
          
                // Get the dragged ListViewItem
                FrameworkElement item = sender as FrameworkElement;
                if (item != null)
                {
                    ListViewItem listViewItem = WpfUtil.FindAncestor<ListViewItem>(item);

                    // Find the data behind the ListViewItem
                    Item contact = (Item)itemsControl.ItemContainerGenerator.ItemFromContainer(listViewItem);
                    if (contact != null && contact.Entity != null)
                    {
                        // Initialize the drag & drop operation

                        DragDrop.DoDragDrop(listViewItem, contact.Entity.FullName, DragDropEffects.Copy);
                    }
                }
               
            }
        }

        private void PropertiesTextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isDragDropStartedPropeties == true)
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

            }
        }

     


        Brush _borderBrush = new SolidColorBrush(Color.FromRgb(0x64,0x95,0xed));
        Brush _blurBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x64, 0x95, 0xed));

       
        private void cbDatabase_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = _borderBrush;
            cbDatabase.BorderBrush = _borderBrush;
        }

        private void cbDatabase_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = Brushes.Transparent;
            //cbDatabase.BorderBrush = _blurBrush;
        }

        private void cbServer_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = _borderBrush;
            //cbServer.BorderBrush = _borderBrush;
        }

        private void cbServer_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = Brushes.Transparent;
            //cbServer.BorderBrush = _blurBrush;
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _borderBrush;
            imgSearch.Opacity = 1;
            txtSearch.SelectAll();
           
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            imgSearch.Opacity = 0.5;
            borderText.BorderBrush = _blurBrush;
        }

        private void borderText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtSearch.Focus();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedServer!=null)
                _processor.AddRequest(Async_ReloadServers, new List<string>{ SelectedServer}, (int)ERquestType.Server, true);
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
                return null;

            var cont = itemsControl.ItemContainerGenerator.ContainerFromItem(si);
            if (cont != null)
            {
                
                return (cont as ListViewItem).ContextMenu;
            }

            return null;
        }

        private void ListViewContextMenuClosing(object sender, ContextMenuEventArgs e)
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

        private void ListViewContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var li = sender as ListViewItem;
            if(li!=null)
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
           bool focused = false;
            foreach (var item in ctx.Items)
            {
                var menuItem = ctx.ItemContainerGenerator.ContainerFromItem(item) as MenuItem;
                if(menuItem!=null)
                {
                    menuItem.Click += menuItem_Click;
                    if (!focused)
                     menuItem.Focus();

                    focused = true;
                }
            }

           
            
        }

        void menuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if(menuItem!=null)
            {
                var cmd = menuItem.Header.ToString();
                InvokeActionByName(SelectedItem, cmd);

            }
        }

        void PrepareContextMenu(ContextMenu ctx,Item item)
        {
            
        }

        void OpenContextMenu()
        {

            var ctx = GetCtxMenuFromItem(SelectedItem);
            if (ctx != null)
            {
                ctx.Placement = PlacementMode.Left;
                ctx.PlacementTarget = SelectedListViewItem;
                ctx.HorizontalOffset = itemsControl.ActualWidth/2;
                ctx.VerticalOffset = SelectedListViewItem.ActualHeight/2;

                //var listViewItem = SelectedListViewItem;
                //var txt = WpfUtil.FindChild<TextBlock>(SelectedListViewItem);

                //ctx.PlacementTarget = txt;
                ctx.ItemsSource = BuilsAvailableActions(SelectedItem);
               
                
                ctx.IsOpen = true;
               // var lv = (popup.Child as Border).Child as ListView;
              //  lv.ItemsSource = BuilsAvailableActions(SelectedItem);

            }
        }

        private bool IsItemFocused(ListView lv)
        {
            var it = GetSelectedItem(lv);
            if (it != null)
                return it.IsFocused;

            return false;
        }


        private Control GetSelectedItem(ListView lv)
        {
            return lv.ItemContainerGenerator.ContainerFromIndex(lv.SelectedIndex) as Control;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            UnsubscribeToAction(sender as ContextMenu);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
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

    class SearchAsyncParam
    {
        public int SequenceNumber { get; set; }
        public string Srv { get; set; }
        public string Text { get; set; }
        public string Database{get;set;}
    }

    public class WidthConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double diff = double.Parse(parameter.ToString());
           // var desiredWidth =  ((double)value - 8);
            double desiredWidth = ((double)value - diff);
            //desiredWidth -= 80;

            if (desiredWidth < 100)
                desiredWidth = 100;

            return desiredWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}