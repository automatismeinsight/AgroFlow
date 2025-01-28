using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CreationDeProjet.GenerateObject;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;

namespace CreationDeProjet
{
    public partial class CreationDeProjet : UserControl
    {
        public CreationDeProjet()
        {
            InitializeComponent(); 
        }

        private TiaPortal oTiaPortal;
        private Project oTiaProject;
        private TIAGenerator oTIAGenerator = new TIAGenerator();
        AddStation oAddStation = new AddStation();

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
            string sError = string.Empty;

            while (true)
            {

                if (!oTIAGenerator.ProjectGenerator(ref sError, ref oTiaPortal, ref oTiaProject, txtInputName.Text, txtInputPath.Text)) 
                {
                    Console.WriteLine(sError);
                    break;
                }
                if (!oTIAGenerator.DeviceGenerator(ref sError, ref oTiaProject))
                {
                    Console.WriteLine(sError);
                    break;
                }

                break;
            }
            
        }

        private void BpStation_Click(object sender, EventArgs e)
        {
            oAddStation.ShowDialog();
        }
    }
}
