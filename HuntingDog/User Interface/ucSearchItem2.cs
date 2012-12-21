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
using Plasmoid.Extensions;
using System.Drawing.Drawing2D;
using Transitions;
using System.Runtime.InteropServices;

namespace DatabaseObjectSearcherUI
{


    public partial class ucSearchItem2 : UserControl,IListViewItem
    {
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern long HideCaret(IntPtr hwnd);

        Color _notSelectedColor;
        Color _SelectedColor;


        private void ucSearchItem2_Load(object sender, EventArgs e)
        {
       
        }

        Size _intial = new Size();
        public ucSearchItem2()
        {
          
            InitializeComponent();
      

            _intial = Size;
            _notSelectedColor = BackColor;
            _SelectedColor = Color.FromArgb(67, 87, 123);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.lblMain.Click += new EventHandler(textEdit1_Click);
            this.lblMain.DoubleClick += new EventHandler(textEdit1_DoubleClick);
            this.MouseClick += new MouseEventHandler(ucSearchItem_MouseClick);
            this.MouseDoubleClick += new MouseEventHandler(ucSearchItem_MouseDoubleClick);

       
            UpdateViewState();
          

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
                if(_isSelected == true)
                    this.Focus();

                UpdateViewState();
                Refresh();
            }
        }


        protected override void OnSizeChanged(EventArgs e)
        {

            if (_isSelected)
                Invalidate();

            base.OnSizeChanged(e);
        }

        LinearGradientBrush linearBrush = new LinearGradientBrush(new Rectangle(0, 0, 10, 22),
                            Color.FromArgb(194, 194, 194), Color.FromArgb(107, 107, 107), LinearGradientMode.Vertical);

        protected override void OnPaint(PaintEventArgs e)
        {
            
            if (_isSelected)
            {



                    //e.Graphics.FillRoundedRectangle(linearBrush, 3, 0, this.Width - 6, this.Height, 8, RectangleEdgeFilter.BottomRight | RectangleEdgeFilter.TopLeft);

                    //e.Graphics.DrawRoundedRectangle(Pens.Black, 3, 0, this.Width - 6, this.Height-1, 8, RectangleEdgeFilter.BottomRight | RectangleEdgeFilter.TopLeft);

                    //var eg = new ExtendedGraphics(e.Graphics);

                //eg.FillRoundRectangle(Brushes.DarkGray, 3, 0,this.Width-6, this.Height, 5);             
            }
            //HideCaret(richTextBox1.Handle);
        }
        

        bool _wasSelected = false;
        private void UpdateViewState()
        {
            if (_isSelected)
            {
                lblMain.BackColor = Color.Gainsboro;
                if (!_wasSelected)
                {
                    /*Transition t1 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t1.add(this.lblMain, "Left", this.lblMain.Left - 5);

                    Transition t2 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t2.add(this.lblMain, "Left", this.lblMain.Left + 2);

                    Transition t3 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t3.add(this.lblMain, "Left", this.lblMain.Left);
                    //t.add(this.textEdit1, "Left", this.textEdit1.Left);

                    Transition.runChain(t1, t2,t3);


                    Transition t4 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t4.add(this.btnExecute, "Left", this.btnExecute.Left-3);

                    Transition t5 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t5.add(this.btnExecute, "Left", this.btnExecute.Left +1);

                    Transition t6 = new Transition(new TransitionType_EaseInEaseOut(100));
                    t6.add(this.btnExecute, "Left", this.btnExecute.Left);

                    Transition.runChain(t4, t5,t6);*/


                    
                }
                

                //MaximumSize = new Size(MaximumSize.Width, _intial.Height + 10);
                //Size = new Size(Size.Width, _intial.Height + 10);
               
                //panel1.Visible = true;
                //panel2.Visible = true;
            
                //BackColor = _SelectedColor;
                //textEdit1.Properties.AppearanceReadOnly.BackColor = _SelectedColor;
            }
            //BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //BackColor = _SelectedColor;
            else
            {
                lblMain.BackColor = Color.White;
                //MaximumSize = new Size(MaximumSize.Width, 22);
                //Size = new Size(Size.Width, 22);
               
                //BackColor = _notSelectedColor;
                //textEdit1.Properties.AppearanceReadOnly.BackColor = _notSelectedColor;
              
            }
                //BorderStyle = System.Windows.Forms.BorderStyle.None;
                //BackColor = _notSelectedColor;

            _wasSelected = _isSelected;
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

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (OnClicked != null)
                OnClicked(this, null);
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
                obj = (DatabaseSearchResult)value;

                if (obj.ObjectType == ObjType.StoredProc) 
                {
                    btnExecute.Visible = true;
                    btnLocate.Image = HuntingDog.Properties.Resources.scroll;
                    btnExecute.Image = HuntingDog.Properties.Resources.media_play_green;
                    btnExecute.ToolTip = "Execute stored procedure";
                    lblMain.ToolTip = "Double click to modify stored procedure";
                }
                else if (obj.ObjectType == ObjType.Func) 
                {
                    btnExecute.Visible = true;
                    btnLocate.Image = HuntingDog.Properties.Resources.text_formula;
                }
                else if (obj.ObjectType == ObjType.Table)
                {
                    btnExecute.Visible = true;
                    btnLocate.Image = HuntingDog.Properties.Resources.table_sql;
                    btnExecute.Image = HuntingDog.Properties.Resources.wrench;
                    btnExecute.ToolTip = "Design table";
                    lblMain.ToolTip = "Double click to edit table data";
                }
                else if (obj.ObjectType == ObjType.View)
                {
                    btnExecute.Visible = false;
                    btnLocate.Image = HuntingDog.Properties.Resources.text_align_center;
                }

                lblMain.Text = obj.HighlightName;


            }
        }

     

        Font boldFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);


        public event ListHandler OnClicked;
        public event ListHandler OnDoubleClicked;
        public event ListHandler<ActionArgs> OnAction;


        private void textEdit1_MouseMove(object sender, MouseEventArgs e)
        {
            //if (a == false && leave == false)
            //{
            //    Transition t1 = new Transition(new TransitionType_EaseInEaseOut(100));
            //    t1.add(this.textEdit1, "Left", this.textEdit1.Left - 5);

            //    Transition t2 = new Transition(new TransitionType_EaseInEaseOut(100));
            //    t2.add(this.textEdit1, "Left", this.textEdit1.Left + 2);

            //    Transition t3 = new Transition(new TransitionType_EaseInEaseOut(100));
            //    t3.add(this.textEdit1, "Left", this.textEdit1.Left);
            //    //t.add(this.textEdit1, "Left", this.textEdit1.Left);

            //    t3.TransitionCompletedEvent += new EventHandler<Transition.Args>(t3_TransitionCompletedEvent);
            //    Transition.runChain(t1, t2, t3);

            //    a = true;
            //    leave = true;
            //}

        }

        void t3_TransitionCompletedEvent(object sender, Transition.Args e)
        {
            a = false;
        }

        bool a = false;
        bool leave = false;

        private void textEdit1_MouseLeave(object sender, EventArgs e)
        {
            
            leave = false;
        }


        public event ListHandler OnKeyPressed;


    
        private void btnLocate_KeyDown(object sender, KeyEventArgs e)
        {
    
        }

        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (OnClicked != null)
                OnClicked(this, null);

           
        }

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }


    

    
    }
}
