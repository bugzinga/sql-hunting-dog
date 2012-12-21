namespace DatabaseObjectSearcherUI
{
    partial class ucIndexItem
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
            this.txtLeft = new DevExpress.XtraEditors.TextEdit();
            this.txtRight = new DevExpress.XtraEditors.TextEdit();
            this.txtClust = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLeft.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRight.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClust.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLeft
            // 
            this.txtLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLeft.Location = new System.Drawing.Point(41, 0);
            this.txtLeft.Name = "txtLeft";
            this.txtLeft.Size = new System.Drawing.Size(121, 20);
            this.txtLeft.TabIndex = 0;
            // 
            // txtRight
            // 
            this.txtRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRight.Location = new System.Drawing.Point(162, 0);
            this.txtRight.Name = "txtRight";
            this.txtRight.Size = new System.Drawing.Size(97, 20);
            this.txtRight.TabIndex = 1;
            // 
            // txtClust
            // 
            this.txtClust.EditValue = "PK";
            this.txtClust.Location = new System.Drawing.Point(0, 0);
            this.txtClust.Name = "txtClust";
            this.txtClust.Size = new System.Drawing.Size(43, 20);
            this.txtClust.TabIndex = 3;
            // 
            // ucIndexItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtClust);
            this.Controls.Add(this.txtRight);
            this.Controls.Add(this.txtLeft);
            this.MaximumSize = new System.Drawing.Size(1000, 20);
            this.MinimumSize = new System.Drawing.Size(0, 20);
            this.Name = "ucIndexItem";
            this.Size = new System.Drawing.Size(259, 20);
            ((System.ComponentModel.ISupportInitialize)(this.txtLeft.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRight.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClust.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit txtLeft;
        private DevExpress.XtraEditors.TextEdit txtRight;
        private DevExpress.XtraEditors.TextEdit txtClust;

    }
}
