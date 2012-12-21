namespace DatabaseObjectSearcherUI
{
    partial class ucListView
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
            this.txtKeyCatcher = new System.Windows.Forms.TextBox();
            this.circularProgressControl1 = new HuntingDog.CircularProgressControl();
            this.SuspendLayout();
            // 
            // txtKeyCatcher
            // 
            this.txtKeyCatcher.AcceptsReturn = true;
            this.txtKeyCatcher.AcceptsTab = true;
            this.txtKeyCatcher.Location = new System.Drawing.Point(0, -40);
            this.txtKeyCatcher.Multiline = true;
            this.txtKeyCatcher.Name = "txtKeyCatcher";
            this.txtKeyCatcher.Size = new System.Drawing.Size(15, 20);
            this.txtKeyCatcher.TabIndex = 1;
            this.txtKeyCatcher.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtKeyCatcher_KeyDown);
            // 
            // circularProgressControl1
            // 
            this.circularProgressControl1.BackColor = System.Drawing.Color.Transparent;
            this.circularProgressControl1.Interval = 60;
            this.circularProgressControl1.Location = new System.Drawing.Point(98, 75);
            this.circularProgressControl1.MaximumSize = new System.Drawing.Size(50, 50);
            this.circularProgressControl1.MinimumSize = new System.Drawing.Size(28, 28);
            this.circularProgressControl1.Name = "circularProgressControl1";
            this.circularProgressControl1.Rotation = HuntingDog.CircularProgressControl.Direction.CLOCKWISE;
            this.circularProgressControl1.Size = new System.Drawing.Size(50, 50);
            this.circularProgressControl1.StartAngle = 270;
            this.circularProgressControl1.TabIndex = 0;
            this.circularProgressControl1.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            // 
            // ucListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.txtKeyCatcher);
            this.Controls.Add(this.circularProgressControl1);
            this.DoubleBuffered = true;
            this.Name = "ucListView";
            this.Size = new System.Drawing.Size(376, 439);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HuntingDog.CircularProgressControl circularProgressControl1;
        private System.Windows.Forms.TextBox txtKeyCatcher;



    }
}
