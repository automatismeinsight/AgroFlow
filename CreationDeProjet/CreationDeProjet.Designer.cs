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
            this.txtInputName = new System.Windows.Forms.TextBox();
            this.lbErrorName = new System.Windows.Forms.Label();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.lbErrorPath = new System.Windows.Forms.Label();
            this.BpStation = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bpGeneration
            // 
            this.bpGeneration.Enabled = false;
            this.bpGeneration.Location = new System.Drawing.Point(256, 24);
            this.bpGeneration.Name = "bpGeneration";
            this.bpGeneration.Size = new System.Drawing.Size(153, 63);
            this.bpGeneration.TabIndex = 0;
            this.bpGeneration.Text = "bpGeneration";
            this.bpGeneration.UseVisualStyleBackColor = true;
            this.bpGeneration.Click += new System.EventHandler(this.bpGeneration_Click);
            // 
            // txtInputName
            // 
            this.txtInputName.Location = new System.Drawing.Point(15, 17);
            this.txtInputName.Name = "txtInputName";
            this.txtInputName.Size = new System.Drawing.Size(225, 20);
            this.txtInputName.TabIndex = 2;
            this.txtInputName.TextChanged += new System.EventHandler(this.txtInputName_TextChanged);
            // 
            // lbErrorName
            // 
            this.lbErrorName.AutoSize = true;
            this.lbErrorName.ForeColor = System.Drawing.Color.Red;
            this.lbErrorName.Location = new System.Drawing.Point(12, 40);
            this.lbErrorName.Name = "lbErrorName";
            this.lbErrorName.Size = new System.Drawing.Size(170, 13);
            this.lbErrorName.TabIndex = 3;
            this.lbErrorName.Text = "Erreur dans le nom, espace interdit";
            this.lbErrorName.Visible = false;
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(15, 67);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(225, 20);
            this.txtInputPath.TabIndex = 4;
            this.txtInputPath.Click += new System.EventHandler(this.txtInputPath_Click);
            // 
            // lbErrorPath
            // 
            this.lbErrorPath.AutoSize = true;
            this.lbErrorPath.ForeColor = System.Drawing.Color.Red;
            this.lbErrorPath.Location = new System.Drawing.Point(12, 90);
            this.lbErrorPath.Name = "lbErrorPath";
            this.lbErrorPath.Size = new System.Drawing.Size(126, 13);
            this.lbErrorPath.TabIndex = 5;
            this.lbErrorPath.Text = "Erreur, le chemin est vide";
            this.lbErrorPath.Visible = false;
            // 
            // BpStation
            // 
            this.BpStation.Location = new System.Drawing.Point(54, 129);
            this.BpStation.Name = "BpStation";
            this.BpStation.Size = new System.Drawing.Size(127, 45);
            this.BpStation.TabIndex = 6;
            this.BpStation.Text = "AddStation";
            this.BpStation.UseVisualStyleBackColor = true;
            this.BpStation.Click += new System.EventHandler(this.BpStation_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(282, 145);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
            // 
            // CreationDeProjet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BpStation);
            this.Controls.Add(this.lbErrorPath);
            this.Controls.Add(this.txtInputPath);
            this.Controls.Add(this.lbErrorName);
            this.Controls.Add(this.txtInputName);
            this.Controls.Add(this.bpGeneration);
            this.Name = "CreationDeProjet";
            this.Size = new System.Drawing.Size(412, 195);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bpGeneration;
        private System.Windows.Forms.TextBox txtInputName;
        private System.Windows.Forms.Label lbErrorName;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Label lbErrorPath;
        private System.Windows.Forms.Button BpStation;
        private System.Windows.Forms.Label label1;
    }
}
