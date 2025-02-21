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
            this.BpCdcLoad = new System.Windows.Forms.Button();
            this.txBInformations = new System.Windows.Forms.RichTextBox();
            this.BpSelectProject = new System.Windows.Forms.Button();
            this.BpVerification = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BpPdfExport = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BpCdcLoad
            // 
            this.BpCdcLoad.Location = new System.Drawing.Point(47, 22);
            this.BpCdcLoad.Name = "BpCdcLoad";
            this.BpCdcLoad.Size = new System.Drawing.Size(75, 23);
            this.BpCdcLoad.TabIndex = 0;
            this.BpCdcLoad.Text = "FileLoad";
            this.BpCdcLoad.UseVisualStyleBackColor = true;
            this.BpCdcLoad.Click += new System.EventHandler(this.BpCdcLoad_Click);
            // 
            // txBInformations
            // 
            this.txBInformations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txBInformations.Location = new System.Drawing.Point(6, 19);
            this.txBInformations.Name = "txBInformations";
            this.txBInformations.ReadOnly = true;
            this.txBInformations.Size = new System.Drawing.Size(188, 164);
            this.txBInformations.TabIndex = 3;
            this.txBInformations.Text = "";
            // 
            // BpSelectProject
            // 
            this.BpSelectProject.Location = new System.Drawing.Point(47, 63);
            this.BpSelectProject.Name = "BpSelectProject";
            this.BpSelectProject.Size = new System.Drawing.Size(75, 23);
            this.BpSelectProject.TabIndex = 2;
            this.BpSelectProject.Text = "ProjectSelect";
            this.BpSelectProject.UseVisualStyleBackColor = true;
            this.BpSelectProject.Click += new System.EventHandler(this.BpSelectProject_Click);
            // 
            // BpVerification
            // 
            this.BpVerification.Location = new System.Drawing.Point(47, 110);
            this.BpVerification.Name = "BpVerification";
            this.BpVerification.Size = new System.Drawing.Size(75, 23);
            this.BpVerification.TabIndex = 3;
            this.BpVerification.Text = "Compare";
            this.BpVerification.UseVisualStyleBackColor = true;
            this.BpVerification.Click += new System.EventHandler(this.BpVerification_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txBInformations);
            this.groupBox1.Location = new System.Drawing.Point(209, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 189);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Informations";
            // 
            // BpPdfExport
            // 
            this.BpPdfExport.Location = new System.Drawing.Point(47, 163);
            this.BpPdfExport.Name = "BpPdfExport";
            this.BpPdfExport.Size = new System.Drawing.Size(75, 23);
            this.BpPdfExport.TabIndex = 5;
            this.BpPdfExport.Text = "Export PDF";
            this.BpPdfExport.UseVisualStyleBackColor = true;
            this.BpPdfExport.Click += new System.EventHandler(this.BpPdfExport_Click);
            // 
            // ReceptionDeProjet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BpPdfExport);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.BpVerification);
            this.Controls.Add(this.BpSelectProject);
            this.Controls.Add(this.BpCdcLoad);
            this.Name = "ReceptionDeProjet";
            this.Size = new System.Drawing.Size(412, 195);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BpCdcLoad;
        private System.Windows.Forms.RichTextBox txBInformations;
        private System.Windows.Forms.Button BpSelectProject;
        private System.Windows.Forms.Button BpVerification;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BpPdfExport;
    }
}
