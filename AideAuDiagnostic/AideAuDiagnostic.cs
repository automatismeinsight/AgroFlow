using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ClosedXML.Excel;
using Common;

namespace AideAuDiagnostic
{
    /// <summary>
    /// User control for the Diagnostic Assistance feature.
    /// Handles TIA project initialization, station selection, PLC code generation, and information display.
    /// </summary>
    public partial class AideAuDiagnostic : UserControl
    {
        #region VARIABLES

        /// <summary>
        /// Indicates whether DebugView tracing is enabled.
        /// </summary>
        private bool bWithDebugView = false;

        /// <summary>
        /// Gets whether DebugView tracing is enabled.
        /// </summary>
        public bool GetWithDebugView { get { return bWithDebugView; } }

        /// <summary>
        /// Indicates if the application has started.
        /// </summary>
        private bool bApplicationIsStarted = false;

        /// <summary>
        /// Thread object for application startup.
        /// </summary>
        private Thread oThreadStartApplication;

        /// <summary>
        /// General project definitions for the PLC.
        /// </summary>
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();

        /// <summary>
        /// Event triggered at the end of the application start thread.
        /// </summary>
        public event EventHandler oTaskEventEndThreadStartApplication;

        /// <summary>
        /// TIA Portal interface explorer object.
        /// </summary>
        private ExploreTiaPLC oExploreTiaPLC;

        /// <summary>
        /// CPU project definition object for TIA.
        /// </summary>
        private TiaProjectForCPU oTiaProjectForCPU;

        /// <summary>
        /// Code generation object for gateway PLCs.
        /// </summary>
        private PlcGenerateTiaCode oPlcGenerateTiaCode;

        /// <summary>
        /// Event triggered at the end of code generation thread.
        /// </summary>
        public event EventHandler oTaskEventEndThreadGenerationCode;

        /// <summary>
        /// Indicates if code generation has started.
        /// </summary>
        private bool bCodeGenerationIsStarted = false;

        /// <summary>
        /// Indicates if an error occurred during code generation.
        /// </summary>
        private readonly bool bErrorWhenGeneratedCode = false;

        /// <summary>
        /// Thread object for PLC code generation.
        /// </summary>
        private Thread oThreadStartGeneratePLC;

        /// <summary>
        /// Data dictionary for storing Excel or project data.
        /// </summary>
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();

        /// <summary>
        /// List of FC instructions for data collection.
        /// </summary>
        private readonly List<string> lsDataCollection = new List<string>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the AideAuDiagnostic user control.
        /// </summary>
        public AideAuDiagnostic()
        {
            InitializeComponent();
        }

        #region APPLICATION STARTUP

        /// <summary>
        /// Loads the diagnostic view and starts the initialization process.
        /// </summary>
        private void AideAuDiagnostic_Load(object sender, EventArgs e)
        {
            var tStartTime = DateTime.Now;

            UpdateInfo("Démarrage de l'application...");
            oTaskEventEndThreadStartApplication += ThreadConnectionIsFinishedTask;
            oThreadStartApplication = new Thread(() => StartApplication());
            try
            {
                bApplicationIsStarted = false;

                oThreadStartApplication.Start();

                while (bApplicationIsStarted == false)
                {
                    Application.DoEvents();
                }
            }
            finally
            {
                UpdateInfo("Fonction démarrer avec sccès");
                var tEndTime = DateTime.Now;
                UpdateInfo(string.Format("Temps de démarrage de l'application : {0} secondes", (tEndTime - tStartTime).TotalSeconds));
            }
        }

        /// <summary>
        /// Marks the application as started upon thread completion.
        /// </summary>
        private void ThreadConnectionIsFinishedTask(object sender, EventArgs arg)
        {
            bApplicationIsStarted = true;
        }

        /// <summary>
        /// Application startup procedure: reads configuration files, initializes objects, and prepares UI.
        /// </summary>
        private void StartApplication()
        {
            string sError = string.Empty;

            TraceThreadUIPrincipal(@"Lecture du fichier de configuration...");
            ReadIniFile();
            ReadExcel();
            TraceThreadUIPrincipal(@"Lecture du fichier excel : OK");
            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oPlcGenerateTiaCode = new PlcGenerateTiaCode(oExploreTiaPLC, lsDataCollection);

            BeginInvoke(new MethodInvoker(() => bPSelectStation.Enabled = true));
            BeginInvoke(new MethodInvoker(() => GIFChargement.Visible = false));

            TIAAssemblyLoader.SetupControl(this);
        }

