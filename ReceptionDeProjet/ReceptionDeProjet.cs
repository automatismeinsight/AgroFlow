using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using ClosedXML.Excel;
using Common;
using System.Reflection;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ReceptionDeProjet
{
    /// <summary>
    /// UserControl for the "Project Reception" function in AgroFlow.
    /// <para>
    /// This module allows users to:
    /// <list type="bullet">
    ///   <item>Load and analyze a TIA Portal project.</item>
    ///   <item>Verify compliance and retrieve detailed information (project info, PLCs, settings, etc.).</item>
    ///   <item>Export all relevant configuration and hardware data into a standardized Excel file for traceability and documentation.</item>
    /// </list>
    /// Typical workflow:
    /// <list type="number">
    ///   <item>Select a CDC Excel file (project requirements/specs).</item>
    ///   <item>Select the TIA Portal project to analyze.</item>
    ///   <item>Run the verification step, which collects and displays project information and device details.</item>
    ///   <item>Export the gathered data back into the provided Excel file for archiving or transfer.</item>
    /// </list>
    /// This feature ensures project conformity to standards (Agro Mousquetaires, LTU library, etc.) and provides automatic documentation of PLC hardware and configuration.
    /// </para>
    /// <remarks>
    /// Key components:
    /// <list type="bullet">
    ///   <item><see cref="ExploreTiaPLC"/>: Handles TIA Portal project exploration and selection.</item>
    ///   <item><see cref="CompareTIA"/>: Compares and extracts device information from the selected project.</item>
    ///   <item><see cref="PLC_ProjectDefinitions"/>: Holds configuration and mapping definitions for the PLC project.</item>
    /// </list>
    /// </remarks>
    /// </summary>
    public partial class ReceptionDeProjet : UserControl
    {
        /// <summary>
        /// Path to the loaded CDC Excel file.
        /// </summary>
        protected string sCdcFilePath = null;

        /// <summary>
        /// Interface for exploring the TIA Portal project.
        /// </summary>
        public ExploreTiaPLC oExploreTiaPLC;

        /// <summary>
        /// Interface for comparing the TIA project and retrieving device information.
        /// </summary>
        public CompareTIA oCompareTiaPLC;

        /// <summary>
        /// Global configuration and mapping settings for the PLC project.
        /// </summary>
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();

        /// <summary>
        /// Data storage dictionary for internal processing.
        /// </summary>
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();

        /// <summary>
        /// List of instructions to be generated for the FC (function code).
        /// </summary>
        private readonly List<string> lsDataCollection = new List<string>();

        /// <summary>
        /// Stores the loaded TIA Project information.
        /// </summary>
        MyProject oTiaProject = new MyProject();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionDeProjet"/> UserControl.
        /// Sets up assembly loading and initializes project exploration and comparison interfaces.
        /// </summary>
        public ReceptionDeProjet()
        {
            InitializeComponent();

            TIAAssemblyLoader.SetupControl(this);

            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oCompareTiaPLC = new CompareTIA();

            InitExcel();
        }

        /// <summary>
        /// Handles the "Load CDC" button click event.
        /// Allows the user to select a CDC Excel file and stores its path.
        /// </summary>
        private void InitExcel()
        {
            var filePath = @".\copy_of_source.xlsm";
            var sourcePath = @".\source.xlsm";

            try
            {
                File.Copy(sourcePath, filePath, overwrite: true);
                sCdcFilePath += filePath;
                UpdateInfo($"Fichier {filePath.Split('\\').Last()} chargé");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Erreur lors de la copie : {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the "Select Project" button click event.
        /// Prompts the user to select a TIA Portal project and updates the info panel.
        /// </summary>
        private void BpSelectProject_Click(object sender, EventArgs e)
        {
            string sProjectName = string.Empty;

            if (GetTiaProject(ref sProjectName) == true)
            {
                UpdateInfo(string.Format($"Le projet cible : {sProjectName} est bien sélectionné"));
                UpdateInfo("-");

                BpVerification.Enabled = true;
            }
            else
            {
                UpdateInfo("Le projet cible n'est pas sélectionné");
            }
        }

        /// <summary>
        /// Selects a TIA Portal project via the <see cref="ExploreTiaPLC"/> interface.
        /// </summary>
        /// <param name="sProjectName">Returns the selected project name if successful.</param>
        /// <returns>True if a project is selected, otherwise false.</returns>
        private bool GetTiaProject(ref string sProjectName)
        {
            bool bRet = false;
            string sError = string.Empty;

            sProjectName = string.Empty;

            // Test if a TIA Portal project is already selected
            if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
            {
                // Prompt user to select a TIA Portal project
                if (oExploreTiaPLC.ChooseTiaProject(ref sError) == true)
                {
                    bRet = true;
                    sProjectName = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
                }
            }
            return bRet;
        }

        /// <summary>
        /// Handles the "Verification" button click event.
        /// Retrieves project and PLC device information, updates the info panel, and exports data to Excel.
        /// </summary>
        private void BpVerification_Click(object sender, EventArgs e)
        {
            string sError = null;

            UpdateInfo("Début de la vérification...");
            txBInformations.Refresh();

            oTiaProject = oCompareTiaPLC.GetTiaProjectContains(oExploreTiaPLC.oTiainterface, sError);

            if(sError != null)
            {
                UpdateInfo($"Erreur lors de la récupération du projet : {sError}");
            }

            UpdateInfo("-");
            // Display project information
            if (oTiaProject == null)
            {
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
            UpdateInfo("Devices trouvé : ");
            foreach (MyAutomate plc in oTiaProject.oAutomates)
            {
                UpdateInfo("");
                UpdateInfo($"Nom: {plc.sName}");
                UpdateInfo($"Gamme: {plc.sGamme}");
                UpdateInfo($"IP 1: {plc.sInterfaceX1}");
                UpdateInfo($"IP 2: {plc.sInterfaceX2}");
            }
            UpdateInfo("-");
            // Export DATA To Excel
            ExportDataToExcel();
        }

        /// <summary>
        /// Exports project and PLC device data to the loaded CDC Excel file.
        /// Writes project info and device details into the appropriate worksheets.
        /// </summary>
        void ExportDataToExcel()
        {
            UpdateInfo("Exportation des données vers Excel...");
            try
            {
                using (XLWorkbook wb = new XLWorkbook(sCdcFilePath))
                {
                    // Project Info
                    var ws = wb.Worksheet("PROJECT");
                    var range = ws.RangeUsed();

                    ws.Cell(2, 1).Value = oTiaProject.sName;
                    ws.Cell(2, 2).Value = oTiaProject.sProjectPath;
                    ws.Cell(2, 3).Value = oTiaProject.sVersion;
                    ws.Cell(2, 4).Value = oTiaProject.sDateCreation;
                    ws.Cell(2, 5).Value = oTiaProject.sLanguage;
                    ws.Cell(2, 6).Value = oTiaProject.sSize;
                    ws.Cell(2, 7).Value = oTiaProject.sSimulation;
                    ws.Cell(2, 8).Value = oTiaProject.oAutomates.Count;
                    ws.Cell(2, 9).Value = oTiaProject.oHMIs.Count;
                    ws.Cell(2, 10).Value = oTiaProject.oSCADAs.Count;

                    // PLC Info
                    int currentRow = 2;
                    ws = wb.Worksheet("PLC");
                    range = ws.RangeUsed();

                    var wsv = wb.Worksheet("VARIATEUR");
                    int currentRowV = 2;

                    for (int i = 2; i <= range.LastRowUsed().RowNumber(); i++)
                    {
                        ws.Row(i).Clear();
                    }

                    foreach (MyAutomate plc in oTiaProject.oAutomates)
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
                        
                        if (plc.oVariators != null) 
                        {
                            foreach (MyVariator variator in plc.oVariators)
                            {
                                wsv.Cell(currentRowV, 1).Value = variator.sName;
                                wsv.Cell(currentRowV, 2).Value = variator.sReference;
                                wsv.Cell(currentRowV, 3).Value = variator.sGamme;
                                wsv.Cell(currentRowV, 4).Value = variator.sInterfaceX1;
                                wsv.Cell(currentRowV, 5).Value = variator.sVlanX1;
                                wsv.Cell(currentRowV, 6).Value = variator.sMasterName;

                                currentRowV++;
                            }
                        }
                        
                        currentRow++;
                    }

                    ws = wb.Worksheet("SWITCH");
                    currentRow = 2;

                    foreach (MySwitch swi in oTiaProject.oSwitchs)
                    {
                        ws.Cell(currentRow, 1).Value = swi.sName;
                        ws.Cell(currentRow, 2).Value = swi.sGamme;
                        ws.Cell(currentRow, 3).Value = swi.sReference;
                        ws.Cell(currentRow, 4).Value = swi.sFirmware;
                        ws.Cell(currentRow, 5).Value = swi.sInterfaceX1;
                        ws.Cell(currentRow, 6).Value = swi.sVlanX1;
                        currentRow++;
                    }

                    ws = wb.Worksheet("IHM_SCADA");
                    currentRow = 2;

                    foreach(MyHmi hmi in oTiaProject.oHMIs)
                    {
                        ws.Cell(currentRow, 1).Value = "Hmi";
                        ws.Cell(currentRow, 2).Value = hmi.sName;
                        ws.Cell(currentRow, 3).Value = hmi.sReference;
                        ws.Cell(currentRow, 4).Value = hmi.sFirmware;
                        ws.Cell(currentRow, 5).Value = hmi.sInterfaceX1;
                        ws.Cell(currentRow, 6).Value = hmi.sVlanX1;
                        ws.Cell(currentRow, 7).Value = hmi.sConnectedDeviceX1;
                        ws.Cell(currentRow, 8).Value = hmi.sInterfaceX2;
                        ws.Cell(currentRow, 9).Value = hmi.sVlanX2;
                        ws.Cell(currentRow, 10).Value = hmi.sConnectedDeviceX2;
                        currentRow++;
                    }

                    foreach(MyScada scada in oTiaProject.oSCADAs)
                    {
                        ws.Cell(currentRow, 1).Value = "Scada";
                        ws.Cell(currentRow, 2).Value = scada.sName;
                        ws.Cell(currentRow, 3).Value = scada.sReference;
                        ws.Cell(currentRow, 4).Value = scada.sFirmware;
                        ws.Cell(currentRow, 5).Value = scada.sInterfaceX1;
                        ws.Cell(currentRow, 6).Value = scada.sVlanX1;
                        ws.Cell(currentRow, 7).Value = scada.sConnectedDeviceX1;
                        ws.Cell(currentRow, 8).Value = scada.sInterfaceX2;
                        ws.Cell(currentRow, 9).Value = scada.sVlanX2;
                        ws.Cell(currentRow, 10).Value = scada.sConnectedDeviceX2;
                        currentRow++;
                    }

                    ws = wb.Worksheet("VLAN");
                    currentRow = 2;
                    int j = 0;

                    foreach (MyConnexion connexion in oTiaProject.oConnexions)
                    {
                        j = 2;
                        ws.Cell(currentRow, 1).Value = connexion.sName;
                        foreach(string sDevice in connexion.oConnectedDevices)
                        {
                            ws.Cell(currentRow, j).Value = sDevice;
                            j++;
                        }

                        currentRow++;
                    }

                    wb.Save();
                }
                UpdateInfo("Exportation des données vers Excel terminée");
                BpDownloadFile.Enabled = true;
            }
            catch (Exception ex)
            {
                UpdateInfo($"Erreur lors de l'exportation des données vers Excel : {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the column titles for the PLC worksheet in the provided Excel workbook.
        /// </summary>
        /// <param name="wb">The Excel workbook.</param>
        /// <param name="sheet">The sheet index for the PLC worksheet.</param>
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

        /// <summary>
        /// Handles the "Download" button click event.
        /// Copy the result Excel file to a user-defined location.
        /// </summary>
        private void BpDownloadFile_Click(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Enregistrer le fichier sous";
                saveFileDialog.Filter = "Fichiers Excel Macro (*.xlsm)|*.xlsm|Tous les fichiers (*.*)|*.*";
                saveFileDialog.FileName = "Export_" + oTiaProject.sName + "_" + $"{now.Day}{now.Month:D2}{now.Year:D2}";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string destinationPath = saveFileDialog.FileName;
                    string sourcePath = sCdcFilePath;

                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        UpdateInfo("Fichier enregistré avec succès.");
                    }
                    catch (Exception ex)
                    {
                        UpdateInfo($"Erreur lors de l'enregistrement : {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Adds an informational message to the information RichTextBox and scrolls to the latest message.
        /// </summary>
        /// <param name="sMessage">The message to display. Use "-" for a section separator.</param>
        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now;
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "----------------------------------------------------------------------------------------------------------------------------------------------\n";
            }
            else
            {
                sFullMessage = $"{sCurrentDateTime} {sMessage}\n";
            }
            // Add the message to the RichTextBox
            txBInformations.AppendText(sFullMessage);

            // Auto-scroll to the bottom
            txBInformations.SelectionStart = txBInformations.Text.Length;
            txBInformations.ScrollToCaret();
        }
    }
}