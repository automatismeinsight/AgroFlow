namespace InterfaceMain
{
    partial class MainForms
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForms));
            this.LbTitle = new System.Windows.Forms.Label();
            this.LbLogin = new System.Windows.Forms.Label();
            this.CbTIAVersion = new System.Windows.Forms.ComboBox();
            this.CbFunction = new System.Windows.Forms.ComboBox();
            this.GbFunctions = new System.Windows.Forms.GroupBox();
            this.AgromIcon = new System.Windows.Forms.PictureBox();
            this.CloseIconButton = new System.Windows.Forms.PictureBox();
            this.MaximizeIconButton = new System.Windows.Forms.PictureBox();
            this.MinimizeIconButton = new System.Windows.Forms.PictureBox();
            this.LeftBorder = new System.Windows.Forms.Panel();
            this.RightBorder = new System.Windows.Forms.Panel();
            this.BottomBorder = new System.Windows.Forms.Panel();
            this.CloseFullIconButton = new System.Windows.Forms.PictureBox();
            this.UndeveloppIconButton = new System.Windows.Forms.PictureBox();
            this.GbMenu = new System.Windows.Forms.GroupBox();
            this.TerminalIconButton = new System.Windows.Forms.PictureBox();
            this.DeveloppIconButton = new System.Windows.Forms.PictureBox();
            this.LoginIconButton = new System.Windows.Forms.PictureBox();
            this.LogoutIconButton = new System.Windows.Forms.PictureBox();
            this.InfoPictureButton = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaximizeIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinimizeIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseFullIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UndeveloppIconButton)).BeginInit();
            this.GbMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TerminalIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeveloppIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoutIconButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InfoPictureButton)).BeginInit();
            this.SuspendLayout();
            // 
            // LbTitle
            // 
            this.LbTitle.BackColor = System.Drawing.SystemColors.Control;
            this.LbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbTitle.Location = new System.Drawing.Point(0, 0);
            this.LbTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LbTitle.Name = "LbTitle";
            this.LbTitle.Size = new System.Drawing.Size(1150, 50);
            this.LbTitle.TabIndex = 0;
            this.LbTitle.Text = "AgroFlow";
            this.LbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LbTitle_MouseDown);
            // 
            // LbLogin
            // 
            this.LbLogin.BackColor = System.Drawing.SystemColors.Control;
            this.LbLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbLogin.Location = new System.Drawing.Point(714, 19);
            this.LbLogin.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.LbLogin.Name = "LbLogin";
            this.LbLogin.Size = new System.Drawing.Size(225, 75);
            this.LbLogin.TabIndex = 1;
            this.LbLogin.Text = "Login \r\nAccess Level";
            this.LbLogin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LbLogin.Visible = false;
            // 
            // CbTIAVersion
            // 
            this.CbTIAVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CbTIAVersion.FormattingEnabled = true;
            this.CbTIAVersion.Location = new System.Drawing.Point(22, 40);
            this.CbTIAVersion.Margin = new System.Windows.Forms.Padding(2);
            this.CbTIAVersion.Name = "CbTIAVersion";
            this.CbTIAVersion.Size = new System.Drawing.Size(335, 37);
            this.CbTIAVersion.TabIndex = 2;
            this.CbTIAVersion.Text = "Sélectionner une version TIA";
            this.CbTIAVersion.SelectedIndexChanged += new System.EventHandler(this.CbTIAVersion_SelectedIndexChanged);
            this.CbTIAVersion.Click += new System.EventHandler(this.CbTIAVersion_Click);
            // 
            // CbFunction
            // 
            this.CbFunction.Enabled = false;
            this.CbFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CbFunction.FormattingEnabled = true;
            this.CbFunction.Location = new System.Drawing.Point(383, 40);
            this.CbFunction.Margin = new System.Windows.Forms.Padding(2);
            this.CbFunction.Name = "CbFunction";
            this.CbFunction.Size = new System.Drawing.Size(300, 37);
            this.CbFunction.TabIndex = 3;
            this.CbFunction.Text = "Sélectionner une fonction";
            this.CbFunction.SelectedIndexChanged += new System.EventHandler(this.CbFunction_SelectedIndexChanged);
            // 
            // GbFunctions
            // 
            this.GbFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GbFunctions.Location = new System.Drawing.Point(25, 175);
            this.GbFunctions.Margin = new System.Windows.Forms.Padding(2);
            this.GbFunctions.Name = "GbFunctions";
            this.GbFunctions.Padding = new System.Windows.Forms.Padding(2);
            this.GbFunctions.Size = new System.Drawing.Size(1100, 500);
            this.GbFunctions.TabIndex = 4;
            this.GbFunctions.TabStop = false;
            this.GbFunctions.Text = "Fonction";
            // 
            // AgromIcon
            // 
            this.AgromIcon.BackColor = System.Drawing.SystemColors.Control;
            this.AgromIcon.Image = ((System.Drawing.Image)(resources.GetObject("AgromIcon.Image")));
            this.AgromIcon.Location = new System.Drawing.Point(5, 0);
            this.AgromIcon.Name = "AgromIcon";
            this.AgromIcon.Size = new System.Drawing.Size(144, 50);
            this.AgromIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AgromIcon.TabIndex = 0;
            this.AgromIcon.TabStop = false;
            this.AgromIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AgromIcon_MouseDown);
            // 
            // CloseIconButton
            // 
            this.CloseIconButton.BackColor = System.Drawing.SystemColors.Control;
            this.CloseIconButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseIconButton.Image")));
            this.CloseIconButton.Location = new System.Drawing.Point(1100, 0);
            this.CloseIconButton.Name = "CloseIconButton";
            this.CloseIconButton.Size = new System.Drawing.Size(50, 50);
            this.CloseIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.CloseIconButton.TabIndex = 5;
            this.CloseIconButton.TabStop = false;
            this.CloseIconButton.Click += new System.EventHandler(this.CloseIconButton_Click);
            // 
            // MaximizeIconButton
            // 
            this.MaximizeIconButton.BackColor = System.Drawing.SystemColors.Control;
            this.MaximizeIconButton.Image = ((System.Drawing.Image)(resources.GetObject("MaximizeIconButton.Image")));
            this.MaximizeIconButton.Location = new System.Drawing.Point(1050, 0);
            this.MaximizeIconButton.Name = "MaximizeIconButton";
            this.MaximizeIconButton.Size = new System.Drawing.Size(50, 50);
            this.MaximizeIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.MaximizeIconButton.TabIndex = 6;
            this.MaximizeIconButton.TabStop = false;
            this.MaximizeIconButton.Click += new System.EventHandler(this.MaximizeIconButton_Click);
            // 
            // MinimizeIconButton
            // 
            this.MinimizeIconButton.BackColor = System.Drawing.SystemColors.Control;
            this.MinimizeIconButton.Image = ((System.Drawing.Image)(resources.GetObject("MinimizeIconButton.Image")));
            this.MinimizeIconButton.Location = new System.Drawing.Point(1000, 0);
            this.MinimizeIconButton.Name = "MinimizeIconButton";
            this.MinimizeIconButton.Size = new System.Drawing.Size(50, 50);
            this.MinimizeIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.MinimizeIconButton.TabIndex = 0;
            this.MinimizeIconButton.TabStop = false;
            this.MinimizeIconButton.Click += new System.EventHandler(this.MinimizeIconButton_Click);
            // 
            // LeftBorder
            // 
            this.LeftBorder.Location = new System.Drawing.Point(0, 0);
            this.LeftBorder.Name = "LeftBorder";
            this.LeftBorder.Size = new System.Drawing.Size(5, 700);
            this.LeftBorder.TabIndex = 7;
            // 
            // RightBorder
            // 
            this.RightBorder.Location = new System.Drawing.Point(1145, 0);
            this.RightBorder.Name = "RightBorder";
            this.RightBorder.Size = new System.Drawing.Size(5, 700);
            this.RightBorder.TabIndex = 8;
            // 
            // BottomBorder
            // 
            this.BottomBorder.Location = new System.Drawing.Point(0, 690);
            this.BottomBorder.Name = "BottomBorder";
            this.BottomBorder.Size = new System.Drawing.Size(1150, 10);
            this.BottomBorder.TabIndex = 9;
            // 
            // CloseFullIconButton
            // 
            this.CloseFullIconButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseFullIconButton.Image")));
            this.CloseFullIconButton.Location = new System.Drawing.Point(1050, 0);
            this.CloseFullIconButton.Name = "CloseFullIconButton";
            this.CloseFullIconButton.Size = new System.Drawing.Size(50, 50);
            this.CloseFullIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.CloseFullIconButton.TabIndex = 10;
            this.CloseFullIconButton.TabStop = false;
            this.CloseFullIconButton.Visible = false;
            this.CloseFullIconButton.Click += new System.EventHandler(this.CloseFullIconButton_Click);
            // 
            // UndeveloppIconButton
            // 
            this.UndeveloppIconButton.BackColor = System.Drawing.Color.Transparent;
            this.UndeveloppIconButton.Image = ((System.Drawing.Image)(resources.GetObject("UndeveloppIconButton.Image")));
            this.UndeveloppIconButton.Location = new System.Drawing.Point(1035, 37);
            this.UndeveloppIconButton.Name = "UndeveloppIconButton";
            this.UndeveloppIconButton.Size = new System.Drawing.Size(40, 40);
            this.UndeveloppIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.UndeveloppIconButton.TabIndex = 11;
            this.UndeveloppIconButton.TabStop = false;
            this.UndeveloppIconButton.Click += new System.EventHandler(this.UndeveloppIconButton_Click);
            // 
            // GbMenu
            // 
            this.GbMenu.Controls.Add(this.TerminalIconButton);
            this.GbMenu.Controls.Add(this.DeveloppIconButton);
            this.GbMenu.Controls.Add(this.UndeveloppIconButton);
            this.GbMenu.Controls.Add(this.CbTIAVersion);
            this.GbMenu.Controls.Add(this.CbFunction);
            this.GbMenu.Controls.Add(this.LbLogin);
            this.GbMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GbMenu.Location = new System.Drawing.Point(25, 66);
            this.GbMenu.Name = "GbMenu";
            this.GbMenu.Size = new System.Drawing.Size(1100, 100);
            this.GbMenu.TabIndex = 12;
            this.GbMenu.TabStop = false;
            this.GbMenu.Text = "Menu";
            // 
            // TerminalIconButton
            // 
            this.TerminalIconButton.Image = ((System.Drawing.Image)(resources.GetObject("TerminalIconButton.Image")));
            this.TerminalIconButton.Location = new System.Drawing.Point(955, 27);
            this.TerminalIconButton.Name = "TerminalIconButton";
            this.TerminalIconButton.Size = new System.Drawing.Size(60, 60);
            this.TerminalIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.TerminalIconButton.TabIndex = 0;
            this.TerminalIconButton.TabStop = false;
            this.TerminalIconButton.Visible = false;
            this.TerminalIconButton.Click += new System.EventHandler(this.IconTerminalButton_Click);
            // 
            // DeveloppIconButton
            // 
            this.DeveloppIconButton.BackColor = System.Drawing.Color.Transparent;
            this.DeveloppIconButton.Image = ((System.Drawing.Image)(resources.GetObject("DeveloppIconButton.Image")));
            this.DeveloppIconButton.Location = new System.Drawing.Point(1035, 4);
            this.DeveloppIconButton.Name = "DeveloppIconButton";
            this.DeveloppIconButton.Size = new System.Drawing.Size(40, 40);
            this.DeveloppIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.DeveloppIconButton.TabIndex = 13;
            this.DeveloppIconButton.TabStop = false;
            this.DeveloppIconButton.Visible = false;
            this.DeveloppIconButton.Click += new System.EventHandler(this.DeveloppIconButton_Click);
            // 
            // LoginIconButton
            // 
            this.LoginIconButton.Image = ((System.Drawing.Image)(resources.GetObject("LoginIconButton.Image")));
            this.LoginIconButton.Location = new System.Drawing.Point(950, 0);
            this.LoginIconButton.Name = "LoginIconButton";
            this.LoginIconButton.Size = new System.Drawing.Size(50, 50);
            this.LoginIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LoginIconButton.TabIndex = 0;
            this.LoginIconButton.TabStop = false;
            this.LoginIconButton.Click += new System.EventHandler(this.LoginIconButton_Click);
            // 
            // LogoutIconButton
            // 
            this.LogoutIconButton.Enabled = false;
            this.LogoutIconButton.Image = ((System.Drawing.Image)(resources.GetObject("LogoutIconButton.Image")));
            this.LogoutIconButton.Location = new System.Drawing.Point(950, 0);
            this.LogoutIconButton.Name = "LogoutIconButton";
            this.LogoutIconButton.Size = new System.Drawing.Size(50, 50);
            this.LogoutIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LogoutIconButton.TabIndex = 13;
            this.LogoutIconButton.TabStop = false;
            this.LogoutIconButton.Visible = false;
            this.LogoutIconButton.Click += new System.EventHandler(this.LogoutIconButton_Click);
            // 
            // InfoPictureButton
            // 
            this.InfoPictureButton.BackColor = System.Drawing.SystemColors.Control;
            this.InfoPictureButton.Image = ((System.Drawing.Image)(resources.GetObject("InfoPictureButton.Image")));
            this.InfoPictureButton.Location = new System.Drawing.Point(900, 0);
            this.InfoPictureButton.Name = "InfoPictureButton";
            this.InfoPictureButton.Size = new System.Drawing.Size(50, 50);
            this.InfoPictureButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.InfoPictureButton.TabIndex = 14;
            this.InfoPictureButton.TabStop = false;
            this.InfoPictureButton.Click += new System.EventHandler(this.InfoPictureButton_Click);
            // 
            // MainForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 700);
            this.Controls.Add(this.InfoPictureButton);
            this.Controls.Add(this.LogoutIconButton);
            this.Controls.Add(this.LoginIconButton);
            this.Controls.Add(this.CloseFullIconButton);
            this.Controls.Add(this.BottomBorder);
            this.Controls.Add(this.MinimizeIconButton);
            this.Controls.Add(this.MaximizeIconButton);
            this.Controls.Add(this.CloseIconButton);
            this.Controls.Add(this.AgromIcon);
            this.Controls.Add(this.GbFunctions);
            this.Controls.Add(this.LbTitle);
            this.Controls.Add(this.LeftBorder);
            this.Controls.Add(this.RightBorder);
            this.Controls.Add(this.GbMenu);
            this.ForeColor = System.Drawing.SystemColors.Desktop;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AgroFlow";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForms_FormClosed);
            this.Load += new System.EventHandler(this.MainForms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MaximizeIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinimizeIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseFullIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UndeveloppIconButton)).EndInit();
            this.GbMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TerminalIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DeveloppIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoutIconButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InfoPictureButton)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label LbTitle;
        private System.Windows.Forms.Label LbLogin;
        public System.Windows.Forms.ComboBox CbTIAVersion;
        private System.Windows.Forms.ComboBox CbFunction;
        private System.Windows.Forms.GroupBox GbFunctions;
        private System.Windows.Forms.PictureBox AgromIcon;
        private System.Windows.Forms.PictureBox CloseIconButton;
        private System.Windows.Forms.PictureBox MaximizeIconButton;
        private System.Windows.Forms.PictureBox MinimizeIconButton;
        private System.Windows.Forms.Panel LeftBorder;
        private System.Windows.Forms.Panel RightBorder;
        private System.Windows.Forms.Panel BottomBorder;
        private System.Windows.Forms.PictureBox CloseFullIconButton;
        private System.Windows.Forms.PictureBox UndeveloppIconButton;
        private System.Windows.Forms.GroupBox GbMenu;
        private System.Windows.Forms.PictureBox DeveloppIconButton;
        private System.Windows.Forms.PictureBox TerminalIconButton;
        private System.Windows.Forms.PictureBox LoginIconButton;
        private System.Windows.Forms.PictureBox LogoutIconButton;
        private System.Windows.Forms.PictureBox InfoPictureButton;
    }
}

