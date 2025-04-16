using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ClosedXML.Excel;
using Siemens.Engineering.HW;
using System.Collections.ObjectModel;
using Common;

namespace AideAuDiagnostic
{
    public partial class AideAuDiagnostic : UserControl
    {
        #region VARIABLES
        // Debugview activation trace
        private bool bWithDebugView = false;
        public bool GetWithDebugView { get { return bWithDebugView; } }

        // Indication démarrage de l'application
        private bool bApplicationIsStarted = false;

        // Objet thread de démarrage
        private Thread oThreadStartApplication;

        // Paramètres généraux pour l'application
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();

        // Objet evenement de notification
        public event EventHandler oTaskEventEndThreadStartApplication;

        // Objet interface Tia Portal
        private ExploreTiaPLC oExploreTiaPLC;

        // Objet de définition de la CPU 
        private TiaProjectForCPU oTiaProjectForCPU;

        // Objet de génération du code pour les automates gateway
        private PlcGenerateTiaCode oPlcGenerateTiaCode;

        // Objet evenement de notification de fin de génération
        public event EventHandler oTaskEventEndThreadGenerationCode;

        // Indication démarrage generation de code
        private bool bCodeGenerationIsStarted = false;

        // Indication erreur lors de la génération
        private readonly bool bErrorWhenGeneratedCode = false;

        // Objet thread de génération
        private Thread oThreadStartGeneratePLC;

        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();

        //Liste des instructions pour le FC
        private readonly List<string> lsDataCollection = new List<string>();
        #endregion
        public AideAuDiagnostic()
        {
            InitializeComponent();
        }

        #region DÉMARAGE APPLICATION
        //**************************  Chargement de la vue  **************************//
        private void AideAuDiagnostic_Load(object sender, EventArgs e)
        {
            var tStartTime = DateTime.Now;

            // Au démarage de l'application
            UpdateInfo("Démarrage de l'application...");
            // Lancement du processus de l'application
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

        private void OnTIAVersionChanged(object sender, string newVersion)
        {
            // Utiliser Invoke pour s'assurer que la mise à jour se fait sur le thread UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UnloadControl()));
            }
            else
            {
                UnloadControl();
            }
        }

        private void UnloadControl()
        {
            // Si le contrôle est dans un parent, le retirer de la collection de contrôles
            if (this.Parent != null)
            {
                this.Parent.Controls.Remove(this);
            }

            // Détacher les événements
            TIAVersionManager.VersionChanged -= OnTIAVersionChanged;

            // Libérer les ressources
            this.Dispose();
        }
        //**************************  Traitement de fin de thread  **************************//
        private void ThreadConnectionIsFinishedTask(object sender, EventArgs arg)
        {
            bApplicationIsStarted = true;
        }
        //**************************  Procedure au démarrage  **************************//
        private void StartApplication()
        {
            string sError = string.Empty;

            // Lecture du fichier ini de configuration
            TraceThreadUIPrincipal(@"Lecture du fichier de configuration...");
            ReadIniFile();
            //LECTURE DU FICHIER EXCEL A L'INIT DE L'APP
            ReadExcel();
            TraceThreadUIPrincipal(@"Lecture du fichier excel : OK");
            // Initialisation de tous les objets internes
            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oPlcGenerateTiaCode = new PlcGenerateTiaCode(oExploreTiaPLC, lsDataCollection);

            BeginInvoke(new MethodInvoker(() => bPSelectStation.Enabled = true));
            BeginInvoke(new MethodInvoker(() => GIFChargement.Visible = false));

            // S'abonner à l'événement de changement de version
            TIAVersionManager.VersionChanged += OnTIAVersionChanged;

            // Si le contrôle est déchargé, se désabonner de l'événement
            this.Disposed += (s, e) => TIAVersionManager.VersionChanged -= OnTIAVersionChanged;
        }
        //**************************  Gestion de l'ecriture Thread secondaire  **************************//
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
        //**************************  Lecture du fichier Ini  **************************//
        private void ReadIniFile()
        {
            string sTemp;
            IniFile oIniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + @"\Using_Files\HMAOPCUAH.ini");

            // Lecture du chemin d'installation de l'application
            oPLC_ProjectDefinitions.sPathApplication = AppDomain.CurrentDomain.BaseDirectory;

            // Lecture si debugview est utilisé pour les traces
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

            //*****************************************
            // Read Tia portal settings
            //*****************************************

