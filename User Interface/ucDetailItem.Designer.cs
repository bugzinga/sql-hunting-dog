namespace DatabaseObjectSearcherUI
{
    partial class ucDetailItem
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
            this.btnAction = new DevExpress.XtraEditors.SimpleButton();
            this.lblName = new DevExpress.XtraEditors.LabelControl();
            this.lblValue = new DevExpress.XtraEditors.LabelControl();
            this.panelDecoration = new System.Windows.Forms.Panel();
            this.lblPrefix = new DevExpress.XtraEditors.LabelControl();
            this.SuspendLayout();
            // 
            // btnAction
            // 
            this.btnAction.AllowFocus = false;
            this.btnAction.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnAction.Appearance.BorderColor = System.Drawing.Color.Transparent;
            this.btnAction.Appearance.Options.UseBackColor = true;
            this.btnAction.Appearance.Options.UseBorderColor = true;
            this.btnAction.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btnAction.Image = global::HuntingDog.Properties.Resources.tree16x16;
            this.btnAction.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btnAction.Location = new System.Drawing.Point(0, 0);
            this.btnAction.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(20, 20);
            this.btnAction.TabIndex = 3;
            this.btnAction.Text = "L";
            this.btnAction.ToolTip = "Locate object in Object Explorer";
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblName.Location = new System.Drawing.Point(27, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(281, 22);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "labelControl1";
            // 
            // lblValue
            // 
            this.lblValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblValue.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblValue.Location = new System.Drawing.Point(322, 0);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(101, 22);
            this.lblValue.TabIndex = 5;
            this.lblValue.Text = "labelControl1";
            // 
            // panelDecoration
            // 
            this.panelDecoration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDecoration.BackColor = System.Drawing.Color.DarkGray;
            this.panelDecoration.Location = new System.Drawing.Point(311, 0);
            this.panelDecoration.Name = "panelDecoration";
            this.panelDecoration.Size = new System.Drawing.Size(2, 24);
            this.panelDecoration.TabIndex = 16;
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPrefix.Location = new System.Drawing.Point(1, 0);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(22, 22);
            this.lblPrefix.TabIndex = 17;
            this.lblPrefix.Text = "labelControl1";
            // 
            // ucDetailItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblPrefix);
            this.Controls.Add(this.panelDecoration);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.btnAction);
            this.MaximumSize = new System.Drawing.Size(6000, 22);
            this.MinimumSize = new System.Drawing.Size(0, 22);
            this.Name = "ucDetailItem";
            this.Size = new System.Drawing.Size(422, 22);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnAction;
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.LabelControl lblValue;
        private System.Windows.Forms.Panel panelDecoration;
        private DevExpress.XtraEditors.LabelControl lblPrefix;


    }
}
