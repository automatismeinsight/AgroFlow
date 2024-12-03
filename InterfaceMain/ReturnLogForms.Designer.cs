namespace InterfaceMain
{
    partial class ReturnLogForms
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReturnLogForms));
            this.LbTitle = new System.Windows.Forms.Label();
            this.LeftBorder = new System.Windows.Forms.Panel();
            this.RightBorder = new System.Windows.Forms.Panel();
            this.BottomBorder = new System.Windows.Forms.Panel();
            this.AgromIcon = new System.Windows.Forms.PictureBox();
            this.CloseIconButton = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.TxtBLogs = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // LbTitle
            // 
            this.LbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbTitle.Location = new System.Drawing.Point(0, 0);
            this.LbTitle.Name = "LbTitle";
            this.LbTitle.Size = new System.Drawing.Size(1150, 50);
            this.LbTitle.TabIndex = 0;
            this.LbTitle.Text = "LOGS APP";
            this.LbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LbTitle_MouseDown);
            // 
            // LeftBorder
            // 
            this.LeftBorder.Location = new System.Drawing.Point(0, 0);
            this.LeftBorder.Name = "LeftBorder";
            this.LeftBorder.Size = new System.Drawing.Size(10, 700);
            this.LeftBorder.TabIndex = 1;
            // 
            // RightBorder
            // 
            this.RightBorder.Location = new System.Drawing.Point(1140, 0);
            this.RightBorder.Name = "RightBorder";
            this.RightBorder.Size = new System.Drawing.Size(10, 702);
            this.RightBorder.TabIndex = 2;
            // 
            // BottomBorder
            // 
            this.BottomBorder.Location = new System.Drawing.Point(0, 691);
            this.BottomBorder.Name = "BottomBorder";
            this.BottomBorder.Size = new System.Drawing.Size(1150, 11);
            this.BottomBorder.TabIndex = 3;
            // 
            // AgromIcon
            // 
            this.AgromIcon.Image = ((System.Drawing.Image)(resources.GetObject("AgromIcon.Image")));
            this.AgromIcon.Location = new System.Drawing.Point(5, 0);
            this.AgromIcon.Name = "AgromIcon";
            this.AgromIcon.Size = new System.Drawing.Size(144, 50);
            this.AgromIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AgromIcon.TabIndex = 4;
            this.AgromIcon.TabStop = false;
            this.AgromIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AgromIcon_MouseDown);
            // 
            // CloseIconButton
            // 
            this.CloseIconButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseIconButton.Image")));
            this.CloseIconButton.Location = new System.Drawing.Point(1100, 0);
            this.CloseIconButton.Name = "CloseIconButton";
            this.CloseIconButton.Size = new System.Drawing.Size(50, 50);
            this.CloseIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.CloseIconButton.TabIndex = 5;
            this.CloseIconButton.TabStop = false;
            this.CloseIconButton.Click += new System.EventHandler(this.CloseIconButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TxtBLogs);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(28, 73);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1092, 598);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logs";
            // 
            // TxtBLogs
            // 
            this.TxtBLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtBLogs.Cursor = System.Windows.Forms.Cursors.Default;
            this.TxtBLogs.Location = new System.Drawing.Point(16, 37);
            this.TxtBLogs.Name = "TxtBLogs";
            this.TxtBLogs.ReadOnly = true;
            this.TxtBLogs.Size = new System.Drawing.Size(1059, 544);
            this.TxtBLogs.TabIndex = 7;
            this.TxtBLogs.Text = "";
            // 
            // ReturnLogForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 700);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CloseIconButton);
            this.Controls.Add(this.AgromIcon);
            this.Controls.Add(this.BottomBorder);
            this.Controls.Add(this.RightBorder);
            this.Controls.Add(this.LeftBorder);
            this.Controls.Add(this.LbTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReturnLogForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logs";
            this.Load += new System.EventHandler(this.ReturnLogForms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LbTitle;
        private System.Windows.Forms.Panel LeftBorder;
        private System.Windows.Forms.Panel RightBorder;
        private System.Windows.Forms.Panel BottomBorder;
        private System.Windows.Forms.PictureBox AgromIcon;
        private System.Windows.Forms.PictureBox CloseIconButton;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.RichTextBox TxtBLogs;
    }
}