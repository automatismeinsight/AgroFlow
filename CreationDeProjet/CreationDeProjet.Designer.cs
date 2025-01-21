namespace CreationDeProjet
{
    partial class CreationDeProjet
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
            this.bpGeneration = new System.Windows.Forms.Button();
            this.gbCreation = new System.Windows.Forms.GroupBox();
            this.txtInputName = new System.Windows.Forms.TextBox();
            this.gbCreation.SuspendLayout();
            this.SuspendLayout();
            // 
            // bpGeneration
            // 
            this.bpGeneration.Enabled = false;
            this.bpGeneration.Location = new System.Drawing.Point(394, 193);
            this.bpGeneration.Name = "bpGeneration";
            this.bpGeneration.Size = new System.Drawing.Size(153, 63);
            this.bpGeneration.TabIndex = 0;
            this.bpGeneration.Text = "bpGeneration";
            this.bpGeneration.UseVisualStyleBackColor = true;
            this.bpGeneration.Click += new System.EventHandler(this.button1_Click);
            // 
            // gbCreation
            // 
            this.gbCreation.Controls.Add(this.txtInputName);
            this.gbCreation.Controls.Add(this.bpGeneration);
            this.gbCreation.Location = new System.Drawing.Point(22, 26);
            this.gbCreation.Name = "gbCreation";
            this.gbCreation.Size = new System.Drawing.Size(577, 270);
            this.gbCreation.TabIndex = 1;
            this.gbCreation.TabStop = false;
            this.gbCreation.Text = "gbCreation";
            // 
            // txtInputName
            // 
            this.txtInputName.Location = new System.Drawing.Point(22, 31);
            this.txtInputName.Name = "txtInputName";
            this.txtInputName.Size = new System.Drawing.Size(225, 20);
            this.txtInputName.TabIndex = 2;
            this.txtInputName.TextChanged += new System.EventHandler(this.txtInputName_TextChanged);
            // 
            // CreationDeProjet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbCreation);
            this.Name = "CreationDeProjet";
            this.Size = new System.Drawing.Size(630, 314);
            this.gbCreation.ResumeLayout(false);
            this.gbCreation.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bpGeneration;
        private System.Windows.Forms.GroupBox gbCreation;
        private System.Windows.Forms.TextBox txtInputName;
    }
}
