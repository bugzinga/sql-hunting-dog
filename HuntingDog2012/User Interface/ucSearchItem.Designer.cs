namespace DatabaseObjectSearcherUI
{
    partial class ucSearchItem
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
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.btnExecute = new DevExpress.XtraEditors.SimpleButton();
            this.btnLocate = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // textEdit1
            // 
            this.textEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textEdit1.EditValue = "dbo.V2_YES_ICANDOTHIS";
            this.textEdit1.Location = new System.Drawing.Point(30, 1);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Properties.AllowFocused = false;
            this.textEdit1.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.textEdit1.Properties.Appearance.Options.UseBackColor = true;
            this.textEdit1.Properties.AppearanceReadOnly.ForeColor = System.Drawing.Color.Black;
            this.textEdit1.Properties.AppearanceReadOnly.Options.UseForeColor = true;
            this.textEdit1.Properties.AutoHeight = false;
            this.textEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.textEdit1.Properties.ReadOnly = true;
            this.textEdit1.Size = new System.Drawing.Size(293, 17);
            this.textEdit1.TabIndex = 3;
            this.textEdit1.TabStop = false;
            this.textEdit1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textEdit1_KeyDown);
            this.textEdit1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textEdit1_KeyPress);
            this.textEdit1.MouseLeave += new System.EventHandler(this.textEdit1_MouseLeave);
            this.textEdit1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.textEdit1_MouseMove);
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
            // 
            // ucSearchItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.btnLocate);
            this.DoubleBuffered = true;
            this.MaximumSize = new System.Drawing.Size(1900, 22);
            this.MinimumSize = new System.Drawing.Size(0, 22);
            this.Name = "ucSearchItem";
            this.Size = new System.Drawing.Size(352, 22);
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnLocate;
        private DevExpress.XtraEditors.SimpleButton btnExecute;
        private DevExpress.XtraEditors.TextEdit textEdit1;

    }
}
