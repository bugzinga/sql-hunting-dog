using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using DatabaseObjectSearcher;

namespace DatabaseObjectSearcherUI
{


    public partial class ucDependencyItem : UserControl,IListViewItem
    {
        Color _notSelectedColor;
        Color _SelectedColor;

        public ucDependencyItem()
        {
            InitializeComponent();
            _notSelectedColor = BackColor;
            _SelectedColor = Color.FromArgb(67, 87, 123);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
          
            this.textEdit1.Click += new EventHandler(textEdit1_Click);
            this.textEdit1.DoubleClick += new EventHandler(textEdit1_DoubleClick);
            this.MouseClick += new MouseEventHandler(ucSearchItem_MouseClick);
            this.MouseDoubleClick += new MouseEventHandler(ucSearchItem_MouseDoubleClick); 
        }

        void textEdit1_DoubleClick(object sender, EventArgs e)
        {
            if (OnDoubleClicked != null)
                OnDoubleClicked(this, null);
        }

        void ucSearchItem_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (OnDoubleClicked != null)
                OnDoubleClicked(this, null);
        }

        void textEdit1_Click(object sender, EventArgs e)
        {
            if (OnClicked != null)
                OnClicked(this, null);
        }

        void ucSearchItem_MouseClick(object sender, MouseEventArgs e)
        {
            if (OnClicked != null)
                OnClicked(this, null);
        }

        #region IListViewItem Members

        public new Control Control
        {
            get { return this; }
        }

        bool _isSelected = false;
        public bool Selection
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                UpdateViewState();
                Refresh();
            }
        }

        private void UpdateViewState()
        {
            if (_isSelected)
            {
                
            }
            //BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //BackColor = _SelectedColor;
            else
            {
             
            }
                //BorderStyle = System.Windows.Forms.BorderStyle.None;
                //BackColor = _notSelectedColor;
        }

        #endregion

        private void btnLocate_Click(object sender, EventArgs e)
        {
            if (OnAction != null)
                OnAction(this, new ActionArgs() { Action = EAction.Locate });
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (OnAction != null)
                OnAction(this, new ActionArgs() { Action = EAction.Execute });
        }



        private void btnDirection_Click(object sender, EventArgs e)
        {

            if (OnAction != null)
                OnAction(this, new ActionArgs() { Action = EAction.MoveTo });
        }

        DatabaseSearchResult obj;
        string bo;
        public object BoundObject
        {
            get
            {
                return obj;
            }
            set
            {
                var dep = (DatabaseDependencyResult)value;
                obj = dep.Obj;

                if (dep.Direction == Direction.DependentOn)
                    btnDirection.Image = HuntingDog.Properties.Resources.arrow_left_blue;
                else
                    btnDirection.Image = HuntingDog.Properties.Resources.arrow_right_blue;               

                if (obj.ObjectType == ObjType.StoredProc) 
                {
                    //btnExecute.Visible = true;
                    btnLocate.Image = HuntingDog.Properties.Resources.scroll;
                }
                else if (obj.ObjectType == ObjType.Func) 
                {
                    //btnExecute.Visible = true;
                    btnLocate.Image = HuntingDog.Properties.Resources.text_formula;
                }
                else if (obj.ObjectType == ObjType.Table)
                {
                    //btnExecute.Visible = false;
                    btnLocate.Image = HuntingDog.Properties.Resources.table_sql;
                }
                else if (obj.ObjectType == ObjType.View)
                {
                    //btnExecute.Visible = false;
                    btnLocate.Image = HuntingDog.Properties.Resources.text_align_center;
                }

                textEdit1.Text = obj.Name;
            }
        }



        public event ListHandler OnClicked;
        public event ListHandler OnDoubleClicked;
        public event ListHandler<ActionArgs> OnAction;



        public event ListHandler OnKeyPressed;
    }
}
