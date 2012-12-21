using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseObjectSearcherUI
{
    public partial class ucDetailItem : UserControl,IListViewItem
    {
        public ucDetailItem()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        #region IListViewItem Members

        public new Control Control
        {
            get { return this; }
        }

        public bool Selection
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        Detail obj;
        public object BoundObject
        {
            get
            {
                return obj;
            }
            set
            {
                obj = (Detail)value;
                lblName.Text = obj.PropertyName;
                lblValue.Text = obj.PropertyValue;
                btnAction.Image = null;
                lblPrefix.Visible = false;

                if (obj is ColumnDetail)
                {
                    if (((ColumnDetail)obj).isFK)
                        btnAction.Image = HuntingDog.Properties.Resources.key_grey;

                    if (((ColumnDetail)obj).isPK)
                        btnAction.Image = HuntingDog.Properties.Resources.key1;
                       
                }
                else if (obj is ParamDetail)
                {
                    //btnAction.Image = null;
                    //btnAction.Text = (obj as ParamDetail).Out ? "Out" : "In";
                    var defaultValue = (obj as ParamDetail).DefaultValue;
                    if (!string.IsNullOrEmpty(defaultValue))
                    {
                        lblValue.Text += "=" + defaultValue;
                    }
                    lblPrefix.Text = (obj as ParamDetail).Out ? "Out" : "In";
                    lblPrefix.Visible = true;
                }
                else if (obj is IndexDetail)
                {
                    var inDet = obj as IndexDetail;
                    btnAction.Image = null;

                    lblName.Text = inDet.Columns;
                    lblValue.Text = inDet.PropertyName;
                    if (inDet.IsClustered)
                        btnAction.Image = HuntingDog.Properties.Resources.tree16x16;
                }

                if (btnAction.Image == null)
                    btnAction.Visible = false;
             
            }
        }

        public event ListHandler OnClicked;

        public event ListHandler OnDoubleClicked;

        public event ListHandler<ActionArgs> OnAction;

        #endregion


        public event ListHandler OnKeyPressed;

        private void simpleButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
