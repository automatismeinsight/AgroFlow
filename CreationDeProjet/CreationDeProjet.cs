using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Siemens.Engineering;

namespace CreationDeProjet
{
    public partial class CreationDeProjet : UserControl
    {
        public CreationDeProjet()
        {
            InitializeComponent();
        }

        private TiaPortal tiaPortal;
        private Project tiaProject;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Vérifiez si TIA Portal est déjà en cours d'exécution
                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                // Chemin où le projet sera sauvegardé
                string projectPath = @"D:\SBD\TIAMyProject";
                string projectName = "MyNewProject1";

                DirectoryInfo directoryInfo = new DirectoryInfo(projectPath);

                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                // Créez le projet
                tiaProject = tiaPortal.Projects.Create(directoryInfo, projectName);
                MessageBox.Show($"Projet '{projectName}' créé avec succès à : {projectPath}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création du projet : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
