using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;

namespace ReceptionDeProjet
{
    public partial class ReceptionDeProjet: UserControl
    {
        protected string sCdcFilePath = null;

        // Objet interface Tia Portal
        public ExploreTiaPLC oExploreTiaPLC;
        // Paramètres généraux pour l'application
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();
        //Liste des instructions pour le FC
        private readonly List<string> lsDataCollection = new List<string>();

        public ReceptionDeProjet()
        {
            InitializeComponent();

            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
        }

        private void BpCdcLoad_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "xlsx files (*.xlsx) | *.xlsx";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            sCdcFilePath += filePath;
        }

        private void BpSelectProject_Click(object sender, EventArgs e)
        {
            string sProjectName = string.Empty;

            UpdateInfo("-");

            if (GetTiaProject(ref sProjectName) == true)
            {
                UpdateInfo(string.Format(@"Le projet cible : {0} est bien sélectionné", sProjectName));
                
            }
            else
            {
                UpdateInfo("Le projet cible n'est pas sélectionné");
               
            }
        }

        private bool GetTiaProject(ref string sProjectName)
        {
            bool bRet = true;
            string sError = string.Empty;

            sProjectName = string.Empty;

            // Test si le projet Tia Portal est déja sélectionné ?
            if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
            {
                // Sélection du projet Tia Portal
                if (oExploreTiaPLC.ChooseTiaProject(ref sError) == false)
                {
                    bRet = false;
                    sProjectName = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
                }
            }
            return bRet;
        }


        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now; // Actual date and time
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "----------------------------------------------------------------\n";
            }
            else
            {
                sFullMessage = $"{sCurrentDateTime} {sMessage}\n";
            }
            // Ajoute le message au RichTextBox
            txBInformations.AppendText(sFullMessage);

            // Défile automatiquement vers le bas
            txBInformations.SelectionStart = txBInformations.Text.Length;
            txBInformations.ScrollToCaret();
        }

        private void BpVerification_Click(object sender, EventArgs e)
        {
            var plcInfoList = oExploreTiaPLC.GetPlcDevicesInfo();
            foreach (var plc in plcInfoList)
            {
                Console.WriteLine($"Nom: {plc.Name}, IP: {plc.IPAddress}");
            }
        }
    }
}