        /// <summary>
        /// Thread-safe method for updating UI from background threads.
        /// </summary>
        /// <param name="sTrace">The message to display in the UI.</param>
        public void TraceThreadUIPrincipal(string sTrace)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(() => UpdateInfo(sTrace)));
            }
            else
            {
                UpdateInfo(sTrace);
            }
        }

        /// <summary>
        /// Reads configuration values from the INI file and populates project definitions.
        /// </summary>
        private void ReadIniFile()
        {
            string sTemp;
            IniFile oIniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\Using_Files\HMAOPCUAH.ini");

            oPLC_ProjectDefinitions.sPathApplication = AppDomain.CurrentDomain.BaseDirectory;

            sTemp = oIniFile.Read("WithDebugView", "GlobalParameters");
            try
            {
                if (sTemp.Length == 0) bWithDebugView = false;
                else
                {
                    int iValid = int.Parse(sTemp);
                    if (iValid > 0) bWithDebugView = true;
                }
            }
            catch
            {
                bWithDebugView = false;
            }

            sTemp = oIniFile.Read("WithUITiaPortal", "TIAPORTAL");
            try
            {
                if (sTemp.Length == 0) oPLC_ProjectDefinitions.bWithUITiaPortal = false;
                else
                {
                    int iValid = int.Parse(sTemp);
                    if (iValid > 0) oPLC_ProjectDefinitions.bWithUITiaPortal = true;
                }
            }
            catch
            {
                oPLC_ProjectDefinitions.bWithUITiaPortal = false;
            }
            oPLC_ProjectDefinitions.SetOpennessLibraryPath(oIniFile.Read("FolderOpennessAssemblies", "TIAPORTAL"));
            oPLC_ProjectDefinitions.sExtensionProjectName = oIniFile.Read("ExtensionProjectName", "TIAPORTAL");

            oPLC_ProjectDefinitions.sFamilyBlocMarck = oIniFile.Read("FamilyBlocMarckDef", "Project_Definitions");
            oPLC_ProjectDefinitions.sFamillyStrResearchNewBlocName = oIniFile.Read("FamillyStrResearchNewBlocNameDef", "Project_Definitions");
            oPLC_ProjectDefinitions.sCommentarBlocParameterMarck = oIniFile.Read("CommentarBlocParameterMarckDef", "Project_Definitions");
            oPLC_ProjectDefinitions.sCommentarTagVariableMarck = oIniFile.Read("CommentarTagVariableMarckDef", "Project_Definitions");

            oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer = oIniFile.Read("RootFolderBlocOPCUAServer", "OPCUA_Server_Specification");
            oPLC_ProjectDefinitions.sRootFolderTagsOPCUAServer = oIniFile.Read("RootFolderTagsOPCUAServer", "OPCUA_Server_Specification");

            oPLC_ProjectDefinitions.sDBNameMappingOPCUA = oIniFile.Read("DBNameMappingOPCU", "OPCUA_Server_Specification");
            oPLC_ProjectDefinitions.sOPCUAServerNamespace = oIniFile.Read("OPCUAServerNamespace", "OPCUA_Server_Specification");

            sTemp = oIniFile.Read("NewNameBlockOnlyForTagNodeId", "OPCUA_Server_Specification");
            try
            {
                if (sTemp.Length == 0) oPLC_ProjectDefinitions.bNewNameBlockOnlyForTagNodeId = false;
                else
                {
                    int iValid = int.Parse(sTemp);
                    if (iValid > 0) oPLC_ProjectDefinitions.bNewNameBlockOnlyForTagNodeId = true;
                }
            }
            catch
            {
                oPLC_ProjectDefinitions.bNewNameBlockOnlyForTagNodeId = false;
            }

            oPLC_ProjectDefinitions.SetUserName(oIniFile.Read("UserName", "Credentials"));
            oPLC_ProjectDefinitions.SetUncryptPasswordUser(oIniFile.Read("Password", "Credentials"));
        }

        /// <summary>
        /// Reads the Excel file with additional configuration or project data.
        /// </summary>
        private void ReadExcel()
        {
            string sFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Liste_LTU_BackJump.xlsx";

            using (XLWorkbook wb = new XLWorkbook(sFilePath))
            {
                var ws = wb.Worksheets.First();
                var range = ws.RangeUsed();

                for (int i = 1; i < range.RowCount() + 1; i++)
                {
                    for (int j = 1; j < range.ColumnCount() + 1; j++)
                    {
                        if (j == 1)
                        {
                            if (ws.Cell(i, 1).IsEmpty())
                            {
                                break;
                            }
                        }
                        dData.Add(new Tuple<int, int>(i, j), ws.Cell(i, j).Value);
                    }
                }
            }
        }
        #endregion

        #region TARGET SELECTION

        /// <summary>
        /// Handles the click event for station selection button.
        /// Selects a target S7-1500 station from the TIA project.
        /// </summary>
        private void bPSelectStation_Click(object sender, EventArgs e)
        {
            string sStation = string.Empty;

            UpdateInfo("-");

            if (GetS71500_Station(ref sStation) == true)
            {
                UpdateInfo(string.Format(@"La station cible : {0} est bien affectée", sStation));
                oPLC_ProjectDefinitions.sPLCS71500HTargetStationName = sStation;
                txBStation.Text = sStation;
            }
            else
            {
                UpdateInfo("La station cible n'est pas affectée");
                oPLC_ProjectDefinitions.sPLCS71500HTargetStationName = string.Empty;
                txBStation.Text = string.Empty;
            }
        }

        /// <summary>
        /// Performs the selection and validation of the S7-1500 target station from the TIA project.
        /// </summary>
        /// <param name="sStationName">The output station name if found.</param>
        /// <returns>True if station is successfully selected, otherwise false.</returns>
        private bool GetS71500_Station(ref string sStationName)
        {
            bool bRet = true;
            string sError = string.Empty;

            sStationName = string.Empty;

            while (true)
            {
                if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
                {
                    if (oExploreTiaPLC.ChooseTiaProject(ref sError) == false)
                    {
                        bRet = false;
                        break;
                    }
                }
                if (oExploreTiaPLC.GetTiaStationFromProjectList(ref sStationName, ref sError) == false)
                {
                    bRet = false;
                }
                break;
            }
            txBProjet.Text = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
            return bRet;
        }
        #endregion

        #region CODE GENERATION

        /// <summary>
        /// Handles the click event for the code generation (export) button.
        /// Starts the code export and analysis process.
        /// </summary>
        private void bPExport_Click(object sender, EventArgs e)
        {
            bool projetSelectionner = oExploreTiaPLC.bTiaPortalProjectIsSelected;

            UpdateInfo("-");
            var tStartTime = DateTime.Now;

            if (projetSelectionner)
            {
                TraceThreadUIPrincipal(@"Demarrage de la recherche des Blocs FC pour export et analyse");

                StartPLCCodeGenerator();
                TraceThreadUIPrincipal(@"Fin de l'import du bloc : FC Aide au diagnostic");

                var tEndTime = DateTime.Now;
                UpdateInfo(string.Format(@"Temps de génération du code : {0} secondes", (tEndTime - tStartTime).TotalSeconds));
            }
            else
            {
                TraceThreadUIPrincipal(@"ATTENTION : Veuillez selectionner un projet et une CPU avant de lancer l'execution du programme");
            }
        }

        /// <summary>
        /// Starts the PLC code generator in a separate thread and manages UI state.
        /// </summary>
        private void StartPLCCodeGenerator()
        {
            oTaskEventEndThreadGenerationCode += ThreadGenerationCodeIsFinishedTask;
            oThreadStartGeneratePLC = new Thread(() => WaitForEndCodeGeneration());
            try
            {
                bCodeGenerationIsStarted = false;
                oThreadStartGeneratePLC.Start();

                bPSelectStation.Enabled = false;
                bPExport.Enabled = false;
                BeginInvoke(new MethodInvoker(() => GIFChargement.Visible = true));
                Application.DoEvents();

                GeneratePLCCodeMain();

                while (bCodeGenerationIsStarted == false)
                {
                    Application.DoEvents();
                }
            }
            finally
            {
                bPSelectStation.Enabled = true;
                bPExport.Enabled = true;
                BeginInvoke(new MethodInvoker(() => GIFChargement.Visible = false));

                if (bErrorWhenGeneratedCode == true)
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Waits for the PLC code generation process to complete.
        /// </summary>
        private void WaitForEndCodeGeneration()
        {
            while (bCodeGenerationIsStarted == false)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Main PLC code generation workflow, including compilation, code reading, and export.
        /// </summary>
        private void GeneratePLCCodeMain()
        {
            string sErrorText = string.Empty;
            try
            {
                while (true)
                {
                    TraceThreadUIPrincipal(@"Compilation de la CPU avant generation...");
                    if (oPlcGenerateTiaCode.CompileThisPlcAndCheckErrors(oPLC_ProjectDefinitions.sPLCS71500HTargetStationName, ref sErrorText) == false)
                    {
                        TraceThreadUIPrincipal(string.Format(@"Erreur à la compilation de la CPU : {0}", sErrorText));
                        break;
                    }
                    TraceThreadUIPrincipal(@"La CPU est bien compilée sans erreur");

                    if (StartExamineS71500PLCCode(ref sErrorText) == false)
                    {
                        TraceThreadUIPrincipal(string.Format(@"Erreur à la lecture du code de la CPU : {0}", sErrorText));
                        break;
                    }
                    TraceThreadUIPrincipal(@"L'export et l'analyse du programme CPU s'est bien déroulé");

                    if (StartGeneratePLCProgramInPLCS71500(ref sErrorText) == false)
                    {
                        TraceThreadUIPrincipal(string.Format(@"Erreur à la génération de code dans la CPU : {0}", sErrorText));
                        break;
                    }
                    TraceThreadUIPrincipal(@"Le FC a été correctement généré dans la CPU");

                    break;
                }
            }
            finally
            {
                this.BeginInvoke(oTaskEventEndThreadGenerationCode, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the flag indicating code generation is complete.
        /// </summary>
        private void ThreadGenerationCodeIsFinishedTask(object sender, EventArgs arg)
        {
            bCodeGenerationIsStarted = true;
        }

        /// <summary>
        /// Reads and analyzes the S7-1500 CPU code structure and updates project definitions.
        /// </summary>
        /// <param name="sErrorText">Output error message if any.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool StartExamineS71500PLCCode(ref string sErrorText)
        {
            bool bRet;
            sErrorText = string.Empty;
            oTiaProjectForCPU = new TiaProjectForCPU(oExploreTiaPLC.GetTiainterface().GetTIAPortalProject(),
                                                        txBStation.Text, oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer,
                                                        oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer,
                                                        oPLC_ProjectDefinitions.sDBNameMappingOPCUA);
            bRet = oExploreTiaPLC.EnumerateFoldersBlocksParametersAndTagsForOPCUA(ref oTiaProjectForCPU, txBStation.Text, ref sErrorText);
            oPlcGenerateTiaCode.SetTiaProjectForCPU(oTiaProjectForCPU);
            return bRet;
        }

        /// <summary>
        /// Generates the new PLC program in the S7-1500 CPU.
        /// </summary>
        /// <param name="serrorText">Output error message if any.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool StartGeneratePLCProgramInPLCS71500(ref string serrorText)
        {
            bool bRet;
            string sErrorInterne = string.Empty;
            int iCmptEntree = 0;

            TraceThreadUIPrincipal(@"Lancement de la generation de code dans la CPU");

            while (true)
            {
                bRet = oPlcGenerateTiaCode.GenerateTiaCodeForS71500_R(oPLC_ProjectDefinitions.sPLCS71500HTargetStationName, ref iCmptEntree, ref sErrorInterne);
                if (bRet == false)
                {
                    serrorText = string.Format(@"Error station {0} : {1}", oPLC_ProjectDefinitions.sPLCS71500HTargetStationName, sErrorInterne);
                }

                UpdateInfo(string.Format(@"Nombre d'entreé générer : {0}", iCmptEntree));
                break;
            }
            return bRet;
        }
        #endregion

        #region INFORMATION DISPLAY

        /// <summary>
        /// Appends a message to the information box with a timestamp.
        /// </summary>
        /// <param name="sMessage">Message to display.</param>
        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now;
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
            txBInformations.AppendText(sFullMessage);

            txBInformations.SelectionStart = txBInformations.Text.Length;
            txBInformations.ScrollToCaret();
        }

        /// <summary>
        /// Maximizes the information group box for better visibility.
        /// </summary>
        private void bPMaximizeInfo_Click(object sender, EventArgs e)
        {
            gBTarget.Enabled = false;
            gBTarget.Visible = false;
            gBExport.Enabled = false;
            gBExport.Visible = false;

            gBInformations.Location = new Point(40, 23);
            gBInformations.Height = 415;
            gBInformations.Width = 1020;

            txBInformations.Width = 1000;
            txBInformations.Height = 350;

            bPMaximizeInfo.Visible = false;
            bPMaximizeInfo.Enabled = false;

            bPMinimizeInfo.Visible = true;
            bPMinimizeInfo.Enabled = true;

            txBInformations.Font = new Font("Microsoft Sans Serif", 16);
        }

        /// <summary>
        /// Minimizes the information group box to its default size.
        /// </summary>
        private void bPMinimize_Click(object sender, EventArgs e)
        {
            gBInformations.Location = new Point(725, 23);
            gBInformations.Height = 225;
            gBInformations.Width = 332;

            txBInformations.Width = 282;
            txBInformations.Height = 160;

            gBTarget.Enabled = true;
            gBTarget.Visible = true;
            gBExport.Enabled = true;
            gBExport.Visible = true;

            bPMinimizeInfo.Visible = false;
            bPMinimizeInfo.Enabled = false;

            bPMaximizeInfo.Visible = true;
            bPMaximizeInfo.Enabled = true;

            txBInformations.Font = new Font("Microsoft Sans Serif", 10);
        }

        #endregion

    }
}