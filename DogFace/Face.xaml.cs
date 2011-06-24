using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
                    _studio = new HuntingDog.DogEngine.StudioController();
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
           

            _processor.RequestFailed += new Action<BackgroundProcessor.Request, Exception>(_processor_RequestFailed);
            StudioController.Initialise();
            StudioController.OnServersChanged += new Action(StudioController_OnServersChanged);
            ReloadServers();

            _userPref = UserPreferencesStorage.Load();
            string lastSrvName = _userPref.GetByName(UserPref_LastSelectedServer);
            string lastSearch = _userPref.GetByName(UserPref_LastSearchText);

            if(string.IsNullOrEmpty(lastSearch))
                  txtSearch.Text = "HAHAHAH";
            else
                txtSearch.Text = lastSearch;

            cbServer.SelectedValue = lastSrvName;

            // select first server
            if (cbServer.SelectedIndex==-1 && cbServer.Items.Count>0)
            {
                cbServer.SelectedIndex = 0;
            }

          
        }

        void StudioController_OnServersChanged()
        {
            ReloadServers();
        }

        void _processor_RequestFailed(BackgroundProcessor.Request arg1, Exception arg2)
        {
            // notify user about an error

        }

        public void ReloadServers()
        {   
            var servers = StudioController.ListServers();
            cbServer.ItemsSource = servers;

            _processor.AddRequest(Async_ReloadServers, servers, (int)ERquestType.Server,true);
        }

        private void SetStatus(string text)
        {
            InvokeInUI(delegate { txtStatusTest.Text = text; });
        }

        // Reload all servers ad read all business object for fast search and access
        private void Async_ReloadServers(object arg)
        {
            foreach (var server in (List<string>)arg)
            {
                SetStatus("Refreshing " + server + "...");
                StudioController.RefreshServer(server);

                SetStatus("Completed " + server);
            }
        }


        
        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = cbServer.SelectedItem;
            if (sel != null)
            {
                cbDatabase.ItemsSource = StudioController.ListDatabase(sel.ToString());

                _databaseChangedByUser = false;
                // changed server - try to restore database user worked with last time
                var databaseName = _userPref.GetByName(UserPref_ServerDatabase + sel.ToString());
                cbDatabase.SelectedValue= databaseName;

                _databaseChangedByUser = true;

                // if we failed to select database (for example it deos not exsit any more - select first one...)
                if(cbDatabase.SelectedIndex == -1 && cbDatabase.Items.Count>0)
                {
                    cbDatabase.SelectedIndex = 0;
                }

                _userPref.StoreByName(UserPref_LastSelectedServer, sel.ToString());
            }

            // keep track of last selected database on this server - need to restore it back!
            DoSearch();
        }

        bool _databaseChangedByUser = true;

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_databaseChangedByUser)
            {
                if (cbServer.SelectedValue != null && cbDatabase.SelectedValue != null)
                    _userPref.StoreByName(UserPref_ServerDatabase + cbServer.SelectedValue.ToString(),
                        cbDatabase.SelectedValue.ToString());

                DoSearch();
            }


        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoSearch();
        }

        void DoSearch()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text) && cbServer.SelectedItem!=null && cbDatabase.SelectedValue!=null)
            {
                var sp = new SearchAsyncParam();
                sp.Srv = cbServer.SelectedItem.ToString();
                sp.Text = txtSearch.Text;
                sp.Database = cbDatabase.SelectedValue.ToString();
                _processor.AddRequest(Async_PerformSearch, sp, (int)ReqType.Search, true);

                _userPref.StoreByName(UserPref_LastSearchText, txtSearch.Text);
            
            }
            else
            {
                itemsControl.ItemsSource = null;
            }
        }


        private void Async_PerformSearch(object arg)
        {
            if (arg == null)
                return;

            var par = (SearchAsyncParam)arg;
            SetStatus("Searching '" + par.Text + "' in " + par.Database);

            var result = StudioController.Find(par.Srv, par.Database, par.Text);


            InvokeInUI(delegate { 
                
                itemsControl.ItemsSource = ItemFactory.BuildFromEntries(result);
            
                itemsControl.SelectedIndex = 0;
                itemsControl.ScrollIntoView(itemsControl.SelectedItem);
            });

            SetStatus("Found " + result.Count + " objects ");

   
         }

        // small hint - to use anonomys delegates in InvokeUI method
        public delegate void AnyInvoker();
        private void InvokeInUI(AnyInvoker invoker)
        {
            Dispatcher.Invoke((Delegate)invoker);
        }

   

        private void itemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void itemsControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

            Stop();
        }

        void InvokeDefaultOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.SelectFromTable(item.Entity.FullName);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ModifyProcedure(item.Entity.FullName);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ModifyFunction(item.Entity.FullName);
            }
            else if (item.Entity.IsView)
            {
                StudioController.SelectFromView(item.Entity.FullName);
            }
        }

        void InvokeActionOnItem(Item item)
        {
            if (item.Entity.IsTable)
            {
                StudioController.DesignTable(item.Entity.FullName);
            }
            else if (item.Entity.IsProcedure)
            {
                StudioController.ExecuteProcedure(item.Entity.FullName);
            }
            else if (item.Entity.IsFunction)
            {
                StudioController.ExecuteFunction(item.Entity.FullName);
            }


        }

        void InvokeNavigationOnItem(Item item)
        {
            StudioController.NavigateObject(item.Entity.FullName);
        }


        private void btnNavigationClick(object sender, RoutedEventArgs e)
        {
            var item = (Item)((Control)sender).Tag;
            InvokeNavigationOnItem(item);
  
        }

        private void btnActionClick(object sender, RoutedEventArgs e)
        {
             var item = (Item)((Control)sender).Tag;
            InvokeActionOnItem(item);
  
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // double click?
            //if (e.ClickCount >= 2)
            {
                var item = (Item)((FrameworkElement)sender).Tag;
                InvokeDefaultOnItem(item);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
 
        }


        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Down)
            {
                if(itemsControl.Items.Count > 0)
                // move focus to result list view
                txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                itemsControl.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                txtSearch.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                e.Handled = true;
            }
        }


        private void itemsControl_KeyDown(object sender, KeyEventArgs e)
        { 
        }

        private void itemsControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // move focus to result text
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                itemsControl.SelectedIndex = -1;
                e.Handled = true;
            }
            else if (e.Key == Key.Up && itemsControl.SelectedIndex == 0)
            {
                // jump to text search box from Result View - 
                itemsControl.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Previous));
                itemsControl.SelectedIndex = -1;
                e.Handled = true;
            }
            else if (e.Key == Key.Down && itemsControl.SelectedIndex == itemsControl.Items.Count-1)
            {
                // last item - do nothing
                e.Handled = true;
            }
        }

        private void cbDatabase_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cbDatabase.IsDropDownOpen == false)
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
        }

        private void cbServer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cbServer.IsDropDownOpen == false)
            {
                cbServer.IsDropDownOpen = true;
                e.Handled = true;
            }

            if ((e.Key == Key.Down || e.Key == Key.Right) && cbServer.IsDropDownOpen == false)
            {
                cbServer.MoveFocus(new TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
                e.Handled = true;
            }

            if ((e.Key == Key.Up || e.Key == Key.Left) && cbServer.IsDropDownOpen == false)
            {         
                e.Handled = true;
            }
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void itemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var gv = itemsControl.View as GridView;

            var totalWidth = itemsControl.ActualWidth;
            
            totalWidth -= gv.Columns[0].ActualWidth;
            totalWidth -= gv.Columns[2].ActualWidth;

            // Magic number - we need to take into acctound padding/margins and all other stuff and caclulate Width of the central column
            // we Width is too high scrool bar will appear. 
            totalWidth -= 6;

            if(totalWidth < 100)
                totalWidth  = 100;
            gv.Columns[1].Width = totalWidth;
        }

        Brush _borderBrush = new SolidColorBrush(Color.FromRgb(0x6A,0xa4,0xb6));
        Brush _blurBrush = new SolidColorBrush(Color.FromArgb(0x50,0x6A, 0xa4, 0xb6));

       
        private void cbDatabase_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = _borderBrush;
            cbDatabase.BorderBrush = _borderBrush;
        }

        private void cbDatabase_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderDatabase.BorderBrush = Brushes.Transparent;
            cbDatabase.BorderBrush = _blurBrush;
        }

        private void cbServer_GotFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = _borderBrush;
            cbServer.BorderBrush = _borderBrush;
        }

        private void cbServer_LostFocus(object sender, RoutedEventArgs e)
        {
            //borderServer.BorderBrush = Brushes.Transparent;
            cbServer.BorderBrush = _blurBrush;
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _borderBrush;
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            borderText.BorderBrush = _blurBrush;
        }

        private void itemsControl_GotFocus(object sender, RoutedEventArgs e)
        {
            borderItems.BorderBrush = _borderBrush;
        }

        private void itemsControl_LostFocus(object sender, RoutedEventArgs e)
        {
            borderItems.BorderBrush = Brushes.Transparent;
        }
    }

    class SearchAsyncParam
    {
      
        public string Srv { get; set; }
        public string Text { get; set; }
        public string Database{get;set;}
    }
}