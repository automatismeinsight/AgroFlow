using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using ClosedXML.Excel;
using Common;
using System.Reflection;

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

        Project oTiaProject = new Project();

        public ReceptionDeProjet()
        {
            InitializeComponent();

            TIAAssemblyLoader.SetupControl(this);
 
            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oCompareTiaPLC = new CompareTIA();
        }

        private void BpCdcLoad_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Excel Files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm";
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

            if (GetTiaProject(ref sProjectName) == true)
            {
                UpdateInfo(string.Format($"Le projet cible : {sProjectName} est bien sélectionné"));
                UpdateInfo("-");
            }
            else
            {
                UpdateInfo("Le projet cible n'est pas sélectionné");
               
            }
        }

        private bool GetTiaProject(ref string sProjectName)
        {
            bool bRet = false;
            string sError = string.Empty;

            sProjectName = string.Empty;

            // Test si le projet Tia Portal est déja sélectionné ?
            if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
            {
                // Sélection du projet Tia Portal
                if (oExploreTiaPLC.ChooseTiaProject(ref sError) == true)
                {
                    bRet = true;
                    sProjectName = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
                }
            }
            return bRet;
        }

        private void BpVerification_Click(object sender, EventArgs e)
        {
            string sError = null;

            UpdateInfo("Début de la vérification...");
            txBInformations.Refresh();

            oTiaProject = oCompareTiaPLC.GetPlcDevicesInfo(oExploreTiaPLC.oTiainterface, sError);
 
            UpdateInfo("-");
            // Affiche les informations du projet
            if (oTiaProject == null) {
                UpdateInfo("Erreur lors de la récupération des informations du projet");
                return;
            }
            UpdateInfo("Informations du projet : ");
            UpdateInfo($"Nom : {oTiaProject.sName}");
            UpdateInfo($"Chemin : {oTiaProject.sProjectPath}");
            UpdateInfo($"Version : {oTiaProject.sVersion}");
            UpdateInfo($"Date de création : {oTiaProject.sDateCreation}");
            UpdateInfo($"Langue : {oTiaProject.sLanguage}");
            UpdateInfo($"Taille : {oTiaProject.sSize}");
            UpdateInfo($"Simulation : {oTiaProject.sSimulation}");
            UpdateInfo($"Nombre d'automates : {oTiaProject.oAutomates.Count}");
            UpdateInfo("-");
            UpdateInfo("Device trouvé : ");
            foreach (Automate plc in oTiaProject.oAutomates)
            {
                UpdateInfo("");
                UpdateInfo($"Name: {plc.sName}");
                UpdateInfo($"Gamme: {plc.sGamme}");
                UpdateInfo($"Reference: {plc.sReference}");
                UpdateInfo($"Firmware: {plc.sFirmware}");
                UpdateInfo($"NtpServer1: {plc.sNtpServer1}");
                UpdateInfo($"NtpServer2: {plc.sNtpServer2}");
                UpdateInfo($"NtpServer3: {plc.sNtpServer3}");
                UpdateInfo($"LocalHour: {plc.sLocalHour}");
                UpdateInfo($"HourChange: {plc.sHourChange}");
                UpdateInfo($"InterfaceX1: {plc.sInterfaceX1}");
                UpdateInfo($"VlanX1: {plc.sVlanX1}");
                UpdateInfo($"InterfaceX2: {plc.sInterfaceX2}");
                UpdateInfo($"VlanX2: {plc.sVlanX2}");
                UpdateInfo($"MMCLife: {plc.sMMCLife}");
                UpdateInfo($"WatchDog: {plc.sWatchDog}");
                UpdateInfo($"Restart: {plc.sRestart}");
                UpdateInfo($"CadenceM0: {plc.sCadenceM0}");
                UpdateInfo($"CadenceM1: {plc.sCadenceM1}");
                UpdateInfo($"ProgramProtection: {plc.sProgramProtection}");
                UpdateInfo($"WebServer: {plc.sWebServer}");
                UpdateInfo($"ControlAccess: {plc.sControlAccess}");
                UpdateInfo($"ApiHmiCom: {plc.sApiHmiCom}");
                UpdateInfo($"OnlineAccess: {plc.sOnlineAccess}");
                UpdateInfo($"ScreenWrite: {plc.sScreenWrite}");
                UpdateInfo($"InstantVar: {plc.sInstantVar}");
                UpdateInfo($"StandardLTU: {plc.iStandardLTU}");
                UpdateInfo($"OB1PID: {plc.sOB1PID}");
                UpdateInfo($"BlocOb1: {plc.iBlocOb1}");
                UpdateInfo($"BlocOb35: {plc.iBlocOb35}");
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
                    // Infos Projet
                    var ws = wb.Worksheet("PROJECT"); // Get the first worksheet in the workbook
                    var range = ws.RangeUsed(); // Get the range of cells used in the worksheet

                    ws.Cell(2, 1).Value = oTiaProject.sName;
                    ws.Cell(2, 2).Value = oTiaProject.sProjectPath;
                    ws.Cell(2, 3).Value = oTiaProject.sVersion;
                    ws.Cell(2, 4).Value = oTiaProject.sDateCreation;
                    ws.Cell(2, 5).Value = oTiaProject.sLanguage;
                    ws.Cell(2, 6).Value = oTiaProject.sSize;
                    ws.Cell(2, 7).Value = oTiaProject.sSimulation;
                    ws.Cell(2, 8).Value = oTiaProject.oAutomates.Count;

                    // Infos automates
                    int currentRow = 2; // Start at row 2
                    ws = wb.Worksheet("PLC"); // Get the first worksheet in the workbook
                    range = ws.RangeUsed(); // Get the range of cells used in the worksheet

                    for (int i = 2; i <= range.LastRowUsed().RowNumber(); i++)
                    {
                        ws.Row(i).Clear(); // Clear the row
                    }

                    foreach (Automate plc in oTiaProject.oAutomates)
                    {

                        ws.Cell(currentRow, 1).Value = plc.sName;
                        ws.Cell(currentRow, 2).Value = plc.sGamme;
                        ws.Cell(currentRow, 3).Value = plc.sReference;
                        ws.Cell(currentRow, 4).Value = plc.sFirmware;
                        ws.Cell(currentRow, 5).Value = plc.sNtpServer1;
                        ws.Cell(currentRow, 6).Value = plc.sNtpServer2;
                        ws.Cell(currentRow, 7).Value = plc.sNtpServer3;
                        ws.Cell(currentRow, 8).Value = plc.sLocalHour;
                        ws.Cell(currentRow, 9).Value = plc.sHourChange;
                        ws.Cell(currentRow, 10).Value = plc.sInterfaceX1;
                        ws.Cell(currentRow, 11).Value = plc.sVlanX1;
                        ws.Cell(currentRow, 12).Value = plc.sConnectedDeviceX1;
                        ws.Cell(currentRow, 13).Value = plc.sInterfaceX2;
                        ws.Cell(currentRow, 14).Value = plc.sVlanX2;
                        ws.Cell(currentRow, 15).Value = plc.sConnectedDeviceX2;
                        ws.Cell(currentRow, 16).Value = plc.sMMCLife;
                        ws.Cell(currentRow, 17).Value = plc.sWatchDog;
                        ws.Cell(currentRow, 18).Value = plc.sRestart;
                        ws.Cell(currentRow, 19).Value = plc.sCadenceM0;
                        ws.Cell(currentRow, 20).Value = plc.sCadenceM1;
                        ws.Cell(currentRow, 21).Value = plc.sProgramProtection;
                        ws.Cell(currentRow, 22).Value = plc.sWebServer;
                        ws.Cell(currentRow, 23).Value = plc.sControlAccess;
                        ws.Cell(currentRow, 24).Value = plc.sApiHmiCom;
                        ws.Cell(currentRow, 25).Value = plc.sOnlineAccess;
                        ws.Cell(currentRow, 26).Value = plc.sScreenWrite;
                        ws.Cell(currentRow, 27).Value = plc.sInstantVar;
                        ws.Cell(currentRow, 28).Value = plc.iStandardLTU;
                        ws.Cell(currentRow, 29).Value = plc.sOB1PID;
                        ws.Cell(currentRow, 30).Value = plc.iBlocOb1;
                        ws.Cell(currentRow, 31).Value = plc.iBlocOb35;
                        currentRow++; // Go to the next row
                    }
                    wb.Save(); //Save file
                }
                UpdateInfo("Exportation des données vers Excel terminée");
            }
            catch (Exception ex)
            {
                UpdateInfo($"Erreur lors de l'exportation des données vers Excel : {ex.Message}");
            }
        }

        public void ResetPlcTitleExcel(XLWorkbook wb, int sheet)
        {
            var ws = wb.Worksheet(sheet);

            ws.Cell(1, 1).Value = "sName";
            ws.Cell(1, 1).Value = "sGamme";
            ws.Cell(1, 2).Value = "sReference";
            ws.Cell(1, 3).Value = "sFirmware";
            ws.Cell(1, 4).Value = "sNtpServer1";
            ws.Cell(1, 5).Value = "sNtpServer2";
            ws.Cell(1, 6).Value = "sNtpServer3";
            ws.Cell(1, 7).Value = "sLocalHour";
            ws.Cell(1, 8).Value = "sHourChange";
            ws.Cell(1, 10).Value = "sInterfaceX1";
            ws.Cell(1, 11).Value = "sVlanX1";
            ws.Cell(1, 12).Value = "sInterfaceX2";
            ws.Cell(1, 13).Value = "sVlanX2";
            ws.Cell(1, 14).Value = "sMMCLife";
            ws.Cell(1, 15).Value = "sWatchDog";
            ws.Cell(1, 17).Value = "sRestart";
            ws.Cell(1, 18).Value = "sCadenceM0";
            ws.Cell(1, 19).Value = "sCadenceM1";
            ws.Cell(1, 21).Value = "sProgramProtection";
            ws.Cell(1, 22).Value = "sWebServer";
            ws.Cell(1, 23).Value = "sControlAccess";
            ws.Cell(1, 24).Value = "sApiHmiCom";
            ws.Cell(1, 25).Value = "sOnlineAccess";
            ws.Cell(1, 26).Value = "sScreenWrite";
            ws.Cell(1, 31).Value = "sInstantVar";
            ws.Cell(1, 32).Value = "iStandardLTU";
            ws.Cell(1, 20).Value = "sOB1PID";
            ws.Cell(1, 33).Value = "iBlocOb1";
            ws.Cell(1, 33).Value = "iBlocOb35";
        }

        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now; // Actual date and time
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "------------------------------------------------------------------------------------------------------------------\n";
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