            // Read if tia Portal interface is visible
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
            // Read TIA Poral openness assemblies path
            oPLC_ProjectDefinitions.SetOpennessLibraryPath(oIniFile.Read("FolderOpennessAssemblies", "TIAPORTAL"));
            // Read Extension project name Tia Portal
            oPLC_ProjectDefinitions.sExtensionProjectName = oIniFile.Read("ExtensionProjectName", "TIAPORTAL");

            //*****************************************
            // Read Project definitions
            //*****************************************
            // Read Mark OPC UA search string for Family Bloc
            oPLC_ProjectDefinitions.sFamilyBlocMarck = oIniFile.Read("FamilyBlocMarckDef", "Project_Definitions");
            // Read String parameter define new bloc name
            oPLC_ProjectDefinitions.sFamillyStrResearchNewBlocName = oIniFile.Read("FamillyStrResearchNewBlocNameDef", "Project_Definitions");
            // Read Mark OPC UA search string for Commentar Bloc Parameter
            oPLC_ProjectDefinitions.sCommentarBlocParameterMarck = oIniFile.Read("CommentarBlocParameterMarckDef", "Project_Definitions");
            // Read Mark OPC UA search string for Commentar Tag variable
            oPLC_ProjectDefinitions.sCommentarTagVariableMarck = oIniFile.Read("CommentarTagVariableMarckDef", "Project_Definitions");

            //*****************************************
            // Read OPCUA_Server Specification
            //*****************************************
            // Read Root Folder for Blocks in OPC UA server
            oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer = oIniFile.Read("RootFolderBlocOPCUAServer", "OPCUA_Server_Specification");
            // Read Root Folder for Tags in OPC UA server
            oPLC_ProjectDefinitions.sRootFolderTagsOPCUAServer = oIniFile.Read("RootFolderTagsOPCUAServer", "OPCUA_Server_Specification");

            //*****************************************
            // Read DB OPC UA mapping
            //*****************************************
            // Read DB OPC UA mapping name
            oPLC_ProjectDefinitions.sDBNameMappingOPCUA = oIniFile.Read("DBNameMappingOPCU", "OPCUA_Server_Specification");

            //*****************************************
            // Read Namespace for OPC UA Server
            //*****************************************
            // Read namespace OPC UA server
            oPLC_ProjectDefinitions.sOPCUAServerNamespace = oIniFile.Read("OPCUAServerNamespace", "OPCUA_Server_Specification");

            //*************************************************************
            // Read flag to validate new name familly only for node id tag
            //*************************************************************
            // Read flag to validate new name familly only for node id tag
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


