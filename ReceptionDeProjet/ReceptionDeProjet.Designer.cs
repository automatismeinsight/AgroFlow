namespace ReceptionDeProjet
{
    partial class ReceptionDeProjet
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

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txBInformations = new System.Windows.Forms.RichTextBox();
            this.BpSelectProject = new System.Windows.Forms.Button();
            this.BpVerification = new System.Windows.Forms.Button();
            this.BpDownloadFile = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txBInformations);
            this.groupBox1.Location = new System.Drawing.Point(180, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 189);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Informations";
            // 
            // txBInformations
            // 
            this.txBInformations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txBInformations.Location = new System.Drawing.Point(5, 18);
            this.txBInformations.Margin = new System.Windows.Forms.Padding(2);
            this.txBInformations.Name = "txBInformations";
            this.txBInformations.ReadOnly = true;
            this.txBInformations.Size = new System.Drawing.Size(219, 166);
            this.txBInformations.TabIndex = 4;
            this.txBInformations.Text = "";
            // 
            // BpSelectProject
            // 
            this.BpSelectProject.Location = new System.Drawing.Point(33, 21);
            this.BpSelectProject.Name = "BpSelectProject";
            this.BpSelectProject.Size = new System.Drawing.Size(117, 29);
            this.BpSelectProject.TabIndex = 7;
            this.BpSelectProject.Text = "Sélectionner le projet";
            this.BpSelectProject.UseVisualStyleBackColor = true;
            this.BpSelectProject.Click += new System.EventHandler(this.BpSelectProject_Click);
            // 
            // BpVerification
            // 
            this.BpVerification.Enabled = false;
            this.BpVerification.Location = new System.Drawing.Point(33, 73);
            this.BpVerification.Name = "BpVerification";
            this.BpVerification.Size = new System.Drawing.Size(117, 29);
            this.BpVerification.TabIndex = 8;
            this.BpVerification.Text = "Comparer le projet";
            this.BpVerification.UseVisualStyleBackColor = true;
            this.BpVerification.Click += new System.EventHandler(this.BpVerification_Click);
            // 
            // BpDownloadFile
            // 
            this.BpDownloadFile.Enabled = false;
            this.BpDownloadFile.Location = new System.Drawing.Point(33, 125);
            this.BpDownloadFile.Name = "BpDownloadFile";
            this.BpDownloadFile.Size = new System.Drawing.Size(117, 48);
            this.BpDownloadFile.TabIndex = 10;
            this.BpDownloadFile.Text = "Télécharger le fichier d\'export";
            this.BpDownloadFile.UseVisualStyleBackColor = true;
            this.BpDownloadFile.Click += new System.EventHandler(this.BpDownloadFile_Click);
            // 
            // ReceptionDeProjet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BpDownloadFile);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.BpSelectProject);
            this.Controls.Add(this.BpVerification);
            this.Name = "ReceptionDeProjet";
            this.Size = new System.Drawing.Size(412, 195);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BpSelectProject;
        private System.Windows.Forms.Button BpVerification;
        private System.Windows.Forms.Button BpDownloadFile;
        private System.Windows.Forms.RichTextBox txBInformations;
    }
}
