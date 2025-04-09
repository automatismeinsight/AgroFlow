namespace AideAuDiagnostic
{
    partial class AideAuDiagnostic
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AideAuDiagnostic));
            this.gBTarget = new System.Windows.Forms.GroupBox();
            this.GIFChargement = new System.Windows.Forms.PictureBox();
            this.bPSelectStation = new System.Windows.Forms.Button();
            this.txBStation = new System.Windows.Forms.TextBox();
            this.txBProjet = new System.Windows.Forms.TextBox();
            this.lBStation = new System.Windows.Forms.Label();
            this.lBProjet = new System.Windows.Forms.Label();
            this.gBExport = new System.Windows.Forms.GroupBox();
            this.bPExport = new System.Windows.Forms.Button();
            this.gBInformations = new System.Windows.Forms.GroupBox();
            this.txBInformations = new System.Windows.Forms.RichTextBox();
            this.bPMaximizeInfo = new System.Windows.Forms.PictureBox();
            this.bPMinimizeInfo = new System.Windows.Forms.PictureBox();
            this.gBTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GIFChargement)).BeginInit();
            this.gBExport.SuspendLayout();
            this.gBInformations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bPMaximizeInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bPMinimizeInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // gBTarget
            // 
            this.gBTarget.Controls.Add(this.GIFChargement);
            this.gBTarget.Controls.Add(this.bPSelectStation);
            this.gBTarget.Controls.Add(this.txBStation);
            this.gBTarget.Controls.Add(this.txBProjet);
            this.gBTarget.Controls.Add(this.lBStation);
            this.gBTarget.Controls.Add(this.lBProjet);
            this.gBTarget.Location = new System.Drawing.Point(15, 10);
            this.gBTarget.Margin = new System.Windows.Forms.Padding(2);
            this.gBTarget.Name = "gBTarget";
            this.gBTarget.Padding = new System.Windows.Forms.Padding(2);
            this.gBTarget.Size = new System.Drawing.Size(254, 174);
            this.gBTarget.TabIndex = 0;
            this.gBTarget.TabStop = false;
            this.gBTarget.Text = "Appareil cible";
            // 
            // GIFChargement
            // 
            this.GIFChargement.BackColor = System.Drawing.SystemColors.Control;
            this.GIFChargement.Image = ((System.Drawing.Image)(resources.GetObject("GIFChargement.Image")));
            this.GIFChargement.Location = new System.Drawing.Point(133, 32);
            this.GIFChargement.Name = "GIFChargement";
            this.GIFChargement.Size = new System.Drawing.Size(116, 84);
            this.GIFChargement.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.GIFChargement.TabIndex = 7;
            this.GIFChargement.TabStop = false;
            // 
            // bPSelectStation
            // 
            this.bPSelectStation.Enabled = false;
            this.bPSelectStation.Location = new System.Drawing.Point(152, 125);
            this.bPSelectStation.Margin = new System.Windows.Forms.Padding(2);
            this.bPSelectStation.Name = "bPSelectStation";
            this.bPSelectStation.Size = new System.Drawing.Size(92, 31);
            this.bPSelectStation.TabIndex = 6;
            this.bPSelectStation.Text = "Sélection de la cible";
            this.bPSelectStation.UseVisualStyleBackColor = true;
            this.bPSelectStation.Click += new System.EventHandler(this.bPSelectStation_Click);
            // 
            // txBStation
            // 
            this.txBStation.Location = new System.Drawing.Point(10, 96);
            this.txBStation.Margin = new System.Windows.Forms.Padding(2);
            this.txBStation.Name = "txBStation";
            this.txBStation.ReadOnly = true;
            this.txBStation.Size = new System.Drawing.Size(236, 20);
            this.txBStation.TabIndex = 5;
            // 
            // txBProjet
            // 
            this.txBProjet.Location = new System.Drawing.Point(10, 40);
            this.txBProjet.Margin = new System.Windows.Forms.Padding(2);
            this.txBProjet.Name = "txBProjet";
            this.txBProjet.ReadOnly = true;
            this.txBProjet.Size = new System.Drawing.Size(236, 20);
            this.txBProjet.TabIndex = 4;
            // 
            // lBStation
            // 
            this.lBStation.AutoSize = true;
            this.lBStation.Location = new System.Drawing.Point(8, 72);
            this.lBStation.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lBStation.Name = "lBStation";
            this.lBStation.Size = new System.Drawing.Size(103, 13);
            this.lBStation.TabIndex = 3;
            this.lBStation.Text = "Station sélectionnée";
            // 
            // lBProjet
            // 
            this.lBProjet.AutoSize = true;
            this.lBProjet.Location = new System.Drawing.Point(8, 20);
            this.lBProjet.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lBProjet.Name = "lBProjet";
            this.lBProjet.Size = new System.Drawing.Size(91, 13);
            this.lBProjet.TabIndex = 2;
            this.lBProjet.Text = "Projet selectionné";
            // 
            // gBExport
            // 
            this.gBExport.Controls.Add(this.bPExport);
            this.gBExport.Location = new System.Drawing.Point(272, 107);
            this.gBExport.Margin = new System.Windows.Forms.Padding(2);
            this.gBExport.Name = "gBExport";
            this.gBExport.Padding = new System.Windows.Forms.Padding(2);
            this.gBExport.Size = new System.Drawing.Size(125, 78);
            this.gBExport.TabIndex = 1;
            this.gBExport.TabStop = false;
            this.gBExport.Text = "Génération du bloc";
            // 
            // bPExport
            // 
            this.bPExport.Location = new System.Drawing.Point(8, 21);
            this.bPExport.Margin = new System.Windows.Forms.Padding(2);
            this.bPExport.Name = "bPExport";
            this.bPExport.Size = new System.Drawing.Size(110, 47);
            this.bPExport.TabIndex = 2;
            this.bPExport.Text = "Générer";
            this.bPExport.UseVisualStyleBackColor = true;
            this.bPExport.Click += new System.EventHandler(this.bPExport_Click);
            // 
            // gBInformations
            // 
            this.gBInformations.Controls.Add(this.txBInformations);
            this.gBInformations.Location = new System.Drawing.Point(272, 10);
            this.gBInformations.Margin = new System.Windows.Forms.Padding(2);
            this.gBInformations.Name = "gBInformations";
            this.gBInformations.Padding = new System.Windows.Forms.Padding(2);
            this.gBInformations.Size = new System.Drawing.Size(125, 93);
            this.gBInformations.TabIndex = 2;
            this.gBInformations.TabStop = false;
            this.gBInformations.Text = "Informations";
            // 
            // txBInformations
            // 
            this.txBInformations.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txBInformations.Location = new System.Drawing.Point(8, 18);
            this.txBInformations.Margin = new System.Windows.Forms.Padding(2);
            this.txBInformations.Name = "txBInformations";
            this.txBInformations.ReadOnly = true;
            this.txBInformations.Size = new System.Drawing.Size(112, 68);
            this.txBInformations.TabIndex = 3;
            this.txBInformations.Text = "";
            // 
            // bPMaximizeInfo
            // 
            this.bPMaximizeInfo.Image = ((System.Drawing.Image)(resources.GetObject("bPMaximizeInfo.Image")));
            this.bPMaximizeInfo.Location = new System.Drawing.Point(390, 10);
            this.bPMaximizeInfo.Margin = new System.Windows.Forms.Padding(2);
            this.bPMaximizeInfo.Name = "bPMaximizeInfo";
            this.bPMaximizeInfo.Size = new System.Drawing.Size(15, 16);
            this.bPMaximizeInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.bPMaximizeInfo.TabIndex = 4;
            this.bPMaximizeInfo.TabStop = false;
            this.bPMaximizeInfo.Click += new System.EventHandler(this.bPMaximizeInfo_Click);
            // 
            // bPMinimizeInfo
            // 
            this.bPMinimizeInfo.Enabled = false;
            this.bPMinimizeInfo.Image = ((System.Drawing.Image)(resources.GetObject("bPMinimizeInfo.Image")));
            this.bPMinimizeInfo.Location = new System.Drawing.Point(390, 10);
            this.bPMinimizeInfo.Margin = new System.Windows.Forms.Padding(2);
            this.bPMinimizeInfo.Name = "bPMinimizeInfo";
            this.bPMinimizeInfo.Size = new System.Drawing.Size(15, 16);
            this.bPMinimizeInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.bPMinimizeInfo.TabIndex = 5;
            this.bPMinimizeInfo.TabStop = false;
            this.bPMinimizeInfo.Visible = false;
            this.bPMinimizeInfo.Click += new System.EventHandler(this.bPMinimize_Click);
            // 
            // AideAuDiagnostic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bPMinimizeInfo);
            this.Controls.Add(this.bPMaximizeInfo);
            this.Controls.Add(this.gBInformations);
            this.Controls.Add(this.gBExport);
            this.Controls.Add(this.gBTarget);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "AideAuDiagnostic";
            this.Size = new System.Drawing.Size(412, 195);
            this.Load += new System.EventHandler(this.AideAuDiagnostic_Load);
            this.gBTarget.ResumeLayout(false);
            this.gBTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GIFChargement)).EndInit();
            this.gBExport.ResumeLayout(false);
            this.gBInformations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bPMaximizeInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bPMinimizeInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gBTarget;
        private System.Windows.Forms.GroupBox gBExport;
        private System.Windows.Forms.Button bPExport;
        private System.Windows.Forms.Label lBProjet;
        private System.Windows.Forms.Button bPSelectStation;
        private System.Windows.Forms.TextBox txBStation;
        private System.Windows.Forms.TextBox txBProjet;
        private System.Windows.Forms.Label lBStation;
        private System.Windows.Forms.GroupBox gBInformations;
        private System.Windows.Forms.RichTextBox txBInformations;
        private System.Windows.Forms.PictureBox bPMaximizeInfo;
        private System.Windows.Forms.PictureBox bPMinimizeInfo;
        private System.Windows.Forms.PictureBox GIFChargement;
    }
}
