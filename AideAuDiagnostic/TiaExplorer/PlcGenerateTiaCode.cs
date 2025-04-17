using GlobalsOPCUA;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AideAuDiagnostic.TiaExplorer
{


    internal class PlcGenerateTiaCode
    {
        #region CONSTANTES
        const string sRootOPCUAFolder = @"AIDE_AU_DIAGNOSTIC";
        const string sFCName_Receive_From_Gateway_1 = @"FC_Aide_au_diagnostic";
        const string sFCTitle_Receive_From_Gateway_1 = @"Update memory map from PLC Gateway 1";
        #endregion
        #region VARIABLES
        // Référence su l'objet d'exploration du projet TIA Portal
        ExploreTiaPLC oExploreTiaPLC;
        // Référence sur les informations programme du S7-1500H
        private TiaProjectForCPU oTiaProjectForCPU;
        public void SetTiaProjectForCPU(TiaProjectForCPU oTiaProjectForCPU) { this.oTiaProjectForCPU = oTiaProjectForCPU; }
        //Infos pour le FC
        private List<string> lsDataCollection;
        #endregion
        //Constructeur de la classe
        public PlcGenerateTiaCode(ExploreTiaPLC oExploreTiaPLC, List<string> lsDataCollection)
        {
            this.oExploreTiaPLC = oExploreTiaPLC;
            //FC
            this.lsDataCollection = lsDataCollection;
        }
        //Méthode pour compiler le programme de la station
        public bool CompileThisPlcAndCheckErrors(string sStationName, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            PlcSoftware oThisPlc = null;
            Device oStationDevice = null;
            DeviceItem oStationDeviceItem = null;
            bool bOneErrorCompilation = false;

            while (true)
            {
                // Recherche de la référence de la station passée en paramètre
                oThisPlc = GetThisStationByName(sStationName, ref oStationDevice, ref oStationDeviceItem, ref sErrorText);
                if (oThisPlc == null)
                {
                    sErrorText = string.Format(@"Station with name {0} not found in TIA Project", sStationName);
                    bRet = false;
                    break;
                }
                // Compilation complète de la CPU
                if (CompileStationAndGetErrors(oThisPlc, ref bOneErrorCompilation, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                break;
            }

            return bRet;
        }
        //Méthode pour récupérer la référence de la station
        private PlcSoftware GetThisStationByName(string sStationName, ref Device oStationDevice, ref DeviceItem oStationDeviceItem, ref string sErrorText)
        {
            PlcSoftware oThisPlc = null;
            List<Device> ldListDevice = new List<Device>();
            Device oPLCHStation = null;

            while (true)
            {
                try
                {
                    // Récupération de l'objet station correspondant à l'automate de la station
                    // Lancement de l'énumération des stations dans le projet
                    if (oExploreTiaPLC.GetTiainterface().EnumerationDevice(ref ldListDevice, ref sErrorText) == false)
                    {
                        break;
                    }

                    // Recherche de la station 
                    foreach (Device oDevice in ldListDevice)
                    {
                        if (oDevice.Name == sStationName)
                        {
                            oPLCHStation = oDevice;
                            oStationDevice = oDevice;
                            break;
                        }
                    }
                    // Test si la station a été trouvée ?
                    if (oPLCHStation == null)
                    {
                        sErrorText = string.Format(@"Station '{0}' for Plc Gateway not found !", sStationName);
                        break;
                    }

                    // Récupération du controllertarget
                    DeviceItem oDeviceItemToGetService = oExploreTiaPLC.TryToFoundDeviceItemInDevice(oPLCHStation);
                    if (oDeviceItemToGetService == null)
                    {
                        sErrorText = @"Impossible to found PLC device item in device";
                        oThisPlc = null;
                        break;
                    }
                    oStationDeviceItem = oDeviceItemToGetService;
                    SoftwareContainer oSoftwareContainer = oDeviceItemToGetService.GetService<SoftwareContainer>() as SoftwareContainer;
                    oThisPlc = oSoftwareContainer.Software as PlcSoftware;
                    if (oThisPlc == null)
                    {
                        sErrorText = @"controllertarget is null in this device";
                        break;
                    }
                }
                catch (Exception e)
                {
                    oPLCHStation = null;
                    sErrorText = string.Format(@"GetThisStationByName() Exception '{0}'", e.Message);
                }
                break;
            }

            return oThisPlc;
        }
        //Méthode pour compiler la station complète et de vérifier si pas d'erreur lors de la compilation
        bool CompileStationAndGetErrors(PlcSoftware oThisPlc, ref bool bAtLeastOneError, ref string sErrorText)
        {
            bool bRet = true;
            bAtLeastOneError = false;

            while (true)
            {
                try
                {
                    ICompilable iCompileService = oThisPlc.GetService<ICompilable>();
                    CompilerResult cResult = iCompileService.Compile();
                    if (cResult.ErrorCount != 0) bAtLeastOneError = true;
                }
                catch (Exception e)
                {
                    bRet = false;
                    bAtLeastOneError = true;
                    sErrorText = string.Format(@"CompileStation Exception '{0}'", e.Message);
                    break;
                }
                // Test si des erreurs de compilation
                if (bAtLeastOneError == true)
                {
                    sErrorText = @"Compilation errors found in station";
                    bRet = false;
                }
                break;
            }
            return bRet;
        }
        //Méthode pour générer le code TIA Portal
        public bool GenerateTiaCodeForS71500_R(string sStationName, ref int iCmptEntree, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            PlcSoftware oThisPlc = null;
            PlcBlockUserGroup oThisOPCUAFolder = null;
            Device oStationDevice = null;
            DeviceItem oStationDeviceItem = null;

            while (true)
            {
                //Recherche de la référence de la station passée en paramètre
                oThisPlc = GetThisStationByName(sStationName, ref oStationDevice, ref oStationDeviceItem, ref sErrorText);
                if (oThisPlc == null)
                {
                    bRet = false;
                    break;
                }

                //Test de la présence du folder BLK_JUMP et référence sur celui-ci
                oThisOPCUAFolder = GetOrCreateOPCUAFolder(oThisPlc, ref sErrorText);
                if (oThisOPCUAFolder == null)
                {
                    bRet = false;
                    break;
                }

                //Génération du bloc FC Test_BLK_JUMP
                if (MakeFC_Receive_From_Gateway(oThisPlc, oThisOPCUAFolder,
                                                sFCName_Receive_From_Gateway_1, sFCTitle_Receive_From_Gateway_1, ref iCmptEntree, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        //Permet de récupérer la réference sur le folder 
        //Le répertoire est créer s'il n'existe pas
        PlcBlockUserGroup GetOrCreateOPCUAFolder(PlcSoftware oThisPlc, ref string sErrorText)
        {
            PlcBlockUserGroup oGrpOPCUA = null;
            sErrorText = string.Empty;
            bool bRootFound = false;

            while (true)
            {
                try
                {
                    //Recherche du folder BLK_JUMP dans la racine du projet
                    foreach (PlcBlockUserGroup oBlockUserFolder in oThisPlc.BlockGroup.Groups)
                    {
                        //Test si le folder correspond a BLK_JUMP
                        if (oBlockUserFolder.Name.ToUpper() == sRootOPCUAFolder.ToUpper())
                        {
                            oGrpOPCUA = oBlockUserFolder;
                            bRootFound = true;
                            break;
                        }
                    }
                    //Si le folder n'est pas trouvé, on le crée
                    if (bRootFound == false)
                    {
                        oGrpOPCUA = oThisPlc.BlockGroup.Groups.Create(sRootOPCUAFolder);
                    }
                }
                catch (Exception e)
                {
                    oGrpOPCUA = null;
                    sErrorText = string.Format(@"GetOrCreateOPCUAFolder() Exception '{0}'", e.Message);
                }
                break;
            }
            return oGrpOPCUA;
        }
        //Méthode pour crée le bloc FC
        bool MakeFC_Receive_From_Gateway(PlcSoftware oThisPLC, PlcBlockUserGroup oThisPlcUserFolder,
                                         string sFC_Name_Receive_From_Gateway, string sFC_Title_Receive_From_Gateway, ref int iCmptEntree,
                                         ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            List<string> lsSourceFC = new List<string>();
            string sFCFileName = string.Empty;
            DateTime dtLocalDate = DateTime.Now;

            while (true)
            {
                //Création de l'entête du bloc FC
                lsSourceFC.AddRange(MakeFC_Header_For_Receive_From_PLC(sFC_Name_Receive_From_Gateway, sFC_Title_Receive_From_Gateway));

                //Ajout dans le corps de la fonction
                lsSourceFC.Add(@"// BLOC FONCTION D'AIDE AU DIAGNOSTIC");
                lsSourceFC.Add(@"//");
                lsSourceFC.Add(@"// Ce bloc permet de recepurer toutes les pattes d'entree se trouvant");
                lsSourceFC.Add(@"// dans le fichier excel : Liste_LTU_BackJump.xlsx");
                lsSourceFC.Add(@"//");
                lsSourceFC.Add(@"// DATE ET HEURE DE LA DERNIERE EXECUTION");
                lsSourceFC.Add(@"// DE LA FONCTION :" + dtLocalDate);
                lsSourceFC.Add(@"");
                lsSourceFC.Add(@"");

                //Test si tous les éléments commencent par un "/"
                bool bTest = false;
                foreach (string sElement in lsDataCollection)
                {
                    if (sElement.Substring(0, 1) != "/")
                    {
                        bTest = true;
                        break;
                    }
                }
                //Si aucun élément n'est trouver ou qu'un élément commence par un "/", on ajoute un commentaire
                if (lsDataCollection.Count == 0 || bTest == false)
                { 
                    lsSourceFC.Add(@"//");
                    lsSourceFC.Add(@"// AUCUNE SOURCE LTU DETECTEE");
                    lsSourceFC.Add(@"//");
                }
                else
                {
                    lsSourceFC.Add(@"IF (""FirstScan"" OR ""EnableBackJump"") THEN");
                    iCmptEntree = 0;
                    //Ecriture du résultat de la recherche
                    foreach (string sElement in lsDataCollection)
                    {
                        if (!sElement.Contains(":= '';"))
                        {
                            lsSourceFC.Add(sElement);
                            if(!sElement.Contains("//")) iCmptEntree++;
                        }
                    }
                    lsSourceFC.Add(@"END_IF;");
                }

                //Ajout de la fin du bloc FC
                lsSourceFC.AddRange(MakeFC_End());

                //Sauvegarde du FC dans un fichier
                if (CreateFCFile(sFC_Name_Receive_From_Gateway, lsSourceFC, ref sFCFileName, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                //Suppression du bloc FC dans le projet TIA Portal si déjà existant 
                if (DeleteBlocFCInTiaPortalProject(oThisPlcUserFolder, sFC_Name_Receive_From_Gateway, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                //Importation de la source du FC dans le projet TIA Portal et génération du bloc dans la bonne arobrescence
                if (ImportSourceDBAndGenerateItInTiaPortalProject(oThisPLC, oThisPlcUserFolder, sFC_Name_Receive_From_Gateway, sFCFileName, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                //Suppression du fichier FC
                if (File.Exists(sFCFileName) == true) File.Delete(sFCFileName);

                break;
            }
            return bRet;
        }
        //Méthode pour générer l'entête du bloc FC
        List<string> MakeFC_Header_For_Receive_From_PLC(string sFCName, string sFCTitle)
        {
            List<string> lsHeader = new List<string>();
            string sLine = string.Empty;

            sLine = string.Format(@"FUNCTION ""{0}"" : Void", sFCName);
            lsHeader.Add(sLine);
            sLine = string.Format(@"TITLE = {0}", sFCTitle);
            lsHeader.Add(sLine);
            lsHeader.Add(@"{ S7_Optimized_Access := 'TRUE' }");
            lsHeader.Add(@"AUTHOR: HMA");
            lsHeader.Add(@"FAMILY : Siemens");
            lsHeader.Add(@"VERSION : 0.1");
            lsHeader.Add(@"BEGIN");

            return lsHeader;
        }
        //Méthode pour générer la fin du bloc FC
        List<string> MakeFC_End()
        {
            List<string> lsEnd = new List<string>
            {
                @"END_FUNCTION"
            };

            return lsEnd;
        }
        //Méthode pour créer le fichier du bloc FC dans le répertoire de l'application
        bool CreateFCFile(string sBlocName, List<string> lsFileLines, ref string sFCFileName, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    //Formatage du nom du fichier
                    sFCFileName = string.Format(@"{0}{1}.scl", oExploreTiaPLC.GetTiaProjectDefinitions().sPathApplication, sBlocName);
                    //Effacement du fichier si déjà existant
                    if (File.Exists(sFCFileName) == true) File.Delete(sFCFileName);
                    //Sauvegarde du contenu du fichier
                    File.WriteAllLines(sFCFileName, lsFileLines.ToArray());

                    break;
                }
                catch (Exception e)
                {
                    sErrorText = string.Format(@"Exception in CreateFCFile() : {0}", e.Message);
                    bRet = false;
                    break;
                }
            }

            return bRet;
        }
        //Méthode pour supprimer le bloc FC dans le projet TIA Portal si présent
        bool DeleteBlocFCInTiaPortalProject(PlcBlockUserGroup oThisPlcUserFolder, string sBlocName, ref string sErrorText)
        {
            bool bRet = true;
            List<PlcBlock> loListBlocks = new List<PlcBlock>();

            while (true)
            {
                //Enumération des blocs dans le folder
                if (oExploreTiaPLC.GetTiainterface().EnumerateBlockUserProgramForThisFolder(oThisPlcUserFolder, ref loListBlocks, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                //Recherche du bloc dans la liste
                foreach (PlcBlock oBlock in loListBlocks)
                {
                    //Test si le bloc est un FC
                    if (oBlock is FC)
                    {
                        try
                        {
                            //Test si le bloc est le bloc recherché
                            if (oBlock.Name.ToUpper() == sBlocName.ToUpper())
                            {
                                //Suppression du bloc
                                oBlock.Delete();
                                break;
                            }
                        }
                        catch {; }
                    }
                }

                break;
            }
            return bRet;
        }
        //Méthode pour importer le bloc source et de le générer dans le bon répertoire cible
        bool ImportSourceDBAndGenerateItInTiaPortalProject(PlcSoftware oThisPLC, PlcBlockUserGroup oThisBlocFolder,
                                                           string sBlocName, string sDBFileName, ref string sErrorText)
        {
            bool bRet = true;
            PlcExternalSource oSource = null;

            while (true)
            {
                try
                {
                    //Incorporation du fichier source dans le projet TIA Portal
                    PlcExternalSourceSystemGroup oSystemeSourceFolder = oThisPLC.ExternalSourceGroup as PlcExternalSourceSystemGroup;
                    //Recherche si la source est déjà présente
                    foreach (PlcExternalSource oSourceRead in oSystemeSourceFolder.ExternalSources)
                    {
                        if (oSourceRead.Name.ToUpper() == sBlocName.ToUpper())
                        {
                            oSourceRead.Delete();
                            break;
                        }
                    }
                    //Insere la source à partir du fichier
                    if (oExploreTiaPLC.GetTiainterface().ImportSourceFileToSourceFolder(oSystemeSourceFolder, sBlocName, sDBFileName, ref oSource, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    //Génération du bloc dans importé
                    if (oExploreTiaPLC.GetTiainterface().GenerateBlocFromSourceFile(oSource, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    //Supression de la source
                    oSource.Delete();
                    //Déplacement du bloc nouvellement créé dans le folder cible
                    if (MoveBlocFromRootFolderToSpecificFolder(oThisPLC, oThisBlocFolder, sBlocName, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                }
                catch (Exception e)
                {
                    sErrorText = string.Format(@"Exception in ImportSourceDBAndGenerateItInTiaPortalProject() : {0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        //Méthode pour déplacer un bloc du répertoire root vers un répertoire spécifique
        bool MoveBlocFromRootFolderToSpecificFolder(PlcSoftware oThisPLC, PlcBlockUserGroup oThisBlocFolder, string sBlocName, ref string sErrorText)
        {
            bool bRet = true;
            PlcBlock oBlocToMove = null;
            string sXmlBlocToMove = string.Empty;
            PlcBlock oBlocImport = null;

            while (true)
            {
                try
                {
                    //Recherche du bloc dans le répertoire root
                    oBlocToMove = FindAnBlocInSpecificFolder(true, oThisPLC, oThisBlocFolder, sBlocName);
                    if (oBlocToMove == null)
                    {
                        sErrorText = string.Format(@"MoveBlocFromRootFolderToSpecificFolder() : Bloc {0} not found in root folder", sBlocName);
                        bRet = false;
                        break;

                    }
                    //Avant de lancer l'export du bloc, il faut le compiler
                    if(oBlocToMove.IsConsistent == false)
                    {
                        if(oExploreTiaPLC.GetTiainterface().CompileBloc(oBlocToMove, ref sErrorText) == false)
                        {
                            bRet = false;
                            break;
                        }
                    }
                    //Importation du bloc en XML pour ke reimporter au bon emplacement
                    sXmlBlocToMove = string.Format(@"{0}BlocToMove.XML", oExploreTiaPLC.GetTiaProjectDefinitions().sPathApplication);
                    
                    //Ajout du bloc dans la cible
                    if(oExploreTiaPLC.GetTiainterface().ExportBlocToXml(oBlocToMove, sXmlBlocToMove, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    //On supprime le bloc de sa source
                    oBlocToMove.Delete();

                    //On importe le bloc dans son emplacement définitif
                    if(oExploreTiaPLC.GetTiainterface().ImportBlocFromXml(oThisBlocFolder.Blocks, sXmlBlocToMove, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    //On supprime le fichier XML
                    if(File.Exists(sXmlBlocToMove) == true) File.Delete(sXmlBlocToMove);

                    //Après importation, on recompile le bloc ainsi importé
                    //Recherche du bloc importé dans le répertoire cible
                    oBlocImport = FindAnBlocInSpecificFolder(false, oThisPLC, oThisBlocFolder, sBlocName);
                    if (oBlocImport == null)
                    {
                        sErrorText = string.Format(@"MoveBlocFromRootFolderToSpecificFolder() : Bloc {0} not found in target folder", sBlocName);
                        bRet = false;
                        break;
                    }
                    //On lance la compilation du bloc après importation
                    if(oBlocImport.IsConsistent == false)
                    {
                        if (oExploreTiaPLC.GetTiainterface().CompileBloc(oBlocImport, ref sErrorText) == false)
                        {
                            bRet = false;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    sErrorText = string.Format(@"Exception in MoveBlocFromRootFolderToSpecificFolder() : {0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        //Méthode pour rechercher un bloc spécifique dans un répertoire spécifique
        PlcBlock FindAnBlocInSpecificFolder(bool bInRoot, PlcSoftware oThisPLC, PlcBlockUserGroup oThisBlocFolder, string sBlocName)
        {
            PlcBlock oBlocToFound = null;

            //Recherche du bloc dans le répertoire root
            if (!bInRoot)
            {
                foreach (PlcBlock oBlock in oThisBlocFolder.Blocks)
                {
                    if (oBlock.Name.ToUpper() == sBlocName.ToUpper())
                    {
                        //Le bon bloc à été trouver
                        oBlocToFound = oBlock;
                        break;
                    }
                }
            }
            //Recherche du bloc dans le répertoire spécifique
            else
            {
                foreach (PlcBlock oBloc in oThisPLC.BlockGroup.Blocks)
                {
                    if (oBloc.Name.ToUpper() == sBlocName.ToUpper())
                    {
                        //Le bon bloc à été trouver
                        oBlocToFound = oBloc;
                        break;
                    }
                }
            }
            return oBlocToFound;
        }
    }
}

