using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HuntingDog
{
   [Guid("4c410c93-d66b-495a-9de2-99d5bde4a3b8")]
    public partial class ucHost : UserControl
    {
        public ucHost()
        {
            InitializeComponent();
        }

        public DogFace.Face DogFace
        {
            get { return this.face1; }
        }
    }
}