            //****************************************************
            // Read Credentials informations for Tia Portal access
            //****************************************************
            // Read Username to access Tia Portal project
            oPLC_ProjectDefinitions.SetUserName(oIniFile.Read("UserName", "Credentials"));
            // Read Crypt password for user to access Tia portal project
            oPLC_ProjectDefinitions.SetUncryptPasswordUser(oIniFile.Read("Password", "Credentials"));
        }
        //**************************  Lecture du fichier Excel  **************************//
        private void ReadExcel()
        {

            //Chemin dans l'application en cours d'exe
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
                            if (ws.Cell(i, 2).IsEmpty())
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
        #region SÉLECTION DE LA CIBLE
        //**************************  Méthode appuie sur le bouton Select  **************************//
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
        //**************************  Gestion de la selection de la cible  **************************//
        private bool GetS71500_Station(ref string sStationName)
        {
            bool bRet = true;
            string sError = string.Empty;

            sStationName = string.Empty;
            
            while (true)
            {
                // Test si le projet Tia Portal est déja sélectionné ?
                if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
                {
                    // Sélection du projet Tia Portal
                    if (oExploreTiaPLC.ChooseTiaProject(ref sError) == false)
                    {
                        bRet = false;
                        break;
                    }
                }
                // Récupération de la station S7-1500 dans le projet sélectionné
                if (oExploreTiaPLC.GetTiaStationFromProjectList(ref sStationName, ref sError) == false)
                {
                    bRet = false;
                }
                break;
            }
            // Affichage du nom du projet selectionné
            txBProjet.Text = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
            return bRet;
        }
        #endregion
        #region GÉNÉRATION DU CODE
        //**************************  Méthode appuie sur le bouton Génération  **************************//
        private void bPExport_Click(object sender, EventArgs e)
        {
            bool projetSelectionner = oExploreTiaPLC.bTiaPortalProjectIsSelected;

            UpdateInfo("-");
            var tStartTime = DateTime.Now;

            if (projetSelectionner)
            {
                TraceThreadUIPrincipal(@"Demarrage de la recherche des Blocs FC pour export et analyse");

                // Lancemement de la lecture des blocs FC  et import du FC dans la CPU  
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
        //**************************  Gestion de la génération du code  **************************//
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
        //**************************  Attente de la fin de la génération du code  **************************//
        private void WaitForEndCodeGeneration()
        {
            while (bCodeGenerationIsStarted == false)
            {
                Application.DoEvents();
            }
        }
        //**************************  Génération du code  **************************//
        private void GeneratePLCCodeMain()
        {
            string sErrorText = string.Empty;
            try
            {
                while (true)
                {
                    // Avant le lancement de la génération, il est necessaire de vérifier que le programme dans la Cpu
                    // est bien compilé sans erreur
                    
                    // Lancement de la compilation du S7-1500
                    TraceThreadUIPrincipal(@"Compilation de la CPU avant generation...");
                    if (oPlcGenerateTiaCode.CompileThisPlcAndCheckErrors(oPLC_ProjectDefinitions.sPLCS71500HTargetStationName, ref sErrorText) == false)
                    {
                        TraceThreadUIPrincipal(string.Format(@"Erreur à la compilation de la CPU : {0}", sErrorText));
                        break;
                    }
                    TraceThreadUIPrincipal(@"La CPU est bien compilée sans erreur");
                    
                    // Lancement de la lecture + Export du code de la CPU S7-1500 
                    if (StartExamineS71500PLCCode(ref sErrorText) == false)
                    {
                        TraceThreadUIPrincipal(string.Format(@"Erreur à la lecture du code de la CPU : {0}", sErrorText));
                        break;
                    }
                    TraceThreadUIPrincipal(@"L'export et l'analyse du programme CPU s'est bien déroulé");

                    // Lancement de la génération du code dans la CPU S7-1500 
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
                // Indication de fin du thread
                this.BeginInvoke(oTaskEventEndThreadGenerationCode, EventArgs.Empty);
            }
        }
        //**************************  Gestion de la fin de la génération du code  **************************//
        private void ThreadGenerationCodeIsFinishedTask(object sender, EventArgs arg)
        {
            bCodeGenerationIsStarted = true;
        }
        //**************************  Examen du code de la CPU S7-1500  **************************//
        private bool StartExamineS71500PLCCode(ref string sErrorText)
        {
            bool bRet;
            sErrorText = string.Empty;
            oTiaProjectForCPU = new TiaProjectForCPU(oExploreTiaPLC.GetTiainterface().GetTIAPortalProject(),
                                                        txBStation.Text, oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer,
                                                        oPLC_ProjectDefinitions.sRootFolderBlocOPCUAServer, 
                                                        oPLC_ProjectDefinitions.sDBNameMappingOPCUA);
            bRet = oExploreTiaPLC.EnumerateFoldersBlocksParametersAndTagsForOPCUA(ref oTiaProjectForCPU, txBStation.Text, ref sErrorText);
            // Affectation des informations de blocs et tags présents dans le projet TIA du S7-1500
            oPlcGenerateTiaCode.SetTiaProjectForCPU(oTiaProjectForCPU);
            return bRet;
        }
        //**************************  Génération du code dans la CPU S7-1500  **************************//
        private bool StartGeneratePLCProgramInPLCS71500(ref string serrorText)
        {
            bool bRet;
            string sErrorInterne = string.Empty;
            int iCmptEntree = 0;

            // Lancement de la génération du code dans la CPU
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
        #region InformationsBox
        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now; // Actual date and time
            string sCurrentDateTime = $"{ now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
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
        private void bPMaximizeInfo_Click(object sender, EventArgs e)
        {
            //Maximize Information Group Box
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

        private void bPMinimize_Click(object sender, EventArgs e)
        {
            //Minimize Information Group Box
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

        /* 
        // Test Chargement dynamique des références
       private void InitializeAssemblies()
       {
           try
           {
               // Utilisation de la bibliothèque partagée
               Type targetType = SharedLibrary.AssemblyManager.GetType("Siemens.Automation.YourTypeName");
               if (targetType != null)
               {
                   // Utilisez le type comme nécessaire
                   object instance = Activator.CreateInstance(targetType);
                   // ... autres opérations
               }
           }
           catch (Exception ex)
           {
               MessageBox.Show($"Erreur d'initialisation: {ex.Message}");
           }
       }
       */
    }
}
