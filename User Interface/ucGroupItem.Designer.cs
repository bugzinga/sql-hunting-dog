namespace DatabaseObjectSearcherUI
{
    partial class ucGroupItem
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
            this.btnLocate = new DevExpress.XtraEditors.SimpleButton();
            this.btnDirection = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // textEdit1
            // 
            this.textEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textEdit1.EditValue = "Uses";
            this.textEdit1.Enabled = false;
            this.textEdit1.Location = new System.Drawing.Point(28, 3);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Properties.AppearanceReadOnly.BackColor = System.Drawing.Color.White;
            this.textEdit1.Properties.AppearanceReadOnly.BorderColor = System.Drawing.Color.White;
            this.textEdit1.Properties.AppearanceReadOnly.ForeColor = System.Drawing.Color.Black;
            this.textEdit1.Properties.AppearanceReadOnly.Options.UseBackColor = true;
            this.textEdit1.Properties.AppearanceReadOnly.Options.UseBorderColor = true;
            this.textEdit1.Properties.AppearanceReadOnly.Options.UseForeColor = true;
            this.textEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.textEdit1.Properties.ReadOnly = true;
            this.textEdit1.Size = new System.Drawing.Size(324, 18);
            this.textEdit1.TabIndex = 3;
            // 
            // btnLocate
            // 
            this.btnLocate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLocate.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnLocate.Appearance.BorderColor = System.Drawing.Color.Transparent;
            this.btnLocate.Appearance.Options.UseBackColor = true;
            this.btnLocate.Appearance.Options.UseBorderColor = true;
            this.btnLocate.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btnLocate.Image = global::HuntingDog.Properties.Resources.scroll;
            this.btnLocate.Location = new System.Drawing.Point(358, 1);
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(20, 20);
            this.btnLocate.TabIndex = 0;
            this.btnLocate.Text = "L";
            this.btnLocate.Click += new System.EventHandler(this.btnLocate_Click);
            // 
            // btnDirection
            // 
            this.btnDirection.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnDirection.Appearance.BorderColor = System.Drawing.Color.Transparent;
            this.btnDirection.Appearance.Options.UseBackColor = true;
            this.btnDirection.Appearance.Options.UseBorderColor = true;
            this.btnDirection.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btnDirection.Image = global::HuntingDog.Properties.Resources.arrow_left_blue;
            this.btnDirection.Location = new System.Drawing.Point(2, 2);
            this.btnDirection.Name = "btnDirection";
            this.btnDirection.Size = new System.Drawing.Size(20, 20);
            this.btnDirection.TabIndex = 4;
            this.btnDirection.Click += new System.EventHandler(this.btnDirection_Click);
            // 
            // ucGroupItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnDirection);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.btnLocate);
            this.DoubleBuffered = true;
            this.MaximumSize = new System.Drawing.Size(800, 22);
            this.MinimumSize = new System.Drawing.Size(0, 22);
            this.Name = "ucGroupItem";
            this.Size = new System.Drawing.Size(380, 22);
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnLocate;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.SimpleButton btnDirection;

    }
}
