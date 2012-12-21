using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using HuntingDog;

namespace DatabaseObjectSearcherUI
{
    public partial class ucListView : UserControl
    {
        public event Action OnEscapePressed;
        public event Action OnLeaveSelection;

        public event ListHandler OnFocusedItemChanged;
        public event ListHandler OnClicked;
        public event ListHandler OnDoubleClicked;
        public event ListHandler<ActionArgs> OnAction;


        CircularProgressControl _progress;
        public ucListView()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            _progress = circularProgressControl1;
            circularProgressControl1.Visible = false;
        }

        public void SetFocus()
        {
            if (listItems.Count > 0)
            {
                SelectedItem = listItems[listItems.Count-1];
            }
        }

        private void SetLocation(IListViewItem listEntry)
        {
            var y = listEntry.Control.Height * listItems.Count;

            // set location and size
            listEntry.Control.Location = new Point(0, 0);
            listEntry.Control.Dock = DockStyle.Top;
        }

        private void UpdateSize(Control ctrl)
        {
           
            ctrl.Width = this.Width - 20;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        public IListViewItemFactory Factory { get; set; }

        List<IListViewItem> listItems = new List<IListViewItem>();

        protected override void OnMouseClick(MouseEventArgs e)
        {
  
        }

        public void StartProgress()
        {
            this.Controls.Add(_progress);
            _progress.Visible = true;
            _progress.Start();
        }

        public void StopProgress()
        {
            if(this.Controls.Contains(_progress))
                this.Controls.Remove(_progress);
            _progress.Visible = false;
            _progress.Stop();
        }

        IListViewItem _selectedItem;
        IListViewItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    this.SuspendLayout();

                    if (_selectedItem != null)
                        _selectedItem.Selection = false;

                    _selectedItem = value;

                    if (_selectedItem != null)
                        _selectedItem.Selection = true;

                    //Refresh();
                    if(OnFocusedItemChanged!=null)
                         OnFocusedItemChanged(_selectedItem, null);

                    this.ResumeLayout(true);                 
                }

                if (_selectedItem != null)
                    txtKeyCatcher.Focus();

                if (_selectedItem != null)
                {                   
                    ScrollControlIntoView(_selectedItem.Control);
                    //AutoScrollPosition = new Point(0,200);
                }

               
            }

        }
        public void SetDataSource(IEnumerable list)
        {
            StopProgress();

            if (Factory == null)
                throw new Exception("List View needs Factory to produce list items");

            this.SuspendLayout();
           
            listItems.Clear();

            this.Controls.Clear();

            // special control - it catches all keyboard input
            this.Controls.Add(txtKeyCatcher);

            if (list != null)
            {
                foreach (var item in list)
                {
                    var listEntry = Factory.CreateNew(item);

                    //UpdateSize(listEntry.Control);
                    //SetLocation(listEntry);
                    listEntry.Control.Dock = DockStyle.Top;

                    listEntry.OnClicked += new ListHandler(listEntry_OnClicked);
                    listEntry.OnAction += new ListHandler<ActionArgs>(listEntry_OnAction);
                    listEntry.OnDoubleClicked += new ListHandler(listEntry_OnDoubleClicked);
                    this.Controls.Add(listEntry.Control);

                    //listEntry.Control.SizeChanged += new EventHandler(Control_SizeChanged);

                    listItems.Add(listEntry);
                }

            }
           
            this.ResumeLayout(true);  
        

           
        }


        void listEntry_OnDoubleClicked(IListViewItem sender, EventArgs e)
        {
            if (OnDoubleClicked != null)
                OnDoubleClicked(sender, e);

            txtKeyCatcher.Focus();
        }

        void listEntry_OnAction(IListViewItem sender, ActionArgs e)
        {
            if (OnAction != null)
                OnAction(sender, e);

            txtKeyCatcher.Focus();
        }

        void listEntry_OnClicked(IListViewItem sender, EventArgs e)
        {
            SelectedItem = sender;
           
        }


        public void MoveSelectionUp()
        {
            for(int i =0; i< listItems.Count; i++)
            {
               if(listItems[i].Selection)
               {
                   // move selection up
                   if (i + 1 < listItems.Count)
                   {
                       SelectedItem = listItems[i + 1];
                       return;
                   }
                   else
                   {
                       SelectedItem = null;

                       if (OnLeaveSelection != null)
                           OnLeaveSelection();
                   }
               }
            }
        }

        public void MoveSelectionDown()
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                if (listItems[i].Selection)
                {
                    // move selection down
                    if (i  >= 1)
                    {
                        SelectedItem = listItems[i - 1];
                        return;
                    }
                }
            }
        }

        private void txtKeyCatcher_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                MoveSelectionUp();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                MoveSelectionDown();
                e.Handled = true;
              
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (OnDoubleClicked != null && SelectedItem!=null)
                    OnDoubleClicked(SelectedItem, e);
                txtKeyCatcher.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                if (OnEscapePressed != null)
                    OnEscapePressed();
            }
        }  

    }
}
