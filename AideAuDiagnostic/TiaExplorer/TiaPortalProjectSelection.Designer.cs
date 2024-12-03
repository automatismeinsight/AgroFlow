namespace AideAuDiagnostic.TiaExplorer
{
    partial class TiaPortalProjectSelection
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>S
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiaPortalProjectSelection));
            this.BottomBorder = new System.Windows.Forms.Panel();
            this.RightBorder = new System.Windows.Forms.Panel();
            this.LeftBorder = new System.Windows.Forms.Panel();
            this.LbTitle = new System.Windows.Forms.Label();
            this.AgromIcon = new System.Windows.Forms.PictureBox();
            this.CloseIconButton = new System.Windows.Forms.PictureBox();
            this.gBInstance = new System.Windows.Forms.GroupBox();
            this.bPValidate = new System.Windows.Forms.Button();
            this.cBCurrentProject = new System.Windows.Forms.ComboBox();
            this.lBProject = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).BeginInit();
            this.gBInstance.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomBorder
            // 
            this.BottomBorder.Location = new System.Drawing.Point(0, 190);
            this.BottomBorder.Margin = new System.Windows.Forms.Padding(6);
            this.BottomBorder.Name = "BottomBorder";
            this.BottomBorder.Size = new System.Drawing.Size(800, 10);
            this.BottomBorder.TabIndex = 9;
            // 
            // RightBorder
            // 
            this.RightBorder.Location = new System.Drawing.Point(795, 0);
            this.RightBorder.Margin = new System.Windows.Forms.Padding(6);
            this.RightBorder.Name = "RightBorder";
            this.RightBorder.Size = new System.Drawing.Size(5, 200);
            this.RightBorder.TabIndex = 10;
            // 
            // LeftBorder
            // 
            this.LeftBorder.Location = new System.Drawing.Point(0, 0);
            this.LeftBorder.Margin = new System.Windows.Forms.Padding(6);
            this.LeftBorder.Name = "LeftBorder";
            this.LeftBorder.Size = new System.Drawing.Size(5, 200);
            this.LeftBorder.TabIndex = 11;
            // 
            // LbTitle
            // 
            this.LbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbTitle.Location = new System.Drawing.Point(0, 0);
            this.LbTitle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LbTitle.Name = "LbTitle";
            this.LbTitle.Size = new System.Drawing.Size(800, 50);
            this.LbTitle.TabIndex = 12;
            this.LbTitle.Text = "TIA Portal project selection";
            this.LbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AgromIcon
            // 
            this.AgromIcon.Image = ((System.Drawing.Image)(resources.GetObject("AgromIcon.Image")));
            this.AgromIcon.Location = new System.Drawing.Point(0, 0);
            this.AgromIcon.Margin = new System.Windows.Forms.Padding(6);
            this.AgromIcon.Name = "AgromIcon";
            this.AgromIcon.Size = new System.Drawing.Size(150, 50);
            this.AgromIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AgromIcon.TabIndex = 13;
            this.AgromIcon.TabStop = false;
            // 
            // CloseIconButton
            // 
            this.CloseIconButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseIconButton.Image")));
            this.CloseIconButton.Location = new System.Drawing.Point(750, 0);
            this.CloseIconButton.Margin = new System.Windows.Forms.Padding(6);
            this.CloseIconButton.Name = "CloseIconButton";
            this.CloseIconButton.Size = new System.Drawing.Size(50, 50);
            this.CloseIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.CloseIconButton.TabIndex = 14;
            this.CloseIconButton.TabStop = false;
            this.CloseIconButton.Click += new System.EventHandler(this.CloseIconButton_Click);
            // 
            // gBInstance
            // 
            this.gBInstance.Controls.Add(this.bPValidate);
            this.gBInstance.Controls.Add(this.cBCurrentProject);
            this.gBInstance.Controls.Add(this.lBProject);
            this.gBInstance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.gBInstance.Location = new System.Drawing.Point(14, 59);
            this.gBInstance.Name = "gBInstance";
            this.gBInstance.Size = new System.Drawing.Size(772, 122);
            this.gBInstance.TabIndex = 15;
            this.gBInstance.TabStop = false;
            this.gBInstance.Text = "Current instance of TIA Portal";
            // 
            // bPValidate
            // 
            this.bPValidate.Location = new System.Drawing.Point(306, 80);
            this.bPValidate.Name = "bPValidate";
            this.bPValidate.Size = new System.Drawing.Size(160, 30);
            this.bPValidate.TabIndex = 3;
            this.bPValidate.Text = "Validate selection";
            this.bPValidate.UseVisualStyleBackColor = true;
            this.bPValidate.Click += new System.EventHandler(this.bPValidate_Click);
            // 
            // cBCurrentProject
            // 
            this.cBCurrentProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBCurrentProject.Enabled = false;
            this.cBCurrentProject.FormattingEnabled = true;
            this.cBCurrentProject.Location = new System.Drawing.Point(165, 39);
            this.cBCurrentProject.Name = "cBCurrentProject";
            this.cBCurrentProject.Size = new System.Drawing.Size(569, 28);
            this.cBCurrentProject.TabIndex = 1;
            // 
            // lBProject
            // 
            this.lBProject.AutoSize = true;
            this.lBProject.Location = new System.Drawing.Point(29, 42);
            this.lBProject.Name = "lBProject";
            this.lBProject.Size = new System.Drawing.Size(130, 20);
            this.lBProject.TabIndex = 0;
            this.lBProject.Text = "Current projects :";
            // 
            // TiaPortalProjectSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 200);
            this.Controls.Add(this.gBInstance);
            this.Controls.Add(this.CloseIconButton);
            this.Controls.Add(this.AgromIcon);
            this.Controls.Add(this.LbTitle);
            this.Controls.Add(this.LeftBorder);
            this.Controls.Add(this.RightBorder);
            this.Controls.Add(this.BottomBorder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TiaPortalProjectSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TiaPortalProjectSelection";
            this.Load += new System.EventHandler(this.TiaPortalProjectSelection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).EndInit();
            this.gBInstance.ResumeLayout(false);
            this.gBInstance.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BottomBorder;
        private System.Windows.Forms.Panel RightBorder;
        private System.Windows.Forms.Panel LeftBorder;
        private System.Windows.Forms.Label LbTitle;
        private System.Windows.Forms.PictureBox AgromIcon;
        private System.Windows.Forms.PictureBox CloseIconButton;
        private System.Windows.Forms.GroupBox gBInstance;
        private System.Windows.Forms.Label lBProject;
        private System.Windows.Forms.ComboBox cBCurrentProject;
        private System.Windows.Forms.Button bPValidate;
    }
}