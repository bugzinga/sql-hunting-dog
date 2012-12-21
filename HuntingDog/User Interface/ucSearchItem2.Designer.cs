namespace DatabaseObjectSearcherUI
{
    partial class ucSearchItem2
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.itemSelectFrom = new System.Windows.Forms.ToolStripMenuItem();
            this.itemOpenTable = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDesign = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.itemCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExecute = new DevExpress.XtraEditors.SimpleButton();
            this.btnLocate = new DevExpress.XtraEditors.SimpleButton();
            this.lblMain = new WinForms.test.SmartLabel();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.itemSelectFrom,
            this.itemOpenTable,
            this.itemDesign,
            this.toolStripSeparator2,
            this.itemCreate,
            this.itemDelete});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(159, 126);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(155, 6);
            // 
            // itemSelectFrom
            // 
            this.itemSelectFrom.Name = "itemSelectFrom";
            this.itemSelectFrom.Size = new System.Drawing.Size(158, 22);
            this.itemSelectFrom.Text = "Script Select";
            // 
            // itemOpenTable
            // 
            this.itemOpenTable.Name = "itemOpenTable";
            this.itemOpenTable.Size = new System.Drawing.Size(158, 22);
            this.itemOpenTable.Text = "Edit Table Data";
            // 
            // itemDesign
            // 
            this.itemDesign.Name = "itemDesign";
            this.itemDesign.Size = new System.Drawing.Size(158, 22);
            this.itemDesign.Text = "Design Table";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(155, 6);
            // 
            // itemCreate
            // 
            this.itemCreate.Name = "itemCreate";
            this.itemCreate.Size = new System.Drawing.Size(158, 22);
            this.itemCreate.Text = "Script Create";
            // 
            // itemDelete
            // 
            this.itemDelete.Name = "itemDelete";
            this.itemDelete.Size = new System.Drawing.Size(158, 22);
            this.itemDelete.Text = "Script Delete";
            // 
            // btnExecute
            // 
            this.btnExecute.AllowFocus = false;
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnExecute.Appearance.BorderColor = System.Drawing.Color.Transparent;
            this.btnExecute.Appearance.Options.UseBackColor = true;
            this.btnExecute.Appearance.Options.UseBorderColor = true;
            this.btnExecute.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btnExecute.Image = global::HuntingDog.Properties.Resources.wrench;
            this.btnExecute.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnExecute.Location = new System.Drawing.Point(326, 1);
            this.btnExecute.LookAndFeel.SkinName = "Black";
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(20, 20);
            this.btnExecute.TabIndex = 2;
            this.btnExecute.TabStop = false;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnLocate
            // 
            this.btnLocate.AllowFocus = false;
            this.btnLocate.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnLocate.Appearance.BorderColor = System.Drawing.Color.Transparent;
            this.btnLocate.Appearance.Options.UseBackColor = true;
            this.btnLocate.Appearance.Options.UseBorderColor = true;
            this.btnLocate.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btnLocate.Image = global::HuntingDog.Properties.Resources.scroll;
            this.btnLocate.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnLocate.Location = new System.Drawing.Point(4, 2);
            this.btnLocate.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(20, 20);
            this.btnLocate.TabIndex = 0;
            this.btnLocate.Text = "L";
            this.btnLocate.ToolTip = "Locate object in Object Explorer";
            this.btnLocate.Click += new System.EventHandler(this.btnLocate_Click);
            this.btnLocate.KeyDown += new System.Windows.Forms.KeyEventHandler(this.btnLocate_KeyDown);
            // 
            // lblMain
            // 
            this.lblMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMain.Appearance.BackColor = System.Drawing.Color.Gainsboro;
            this.lblMain.Appearance.Font = new System.Drawing.Font("Bitstream Vera Sans Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMain.Appearance.Options.UseBackColor = true;
            this.lblMain.Appearance.Options.UseFont = true;
            this.lblMain.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblMain.ContextMenuStrip = this.contextMenuStrip1;
            this.lblMain.Location = new System.Drawing.Point(31, 2);
            this.lblMain.Name = "lblMain";
            this.lblMain.Size = new System.Drawing.Size(289, 20);
            this.lblMain.TabIndex = 4;
            this.lblMain.Text = "smartLabel1";
            // 
            // ucSearchItem2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblMain);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnLocate);
            this.DoubleBuffered = true;
            this.MaximumSize = new System.Drawing.Size(6000, 22);
            this.MinimumSize = new System.Drawing.Size(0, 22);
            this.Name = "ucSearchItem2";
            this.Size = new System.Drawing.Size(352, 22);
            this.Load += new System.EventHandler(this.ucSearchItem2_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnLocate;
        private DevExpress.XtraEditors.SimpleButton btnExecute;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem itemSelectFrom;
        private System.Windows.Forms.ToolStripMenuItem itemOpenTable;
        private System.Windows.Forms.ToolStripMenuItem itemDesign;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem itemCreate;
        private System.Windows.Forms.ToolStripMenuItem itemDelete;
        private WinForms.test.SmartLabel lblMain;

    }
}
