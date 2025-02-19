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
            this.SuspendLayout();
            // 
            // BpCdcLoad
            // 
            this.BpCdcLoad.Location = new System.Drawing.Point(15, 20);
            this.BpCdcLoad.Name = "BpCdcLoad";
            this.BpCdcLoad.Size = new System.Drawing.Size(75, 23);
            this.BpCdcLoad.TabIndex = 0;
            this.BpCdcLoad.Text = "button1";
            this.BpCdcLoad.UseVisualStyleBackColor = true;
            this.BpCdcLoad.Click += new System.EventHandler(this.BpCdcLoad_Click);
            // 
            // txBInformations
            // 
            this.txBInformations.Location = new System.Drawing.Point(280, 36);
            this.txBInformations.Name = "txBInformations";
            this.txBInformations.Size = new System.Drawing.Size(100, 96);
            this.txBInformations.TabIndex = 1;
            this.txBInformations.Text = "";
            // 
            // BpSelectProject
            // 
            this.BpSelectProject.Location = new System.Drawing.Point(15, 64);
            this.BpSelectProject.Name = "BpSelectProject";
            this.BpSelectProject.Size = new System.Drawing.Size(75, 23);
            this.BpSelectProject.TabIndex = 2;
            this.BpSelectProject.Text = "button1";
            this.BpSelectProject.UseVisualStyleBackColor = true;
            this.BpSelectProject.Click += new System.EventHandler(this.BpSelectProject_Click);
            // 
            // BpVerification
            // 
            this.BpVerification.Location = new System.Drawing.Point(15, 118);
            this.BpVerification.Name = "BpVerification";
            this.BpVerification.Size = new System.Drawing.Size(75, 23);
            this.BpVerification.TabIndex = 3;
            this.BpVerification.Text = "²";
            this.BpVerification.UseVisualStyleBackColor = true;
            this.BpVerification.Click += new System.EventHandler(this.BpVerification_Click);
            // 
            // ReceptionDeProjet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BpVerification);
            this.Controls.Add(this.BpSelectProject);
            this.Controls.Add(this.txBInformations);
            this.Controls.Add(this.BpCdcLoad);
            this.Name = "ReceptionDeProjet";
            this.Size = new System.Drawing.Size(412, 195);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BpCdcLoad;
        private System.Windows.Forms.RichTextBox txBInformations;
        private System.Windows.Forms.Button BpSelectProject;
        private System.Windows.Forms.Button BpVerification;
    }
}
