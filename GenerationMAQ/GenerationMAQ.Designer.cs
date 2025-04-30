namespace GenerationMAQ
{
    partial class GenerationMAQ
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
            this.BpDownloadFile = new System.Windows.Forms.Button();
            this.BpGenration = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ChkTagMem = new System.Windows.Forms.CheckBox();
            this.ChkTagOut = new System.Windows.Forms.CheckBox();
            this.ChkTagIn = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txBInformations);
            this.groupBox1.Location = new System.Drawing.Point(180, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(229, 189);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Informations";
            // 
            // txBInformations
            // 
            this.txBInformations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txBInformations.Location = new System.Drawing.Point(10, 21);
            this.txBInformations.Margin = new System.Windows.Forms.Padding(2);
            this.txBInformations.Name = "txBInformations";
            this.txBInformations.ReadOnly = true;
            this.txBInformations.Size = new System.Drawing.Size(219, 166);
            this.txBInformations.TabIndex = 5;
            this.txBInformations.Text = "";
            // 
            // BpSelectProject
            // 
            this.BpSelectProject.Location = new System.Drawing.Point(35, 19);
            this.BpSelectProject.Name = "BpSelectProject";
            this.BpSelectProject.Size = new System.Drawing.Size(99, 31);
            this.BpSelectProject.TabIndex = 10;
            this.BpSelectProject.Text = "Sélectionner le projet";
            this.BpSelectProject.UseVisualStyleBackColor = true;
            this.BpSelectProject.Click += new System.EventHandler(this.BpSelectProject_Click);
            // 
            // BpDownloadFile
            // 
            this.BpDownloadFile.Enabled = false;
            this.BpDownloadFile.Location = new System.Drawing.Point(81, 56);
            this.BpDownloadFile.Name = "BpDownloadFile";
            this.BpDownloadFile.Size = new System.Drawing.Size(84, 37);
            this.BpDownloadFile.TabIndex = 13;
            this.BpDownloadFile.Text = "Télécharger le fichier d\'export";
            this.BpDownloadFile.UseVisualStyleBackColor = true;
            this.BpDownloadFile.Click += new System.EventHandler(this.BpDownloadFile_Click);
            // 
            // BpGenration
            // 
            this.BpGenration.Enabled = false;
            this.BpGenration.Location = new System.Drawing.Point(6, 56);
            this.BpGenration.Name = "BpGenration";
            this.BpGenration.Size = new System.Drawing.Size(69, 37);
            this.BpGenration.TabIndex = 12;
            this.BpGenration.Text = "Générer les tags";
            this.BpGenration.UseVisualStyleBackColor = true;
            this.BpGenration.Click += new System.EventHandler(this.BpGenration_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BpSelectProject);
            this.groupBox2.Controls.Add(this.BpDownloadFile);
            this.groupBox2.Controls.Add(this.BpGenration);
            this.groupBox2.Location = new System.Drawing.Point(3, 97);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(171, 95);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Export des MAQ";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ChkTagMem);
            this.groupBox3.Controls.Add(this.ChkTagOut);
            this.groupBox3.Controls.Add(this.ChkTagIn);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(171, 88);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Choix des tags";
            // 
            // ChkTagMem
            // 
            this.ChkTagMem.AutoSize = true;
            this.ChkTagMem.Location = new System.Drawing.Point(6, 65);
            this.ChkTagMem.Name = "ChkTagMem";
            this.ChkTagMem.Size = new System.Drawing.Size(139, 17);
            this.ChkTagMem.TabIndex = 2;
            this.ChkTagMem.Text = "Mnémoniques automate";
            this.ChkTagMem.UseVisualStyleBackColor = true;
            // 
            // ChkTagOut
            // 
            this.ChkTagOut.AutoSize = true;
            this.ChkTagOut.Location = new System.Drawing.Point(6, 42);
            this.ChkTagOut.Name = "ChkTagOut";
            this.ChkTagOut.Size = new System.Drawing.Size(105, 17);
            this.ChkTagOut.TabIndex = 1;
            this.ChkTagOut.Text = "Sorties automate";
            this.ChkTagOut.UseVisualStyleBackColor = true;
            // 
            // ChkTagIn
            // 
            this.ChkTagIn.AutoSize = true;
            this.ChkTagIn.Location = new System.Drawing.Point(6, 19);
            this.ChkTagIn.Name = "ChkTagIn";
            this.ChkTagIn.Size = new System.Drawing.Size(109, 17);
            this.ChkTagIn.TabIndex = 0;
            this.ChkTagIn.Text = "Entrées automate";
            this.ChkTagIn.UseVisualStyleBackColor = true;
            // 
            // GenerationMAQ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "GenerationMAQ";
            this.Size = new System.Drawing.Size(412, 195);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BpSelectProject;
        private System.Windows.Forms.RichTextBox txBInformations;
        private System.Windows.Forms.Button BpDownloadFile;
        private System.Windows.Forms.Button BpGenration;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox ChkTagOut;
        private System.Windows.Forms.CheckBox ChkTagIn;
        private System.Windows.Forms.CheckBox ChkTagMem;
    }
}
