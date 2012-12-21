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
    public partial class ucIndexItem : UserControl,IListViewItem
    {
        public ucIndexItem()
        {
            InitializeComponent();
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

        IndexDetail obj;
        public object BoundObject
        {
            get
            {
                return obj;
            }
            set
            {
                obj = (IndexDetail)value;
                txtLeft.Text = obj.PropertyName;
                txtRight.Text = obj.Columns;
                txtClust.Text = "";              
                txtClust.Text = obj.IsClustered?"Clustered":"Non-Clustered";
     
            }
        }

        public event ListHandler OnClicked;

        public event ListHandler OnDoubleClicked;

        public event ListHandler<ActionArgs> OnAction;

        #endregion


        public event ListHandler OnKeyPressed;
    }
}
