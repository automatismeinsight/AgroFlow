using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using ClosedXML.Excel;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using System.Windows.Input;

namespace ReceptionDeProjet
{
    public partial class ReceptionDeProjet: UserControl
    {
        protected string sCdcFilePath = null;

        // Objet interface Tia Portal
        public ExploreTiaPLC oExploreTiaPLC;
        public CompareTIA oCompareTiaPLC;
        // Paramètres généraux pour l'application
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();
        //Liste des instructions pour le FC
        private readonly List<string> lsDataCollection = new List<string>();

        List<Automate> oDevicesProject = new List<Automate>();

        public ReceptionDeProjet()
        {
            InitializeComponent();

            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oCompareTiaPLC = new CompareTIA();
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
            UpdateInfo($"Fichier {sCdcFilePath.Split('\\').Last()} chargé");
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

        private void BpVerification_Click(object sender, EventArgs e)
        {
            string sError = null;
            //oDevicesProject = oCompareTiaPLC.GetPlcDevicesInfo(oExploreTiaPLC.oTiainterface, sError);

            UpdateInfo("-");
            UpdateInfo("Device trouvé : ");
            foreach (Automate plc in oDevicesProject)
            {
                UpdateInfo("");
                UpdateInfo($"Nom: {plc.Name}");
                UpdateInfo($"Reference: {plc.Reference}");
                UpdateInfo($"Firmware: {plc.Firmware}");
                UpdateInfo($"WatchDog: {plc.WatchDog}");
                UpdateInfo($"ControlAccess: {plc.ControlAccess}");
                UpdateInfo($"WebServer: {plc.WebServer}");
                UpdateInfo($"Restart: {plc.Restart}");
                UpdateInfo($"CadenceM0: {plc.CadenceM0}");
                UpdateInfo($"CadenceM1: {plc.CadenceM1}");
                UpdateInfo($"LocalHour: {plc.LocalHour}");
                UpdateInfo($"HourChange: {plc.HourChange}");
                UpdateInfo($"MMCLife: {plc.MMCLife}");
                UpdateInfo($"ScreenWrite: {plc.ScreenWrite}");
            }
            UpdateInfo("-");
            //CompareProject();

            //Export DATA To Excel
            ExportDataToExcel();
        }

        void ExportDataToExcel()
        {
            UpdateInfo("Exportation des données vers Excel...");
            try
            {
                using (XLWorkbook wb = new XLWorkbook(sCdcFilePath))
                {
                    var ws = wb.Worksheet(1); // Get the first worksheet in the workbook
                    var range = ws.RangeUsed(); // Get the range of cells used in the worksheet

                    UpdateInfo($"Nombre de lignes : {range.RowCount()}");
                }
            }
            catch (Exception ex)
            {
                UpdateInfo($"Erreur lors de l'exportation des données vers Excel : {ex.Message}");
            }
        }

        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now; // Actual date and time
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "----------------------------------------------------------------------------------------------------------------------\n";
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
    }
}
