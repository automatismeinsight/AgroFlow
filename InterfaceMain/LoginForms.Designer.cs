namespace InterfaceLoginTest
{
    partial class LoginForms
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForms));
            this.LoginInput = new System.Windows.Forms.TextBox();
            this.PasswordInput = new System.Windows.Forms.TextBox();
            this.BPConnexion = new System.Windows.Forms.Button();
            this.LbTitle = new System.Windows.Forms.Label();
            this.AgromIcon = new System.Windows.Forms.PictureBox();
            this.CloseIconButton = new System.Windows.Forms.PictureBox();
            this.LeftBorder = new System.Windows.Forms.Panel();
            this.RightBorder = new System.Windows.Forms.Panel();
            this.BottomBorder = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).BeginInit();
            this.SuspendLayout();
            // 
            // LoginInput
            // 
            this.LoginInput.Location = new System.Drawing.Point(104, 68);
            this.LoginInput.Margin = new System.Windows.Forms.Padding(2);
            this.LoginInput.Name = "LoginInput";
            this.LoginInput.Size = new System.Drawing.Size(194, 20);
            this.LoginInput.TabIndex = 0;
            this.LoginInput.Text = "Identifiant";
            this.LoginInput.MouseClick += new System.Windows.Forms.MouseEventHandler(this.LoginInput_MouseClick);
            // 
            // PasswordInput
            // 
            this.PasswordInput.Location = new System.Drawing.Point(104, 104);
            this.PasswordInput.Margin = new System.Windows.Forms.Padding(2);
            this.PasswordInput.Name = "PasswordInput";
            this.PasswordInput.Size = new System.Drawing.Size(194, 20);
            this.PasswordInput.TabIndex = 1;
            this.PasswordInput.Text = "Mot de passe";
            this.PasswordInput.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PasswordInput_MouseClick);
            this.PasswordInput.TextChanged += new System.EventHandler(this.PasswordInput_TextChanged);
            this.PasswordInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PasswordInput_KeyDown);
            // 
            // BPConnexion
            // 
            this.BPConnexion.BackColor = System.Drawing.SystemColors.Menu;
            this.BPConnexion.Enabled = false;
            this.BPConnexion.Location = new System.Drawing.Point(156, 140);
            this.BPConnexion.Margin = new System.Windows.Forms.Padding(2);
            this.BPConnexion.Name = "BPConnexion";
            this.BPConnexion.Size = new System.Drawing.Size(88, 27);
            this.BPConnexion.TabIndex = 2;
            this.BPConnexion.Text = "Connexion";
            this.BPConnexion.UseVisualStyleBackColor = false;
            this.BPConnexion.Click += new System.EventHandler(this.BPConnexion_Click);
            // 
            // LbTitle
            // 
            this.LbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.LbTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbTitle.Location = new System.Drawing.Point(0, 0);
            this.LbTitle.Name = "LbTitle";
            this.LbTitle.Size = new System.Drawing.Size(400, 40);
            this.LbTitle.TabIndex = 3;
            this.LbTitle.Text = "Connexion";
            this.LbTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.LbTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LbTitle_MouseDown);
            // 
            // AgromIcon
            // 
            this.AgromIcon.Image = ((System.Drawing.Image)(resources.GetObject("AgromIcon.Image")));
            this.AgromIcon.Location = new System.Drawing.Point(5, 0);
            this.AgromIcon.Name = "AgromIcon";
            this.AgromIcon.Size = new System.Drawing.Size(110, 40);
            this.AgromIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.AgromIcon.TabIndex = 4;
            this.AgromIcon.TabStop = false;
            this.AgromIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AgromIcon_MouseDown);
            // 
            // CloseIconButton
            // 
            this.CloseIconButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseIconButton.Image")));
            this.CloseIconButton.Location = new System.Drawing.Point(360, 0);
            this.CloseIconButton.Name = "CloseIconButton";
            this.CloseIconButton.Size = new System.Drawing.Size(40, 40);
            this.CloseIconButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.CloseIconButton.TabIndex = 5;
            this.CloseIconButton.TabStop = false;
            this.CloseIconButton.Click += new System.EventHandler(this.CloseIconButton_Click);
            // 
            // LeftBorder
            // 
            this.LeftBorder.Location = new System.Drawing.Point(0, 0);
            this.LeftBorder.Name = "LeftBorder";
            this.LeftBorder.Size = new System.Drawing.Size(5, 200);
            this.LeftBorder.TabIndex = 6;
            // 
            // RightBorder
            // 
            this.RightBorder.Location = new System.Drawing.Point(395, 0);
            this.RightBorder.Name = "RightBorder";
            this.RightBorder.Size = new System.Drawing.Size(5, 200);
            this.RightBorder.TabIndex = 7;
            // 
            // BottomBorder
            // 
            this.BottomBorder.Location = new System.Drawing.Point(0, 190);
            this.BottomBorder.Name = "BottomBorder";
            this.BottomBorder.Size = new System.Drawing.Size(400, 10);
            this.BottomBorder.TabIndex = 8;
            // 
            // LoginForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.BottomBorder);
            this.Controls.Add(this.CloseIconButton);
            this.Controls.Add(this.AgromIcon);
            this.Controls.Add(this.LbTitle);
            this.Controls.Add(this.BPConnexion);
            this.Controls.Add(this.PasswordInput);
            this.Controls.Add(this.LoginInput);
            this.Controls.Add(this.LeftBorder);
            this.Controls.Add(this.RightBorder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "LoginForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connexion";
            this.Load += new System.EventHandler(this.LoginForms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.AgromIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseIconButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LoginInput;
        private System.Windows.Forms.TextBox PasswordInput;
        private System.Windows.Forms.Button BPConnexion;
        private System.Windows.Forms.Label LbTitle;
        private System.Windows.Forms.PictureBox AgromIcon;
        private System.Windows.Forms.PictureBox CloseIconButton;
        private System.Windows.Forms.Panel LeftBorder;
        private System.Windows.Forms.Panel RightBorder;
        private System.Windows.Forms.Panel BottomBorder;
    }
}

