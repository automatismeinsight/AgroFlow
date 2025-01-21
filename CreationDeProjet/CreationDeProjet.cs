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
       
        //Ajout du nom du projet sans espace
        private void txtInputName_TextChanged(object sender, EventArgs e)
        { 
            lbErrorName.Visible = txtInputName.Text.Contains(" ");
        }

        //Choix du chemin ou le projet sera crée
        private void txtInputPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Veuillez sélectionner un dossier.";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    txtInputPath.Text = folderDialog.SelectedPath;
                    lbErrorPath.Visible = false;
                    bpGeneration.Enabled = !string.IsNullOrEmpty(txtInputName.Text) && !txtInputName.Text.Contains(" ") && !string.IsNullOrEmpty(txtInputPath.Text);
                }
                else
                {
                    lbErrorPath.Visible = true;
                }
            }
        }

        //Génération du projet
        private void bpGeneration_Click(object sender, EventArgs e)
        {
            try
            {
                //Vérifiez si TIA Portal est déjà en cours d'exécution
                tiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                //Configuration du nom et du chemin du projet
                string projectName = txtInputName.Text;
                string projectPath = txtInputPath.Text;

                DirectoryInfo directoryInfo = new DirectoryInfo(projectPath);

                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                //Créez le projet
                tiaProject = tiaPortal.Projects.Create(directoryInfo, projectName);
            }
            catch (Exception ex)
            {
                ;
            }
        }

    }
}
