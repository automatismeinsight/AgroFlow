using AideAuDiagnostic.TiaExplorer;
using OpennessV16;
using System.Collections.Generic;
using System.IO;
using GlobalsOPCUA;
using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks.Interface;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using System.Xml;
using System;
using System.Windows.Forms;
using System.IO.Packaging;
using Siemens.Engineering.Hmi.Tag;
using System.Linq;

namespace AideAuDiagnostic.TiaExplorer
{
    // Classe de traitement de l'exploration d'un projet TIA Portal
    public class ExploreTiaPLC
    {
        #region VARIABLES
        // Objet de définition du projet TIA Portal
        private readonly PLC_ProjectDefinitions oTiaProjectDefinitions;
        public PLC_ProjectDefinitions GetTiaProjectDefinitions() { return oTiaProjectDefinitions; }

        // Interface Openness sur Tia Portal
        public HMATIAOpenness_V16 oTiainterface = new HMATIAOpenness_V16();
        public HMATIAOpenness_V16 GetTiainterface() { return oTiainterface; }

        // Variable to store the selected TIA Portal project
        public bool bTiaPortalProjectIsSelected = false;
        public bool GetTiaPortalProjectIsSelected() { return bTiaPortalProjectIsSelected; }

        // Objet de traitement des fichiers Xml
        private readonly XmlDocument oXmlDocument;

        //Donnees du fichiers excel
        private readonly Dictionary<Tuple<int, int>, object> dData;

        //Infos pour le FC
        private readonly List<string> lsDataCollection;
        #endregion
        // Constructeur de la classe
        public ExploreTiaPLC(PLC_ProjectDefinitions oTiaProjectDefinitions, Dictionary<Tuple<int, int>, object> dData, List<string> lsDataCollection)
        {
            this.oTiaProjectDefinitions = oTiaProjectDefinitions;
            this.oXmlDocument = new XmlDocument();
            //Excel
            this.dData = dData;
            //FC
            this.lsDataCollection = lsDataCollection;

        }
        #region SÉLECTION DE LA CIBLE
        //Selection d'un projet TIA Portal
        public bool ChooseTiaProject(ref string sError)
        {
            bool bRet;
            string sinfo = string.Empty;
            bool bCriticalError = false;

            sError = string.Empty;

            TiaPortalProjectSelection oTiaSelection = new TiaPortalProjectSelection();

            // Récupération de la liste des instances de projets Tia Portal en cours
            List<TiaPortalProcess> loTiaPortalCurrentProcess = HMATIAOpennessCurrentInstance.GetCurrentTiaPortalInstance();

            // Boucle de traitement de la liste des Tia Portal en cours de traitement
            foreach (TiaPortalProcess tiaprocess in loTiaPortalCurrentProcess)
            {
                try
                {
                    oTiaSelection.dTiaProcessList.Add(Path.GetFileNameWithoutExtension(tiaprocess.ProjectPath.Name), new HMATiaPortalProcess(tiaprocess));
                    Console.WriteLine("Recuperation du nom ");
                }
                catch
                {
                    Console.WriteLine("Erreur Nom Projet");
                }
            }

            // Affichage de la boite de dialogue
            oTiaSelection.ShowDialog();

            // On teste si un projet a été sélectionné ?
            if (oTiaSelection.bOneProjectSelected == true)
            {
                // on teste quel type de projet on a choisi ?
                switch (oTiaSelection.iTiaProjectSelectType)
                {
                    // Cas d'un projet Tia Portal en cours
                    case TiaPortalProjectSelection.TiaProjectSelectionType.CurrentTiaProject:
                        if (oTiainterface.AttachTiaPortalInstance(oTiaSelection.GetSelectCurrentProject().GetTiaPortalProcess(), ref sinfo) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }
                        // On associe le projet Tia Portal
                        oTiainterface.SetTIAPortalProject(oTiaSelection.GetSelectCurrentProject().GetTiaPortalProcess().ProjectPath.FullName);
                        if (oTiainterface.OpenCurrentTIAProjectFromInstance(ref sinfo, ref bCriticalError) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }
                        break;
                    // Cas d'un nouveau projet Tia Portal
                    case TiaPortalProjectSelection.TiaProjectSelectionType.NewTiaProject:
                        oTiainterface = new HMATIAOpenness_V16(oTiaProjectDefinitions.bWithUITiaPortal, ref sinfo);
                        // Ouverture du projet avec le nouveau chemin
                        oTiainterface.SetTIAPortalProject(oTiaSelection.sNewTiaPortalSelectionPath);
                        if (oTiainterface.OpenTIAProject(oTiaProjectDefinitions.GetUserName(), oTiaProjectDefinitions.GetUncryptPasswordUser(), ref sinfo, ref bCriticalError) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }
                        break;
                }
                bTiaPortalProjectIsSelected = true;

                //FIN SELECTION/OUVERTURE DE PROJET TIA
                bRet = true;
            }
            else
            {
                sError = @"No Tia project selected";
                bRet = false;
            }
            return bRet;
        }
        // Method to get the TIA Station from the project list
        public bool GetTiaStationFromProjectList(ref string sStationName, ref string sError)
        {
            bool bRet = true;
            List<Device> loListDevice = new List<Device>();


            while (true)
            {
                sStationName = string.Empty;
                // Lancement de l'énumération des stations dans le projet
                if (oTiainterface.EnumerationDevice(ref loListDevice, ref sError) == false)
                {
                    bRet = false;
                    break;
                }

                // Affichage de la boite de sélection d'une station du projet
                TiaPortalStationSelection tiaportalstationselection = new TiaPortalStationSelection();

            // Boucle de traitement de la liste des devices en cours de traitement
            foreach (Device device in loListDevice)
                { 
                    tiaportalstationselection.dDictionnaryTiaStationList.Add(device.Name, new HMATiaPortalDevice(device));
                }

                // Affichage de la boite de dialogue
                tiaportalstationselection.ShowDialog();

                // Test si une station a été sélectionnée ?
                if (tiaportalstationselection.bOneStationSelected == true)
                {
                    sStationName = tiaportalstationselection.GetSelectCurrentStation().GetPortalDevice().Name;
                    bRet = true;
                }
                else
                {
                    bRet = false;
                    sError = @"No PLC station selected";
                }

                break;
            }

            return bRet;
        }

        #endregion
        #region LECTURE DU CODE DE LA CPU
        //Methode d'énumération des folders, blocks, tags et paramètres
        public bool EnumerateFoldersBlocksParametersAndTagsForOPCUA(ref TiaProjectForCPU oTiaProjectForCPU, string sStationNameForPLC, ref string sErrorText)
        {
            bool bRet = true;
            List<Device> loListDevice = new List<Device>();
            Device oStationPLC = null;
            PlcSoftware oControllerPLC = null;
            lsDataCollection.Clear();

            sErrorText = string.Empty;

            while (true)
            {
                // Récupération de l'objet station correspondant à l'automate 
                // Lancement de l'énumération des stations dans le projet
                if (oTiainterface.EnumerationDevice(ref loListDevice, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                // Recherche de la station 
                foreach (Device oDevice in loListDevice)
                {
                    Console.WriteLine("Nom du device : " + oDevice.Name);
                    if (oDevice.Name == sStationNameForPLC)
                    {
                        oStationPLC = oDevice;
                        break;
                    }
                }
                // Test si la station a été trouvée ?
                if (oStationPLC == null)
                {
                    sErrorText = string.Format(@"Station '{0}' for PLC not found !", sStationNameForPLC);
                    bRet = false;
                    break;
                }
                // Enumération de tous les blocs et paramètres avec le repère 
                if (EnumerateBlocksAndParameterOPCUAMarck(ref oTiaProjectForCPU, oStationPLC, ref oControllerPLC, ref sErrorText) == false)
                {
                    Console.WriteLine("Probleme Enum 1");
                    bRet = false;
                    break;
                }
                // Enumération de tous les tags avec repère
                Console.WriteLine("Réussite Enum 1");
                if (EnumerateTagsWithOPCUAMarck(ref oTiaProjectForCPU, oControllerPLC, ref sErrorText) == false)
                {
                    Console.WriteLine("Probleme Enum 2");
                    bRet = false;
                    break;
                }
                Console.WriteLine("Réussite Enum 2");
                // Enumération de tous les variables system de diagnostic de l'automate pour la remontée vers les passerelles
                if (EnumerateVariableSystemTags(ref oTiaProjectForCPU, ref sErrorText) == false)
                {
                    Console.WriteLine("Probleme Enum 3");
                    bRet = false;
                    break;
                }
                Console.WriteLine("Réussite Enum 3");
                break;
            }

            return bRet;
        }
        //Méthode d'énumération des blocs et paramètres
        bool EnumerateBlocksAndParameterOPCUAMarck(ref TiaProjectForCPU oTiaProjectForcCPU, Device oStationPLC, ref PlcSoftware oControllerPLC, ref string sErrorText)
        {
            bool bRet = true;
            PlcSoftware oControllertarget = null;

            while (true)
            {
                //Récupération du controllertarget
                DeviceItem oDeviceItemToGetService = TryToFoundDeviceItemInDevice(oStationPLC);
                if (oDeviceItemToGetService == null)
                {
                    sErrorText = @"Impossible to found PLC device item in device";
                    bRet = false;
                    break;
                }
                SoftwareContainer oSoftwareContainer = oDeviceItemToGetService.GetService<SoftwareContainer>() as SoftwareContainer;
                oControllertarget = oSoftwareContainer.Software as PlcSoftware;
                if (oControllertarget == null)
                {
                    sErrorText = @"controllertarget is null in this device";
                    bRet = false;
                    break;
                }

                //On affecte le PLC pour la fonction d'énumération des tags dans la méthode suivante
                oControllerPLC = oControllertarget;

                oTiaProjectForcCPU.sControllerPLCName = oControllerPLC.Name;

                try
                {
                    //Recherche de tous les blocs présents dans la racine du projet CPU
                    SearchAllBlocInRootProgramFolder(oControllertarget, ref oTiaProjectForcCPU, oTiaProjectForcCPU.GetRootNodefolderBlocks());

                    //Balayage de tous les folders de la racine et les sous folders et les blocs associés
                    SearchAllFolderAndBlocInRootProgramFolder(oControllertarget, ref oTiaProjectForcCPU, oTiaProjectForcCPU.GetRootNodefolderBlocks());
                }
                catch (Exception e)
                {
                    sErrorText = string.Format(@"Exception in EnumerateBlocksAndParameters() '{0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }

            return bRet;
        }
        //Recherche de l'item de type CPU dans le device
        public DeviceItem TryToFoundDeviceItemInDevice(Device oPLCHStation)
        {
            DeviceItem oDeviceitem = null;
            List<DeviceItem> loListDeviceItem = new List<DeviceItem>();
            string sError = string.Empty;
            SoftwareContainer oSoftwarecontainer;

            if (oTiainterface.EnumerationDeviceItems(oPLCHStation, ref loListDeviceItem, ref sError) == true)
            {
                // Boucle de recherche de l'item de type CPU
                foreach (DeviceItem item in loListDeviceItem)
                {
                    oSoftwarecontainer = item.GetService<SoftwareContainer>() as SoftwareContainer;
                    if (oSoftwarecontainer != null)
                    {
                        oDeviceitem = item;
                        break;
                    }
                }
            }

            return oDeviceitem;
        }
        //Recherche des blocs dans le folder en cours
        void SearchAllBlocInRootProgramFolder(PlcSoftware oControllerTarget, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            List<PlcBlock> loListBlocks = new List<PlcBlock>();
            string sErrorText = string.Empty;
            string sNewName = string.Empty;

            //Enumération de tous les blocs pour ce répertoire
            if (oTiainterface.EnumerateBlockUserProgramForPlc(oControllerTarget, ref loListBlocks, ref sErrorText) == true)
            {
                foreach (PlcBlock oBloc in loListBlocks)
                {
                    //Test si on trouve sur un bloc FC alors EXPORT
                    if (oBloc is FC)//DataBlock
                    {
                        TiaPortalBloc blocTIA = new TiaPortalBloc(oBloc.Name, oNodeSource, oTiaProjectForCPU.GetNextFolderVariableId(), sNewName);
                        //Enumération des listes des paramètres du bloc
                        ExportTiaBlockDBAndEnumerateOPCUAParameters(oControllerTarget, (oBloc as FC), blocTIA, ref oTiaProjectForCPU);
                    }
                }
            }

        }
        //Export du bloc et recherche les parametres
        bool ExportTiaBlockDBAndEnumerateOPCUAParameters(PlcSoftware oControllerTarget, FC oDB, TiaPortalBloc oBlocTIA, ref TiaProjectForCPU oTiaProjectForCPU)
        {
            bool bRet = true;
            string sErrorText = string.Empty;
            string sXmlDBFile = oTiaProjectDefinitions.sPathApplication + @"HMADBExport.Xml";

            while (true)
            {
                try
                {
                    //Test si le fichier existe pour l'effacer avant export
                    if (File.Exists(sXmlDBFile) == true) File.Delete(sXmlDBFile);
                    //Export du FC
                    bRet = oTiainterface.ExportBlocToXml(oDB, sXmlDBFile, ref sErrorText);
                    if (bRet == false)
                    {
                        break;
                    }
                    //Traitement du fichier XML
                    bRet = ParseTiaXmlDBFile(oControllerTarget, sXmlDBFile, oBlocTIA, ref oTiaProjectForCPU);
                }
                catch
                {
                    bRet = false;
                }
                break;
            }

            return bRet;
        }
        //Parser le fichier XML d'un FC exporté
        bool ParseTiaXmlDBFile(PlcSoftware oControllerTarget, string sXmlFile, TiaPortalBloc oBlocTIA, ref TiaProjectForCPU oTiaProjectForCPU)
        {
            bool bRet = true;
            string sCurrentPath = string.Empty;

            while (true)
            {
                //Test si le fichier existe
                if (File.Exists(sXmlFile) == false)
                {
                    bRet = false;
                    break;
                }
                //Ouverture du fichier XML
                oXmlDocument.Load(sXmlFile);

                //Recherche des noeuds
                foreach (XmlNode oNodeDocument in oXmlDocument.ChildNodes)
                {
                    if (oNodeDocument.Name == @"Document")
                    {
                        foreach (XmlNode oNode in oNodeDocument.ChildNodes)
                        {
                            if (oNode.Name == @"SW.Blocks.FC")
                            {
                                foreach (XmlNode oNodebloc in oNode.ChildNodes)
                                {
                                    if (oNodebloc.Name == @"AttributeList")
                                    {
                                        foreach (XmlNode oNodeAttributeList in oNodebloc.ChildNodes)
                                        {
                                            if (oNodeAttributeList.Name == @"Name")
                                            {
                                                //Nom du bloc FC dans lequel on se trouve
                                                lsDataCollection.Add("//" + oNodeAttributeList.InnerText.ToString());
                                            }
                                        }
                                    }
                                    if (oNodebloc.Name == @"ObjectList")
                                    {
                                        foreach (XmlNode oNodeobjectlist in oNodebloc.ChildNodes)
                                        {
                                            if (oNodeobjectlist.Name == @"SW.Blocks.CompileUnit")
                                            {
                                                //Enumeration de tous les membres du reseau
                                                EnumerateMembers(oNodeobjectlist, oControllerTarget, oTiaProjectForCPU, oBlocTIA, ref sCurrentPath, true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                break;
            }
            return bRet;
        }
        //Enumere les membres correspondant à des paramètres
        void EnumerateMembers(XmlNode oListOfNodeMembers, PlcSoftware oControllerTarget, TiaProjectForCPU oTiaProjectForCPU, TiaPortalBloc oBlocTIA, ref string sCurrentPath, bool bFirstLevel)
        {
            #region VARIABLES
            bool bIdentCont = false;
            // Gardons les listes séparées comme dans votre code original
            var vUIDVariableEntree = new List<string>();
            var vVariableEntree = new List<string>();
            var vUIDPatteBlockLTU = new List<string>();
            var vPatteBlockLTU = new List<string>();
            var vUIDBlockLTU = new List<string>();
            var vNomBlockLTU = new List<string>();
            var vNomDBi = new List<string>();

            // Pour suivre les éléments traités sans modifier les listes originales
            var processedPatteIndices = new HashSet<int>();
            #endregion

            // DEBUG - Structure pour stocker les relations entre éléments
            var debugRelations = new Dictionary<string, List<string>>();

            //Pour chaque reseaux, on recherche nos variables
            foreach (XmlNode nodeCompileUnit in oListOfNodeMembers.ChildNodes)
            {
                if (nodeCompileUnit.Name == @"AttributeList")
                {
                    foreach (XmlNode nodeAttributeList in nodeCompileUnit)
                    {
                        if (nodeAttributeList.Name == @"NetworkSource")
                        {
                            foreach (XmlNode nodeNetworkSource in nodeAttributeList)
                            {
                                if (nodeNetworkSource.Name == @"FlgNet")
                                {
                                    foreach (XmlNode nodeFlgNet in nodeNetworkSource)
                                    {
                                        //PARTIE DATA ET BLOCS
                                        if (nodeFlgNet.Name == @"Parts")
                                        {
                                            foreach (XmlNode nodeParts in nodeFlgNet)
                                            {
                                                //ON CHERCHE POUR LES NOMS DES VARIABLES EN ENTREE DU BLOC
                                                if (nodeParts.Name == @"Access")
                                                {
                                                    //Sauvegarde du Access Uid
                                                    string accessUid = nodeParts.Attributes["UId"].Value.ToString();
                                                    vUIDVariableEntree.Add(accessUid);
                                                    string componentName = "";

                                                    foreach (XmlNode nodeAccess in nodeParts)
                                                    {
                                                        //Si on a une VARIABLE de connecter
                                                        if (nodeAccess.Name == @"Symbol")
                                                        {
                                                            bool firstComponent = true;

                                                            foreach (XmlNode nodeSymbol in nodeAccess)
                                                            {
                                                                if (firstComponent == true)
                                                                {
                                                                    if (nodeSymbol.Name == @"Component")
                                                                    {
                                                                        //Sauvegarde du COMPONENT NAME
                                                                        componentName = nodeSymbol.Attributes["Name"].Value.ToString();
                                                                        vVariableEntree.Add(componentName);
                                                                        firstComponent = false;

                                                                        // DEBUG - Enregistrer la relation
                                                                        if (!debugRelations.ContainsKey("Variable"))
                                                                            debugRelations["Variable"] = new List<string>();
                                                                        debugRelations["Variable"].Add($"UID:{accessUid}, Name:{componentName}");
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        //Si on a une CONSTANTE de connecter
                                                        else if (nodeAccess.Name == @"Constant")
                                                        {
                                                            foreach (XmlNode nodeConstant in nodeAccess)
                                                            {
                                                                if (nodeConstant.Name == @"ConstantType")
                                                                {
                                                                    //Sauvegarde du COMPONENT NAME
                                                                    componentName = nodeConstant.Name.ToString();
                                                                    vVariableEntree.Add(componentName);

                                                                    // DEBUG - Enregistrer la relation
                                                                    if (!debugRelations.ContainsKey("Constant"))
                                                                        debugRelations["Constant"] = new List<string>();
                                                                    debugRelations["Constant"].Add($"UID:{accessUid}, Type:{componentName}");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                //SI ON CHERCHE POUR LE NOM DU BLOC APPELE ET DU DBi
                                                else if (nodeParts.Name == @"Call")
                                                {
                                                    //Sauvegarde de l'UId du FC appelé
                                                    string blockUid = nodeParts.Attributes["UId"].Value.ToString();
                                                    vUIDBlockLTU.Add(blockUid);
                                                    string blockName = "";
                                                    string dbiName = "";

                                                    foreach (XmlNode nodeCall in nodeParts)
                                                    {
                                                        if (nodeCall.Name == @"CallInfo")
                                                        {
                                                            //Sauvegarde Nom du FC appelé
                                                            blockName = nodeCall.Attributes["Name"].Value.ToString();
                                                            vNomBlockLTU.Add(blockName);

                                                            foreach (XmlNode nodeCallInfo in nodeCall)
                                                            {
                                                                if (nodeCallInfo.Name == @"Instance")
                                                                {
                                                                    foreach (XmlNode nodeInstance in nodeCallInfo)
                                                                    {
                                                                        if (nodeInstance.Name == @"Component")
                                                                        {
                                                                            //Sauvegarde Nom du DBi utilisé
                                                                            dbiName = nodeInstance.Attributes["Name"].Value.ToString();
                                                                            vNomDBi.Add(dbiName);

                                                                            // DEBUG - Enregistrer la relation
                                                                            if (!debugRelations.ContainsKey("Block"))
                                                                                debugRelations["Block"] = new List<string>();
                                                                            debugRelations["Block"].Add($"UID:{blockUid}, Name:{blockName}, DBi:{dbiName}");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //PARTIE LIENS DES BLOCS
                                        else if (nodeFlgNet.Name == @"Wires")
                                        {
                                            foreach (XmlNode nodeWires in nodeFlgNet)
                                            {
                                                //LISTING DES LIENS
                                                if (nodeWires.Name == @"Wire")
                                                {
                                                    string wireSource = "";
                                                    string wireTarget = "";
                                                    string wireName = "";

                                                    foreach (XmlNode nodeWire in nodeWires)
                                                    {
                                                        if (nodeWire.Name == @"IdentCon")
                                                        {
                                                            wireSource = nodeWire.Attributes["UId"].Value.ToString();
                                                        }
                                                        else if (nodeWire.Name == @"NameCon")
                                                        {
                                                            wireTarget = nodeWire.Attributes["UId"].Value.ToString();
                                                            wireName = nodeWire.Attributes["Name"].Value.ToString();

                                                            //Sauvegarde du NAMECON Name et UId
                                                            vPatteBlockLTU.Add(wireName);
                                                            vUIDPatteBlockLTU.Add(wireTarget);

                                                            // DEBUG - Enregistrer la relation Wire
                                                            if (!debugRelations.ContainsKey("Wire"))
                                                                debugRelations["Wire"] = new List<string>();
                                                            debugRelations["Wire"].Add($"Source:{wireSource}, Target:{wireTarget}, Name:{wireName}");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // DEBUG - Afficher les relations
            Console.WriteLine("=== DEBUG RELATIONS ===");
            foreach (var category in debugRelations)
            {
                Console.WriteLine($"Category: {category.Key}");
                foreach (var item in category.Value)
                {
                    Console.WriteLine($"  {item}");
                }
            }

            // Créons un mapping entre les UID des variables et leurs noms
            var uidToVariableName = new Dictionary<string, string>();
            for (int i = 0; i < Math.Min(vUIDVariableEntree.Count, vVariableEntree.Count); i++)
            {
                uidToVariableName[vUIDVariableEntree[i]] = vVariableEntree[i];
            }

            // Créons un mapping entre patteBlockLTU et les variables liées
            var wireMappings = new Dictionary<string, Dictionary<string, string>>();
            for (int blockIdx = 0; blockIdx < vUIDBlockLTU.Count; blockIdx++)
            {
                string blockUID = vUIDBlockLTU[blockIdx];
                wireMappings[blockUID] = new Dictionary<string, string>();

                for (int wireIdx = 0; wireIdx < vUIDPatteBlockLTU.Count; wireIdx++)
                {
                    if (vUIDPatteBlockLTU[wireIdx] == blockUID)
                    {
                        string patteName = vPatteBlockLTU[wireIdx];

                        // Recherche de la variable liée (regarder dans les Wire pour trouver la source)
                        foreach (var wireInfo in debugRelations["Wire"])
                        {
                            if (wireInfo.Contains($"Target:{blockUID}") && wireInfo.Contains($"Name:{patteName}"))
                            {
                                // Extraire l'UID source
                                int sourceIdx = wireInfo.IndexOf("Source:") + 7;
                                int sourceEndIdx = wireInfo.IndexOf(",", sourceIdx);
                                string sourceUID = wireInfo.Substring(sourceIdx, sourceEndIdx - sourceIdx);

                                // Si on a un nom de variable pour cette source
                                if (uidToVariableName.ContainsKey(sourceUID))
                                {
                                    wireMappings[blockUID][patteName] = uidToVariableName[sourceUID];
                                }
                            }
                        }
                    }
                }
            }

            // DEBUG - Afficher les mappings wire-variable
            Console.WriteLine("=== WIRE TO VARIABLE MAPPINGS ===");
            foreach (var blockMapping in wireMappings)
            {
                Console.WriteLine($"Block UID: {blockMapping.Key}");
                foreach (var patteMapping in blockMapping.Value)
                {
                    Console.WriteLine($"  Patte: {patteMapping.Key}, Variable: {patteMapping.Value}");
                }
            }

            //On verifie chaque LTU que l'on a detecter dans le reseau
            for (int a = 0; a < vNomBlockLTU.Count; a++)
            {
                string blockUID = vUIDBlockLTU[a];

                //Lecture des Donnees Excel
                foreach (KeyValuePair<Tuple<int, int>, object> keyValue in dData)
                {
                    //On scan la premiere colonne du fichier Excel
                    if (keyValue.Key.Item2 == 1)
                    {
                        //Si le nom du LTU export est présent dans le fichier Excel
                        if (vNomBlockLTU[a] == keyValue.Value.ToString())
                        {
                            //Ecriture de la premiere colonne dans le DBi
                            lsDataCollection.Add("\"" + vNomDBi[a] + "\"" + ".sHmiUdt.backJump[0] := '" + vNomBlockLTU[a] + "';");
                            //Sauvegarde de l'indice de la ligne du LTU trouver
                            int indiceLigne = keyValue.Key.Item1;

                            // Structure pour stocker toutes les variables dans l'ordre des colonnes Excel
                            var excelColumnToVariable = new Dictionary<int, string>();

                            //Lecture des Donnees de la ligne LTU
                            foreach (KeyValuePair<Tuple<int, int>, object> keyValueH in dData)
                            {
                                //On scan la ligne a l'horizontal
                                if ((keyValueH.Key.Item1 == indiceLigne) && (keyValueH.Key.Item2 > 1))
                                {
                                    //On verifie que la cellule n'est pas vide
                                    if (keyValueH.Value.ToString() != "")
                                    {
                                        string patteName = keyValueH.Value.ToString();
                                        int columnIndex = keyValueH.Key.Item2;

                                        // Chercher la variable correspondante dans notre mapping
                                        if (wireMappings.ContainsKey(blockUID) &&
                                            wireMappings[blockUID].ContainsKey(patteName))
                                        {
                                            string variableName = wireMappings[blockUID][patteName];
                                            if (!string.IsNullOrEmpty(variableName) && variableName != "ConstantType")
                                            {
                                                excelColumnToVariable[columnIndex] = variableName;
                                                Console.WriteLine($"Found variable for block {vNomBlockLTU[a]}, column {columnIndex}: {variableName}");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"No valid variable found for patte {patteName} in block {vNomBlockLTU[a]}");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine($"No mapping found for patte {patteName} in block {vNomBlockLTU[a]}");
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            // Trier les colonnes Excel par ordre croissant
                            var orderedColumns = excelColumnToVariable.Keys.OrderBy(k => k).ToList();

                            // Écrire les variables avec une numérotation consécutive
                            for (int i = 0; i < orderedColumns.Count; i++)
                            {
                                int columnIndex = orderedColumns[i];
                                lsDataCollection.Add("\"" + vNomDBi[a] + "\"" + ".sHmiUdt.backJump[" + (columnIndex - 1) + "] := '" + excelColumnToVariable[columnIndex] + "';");
                            }

                            // DEBUG - Afficher le nombre d'entrées trouvées
                            Console.WriteLine($"Block {vNomBlockLTU[a]} ({vNomDBi[a]}): Found {orderedColumns.Count} entries");
                        }
                    }
                }
            }
        }

        // Recherche des folders et blocs depuis la racine du projet
        void SearchAllFolderAndBlocInRootProgramFolder(PlcSoftware oControllerTarget, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            //Boucle de recherches de tout les folder
            foreach (PlcBlockUserGroup oblockUserFolder in oControllerTarget.BlockGroup.Groups)
            {
                //Ajout du folder dans notre arbre
                TreeNode oNode = new TreeNode(oblockUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oblockUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                //Boucle de balayage des blocs si ils existent
                oNodeSource?.Nodes.Add(oNode);

                //Enumération des blocs dans le folder
                EnumBlocksInFolderBlocks(oControllerTarget, oblockUserFolder, ref oTiaProjectForCPU, oNode);
                //Enumération de tous les sous folders
                EnumerateBlockUserFolders(oControllerTarget, oblockUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }
        //Recherche des blocs dans le folder
        private void EnumBlocksInFolderBlocks(PlcSoftware oControllerTarget, PlcBlockUserGroup oBlocks, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            string sNewName = string.Empty;

            //Boucle de traitement de tous les blocs du repertoire
            foreach (PlcBlock oBloc in oBlocks.Blocks)
            {
                //Test si on trouve sur un bloc FC alors EXPORT
                if (oBloc is FC)
                {
                    //Ajout à la liste interne
                    TreeNode oNode = new TreeNode(oBloc.Name);
                    TiaPortalBloc oTiaBloc = new TiaPortalBloc(oBloc.Name, oNodeSource, oTiaProjectForCPU.GetNextFolderVariableId(), sNewName);
                    //Enumération des listes des paramètres du bloc
                    ExportTiaBlockDBAndEnumerateOPCUAParameters(oControllerTarget, (oBloc as FC), oTiaBloc, ref oTiaProjectForCPU);
                    //Ajout du bloc dans la liste interne
                    oTiaProjectForCPU.loListBlocs.Add(oTiaBloc);
                    oNode.Tag = oTiaBloc;
                    //On ajoute le noeud dans l'arbre
                    oNodeSource.Nodes.Add(oNode);
                }
            }
        }
        //Recherche des sous folder dans un folder
        private void EnumerateBlockUserFolders(PlcSoftware oControllerTarget, PlcBlockUserGroup oBlockUserFolder, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            foreach (PlcBlockUserGroup oSubBlockUserFolder in oBlockUserFolder.Groups)
            {
                //Ajout du folder dans la liste
                TreeNode oNode = new TreeNode(oSubBlockUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oSubBlockUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                oNodeSource.Nodes.Add(oNode);
                //Boucle de balayage des blocs si ils existent
                EnumBlocksInFolderBlocks(oControllerTarget, oSubBlockUserFolder, ref oTiaProjectForCPU, oNode);
                EnumerateBlockUserFolders(oControllerTarget, oSubBlockUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }
        //Enumération des tags contenu dans les tables avec l'atributs OPC UA
        private bool EnumerateTagsWithOPCUAMarck(ref TiaProjectForCPU oTiaProjectForCPU, PlcSoftware oControllerPLC, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    //Recherche toutes les variables présentes dans la racine
                    SearchAllVariableTableInRootPlcTagTableFolder(oControllerPLC, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderTags());
                    // Balayage de tous les folders de la racine et les sous folders et tables associées
                    SearchAllFolderAndTagTableInRootTagTableFolder(oControllerPLC, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderTags());
                }
                catch (Exception ex)
                {
                    sErrorText = string.Format(@"Exception in EnumerateTagsWithOPCUAMarck() '{0}", ex.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        //Recherche des variables dans le folder racine
        private void SearchAllVariableTableInRootPlcTagTableFolder(PlcSoftware oControllerPLC, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            PlcTagTableComposition oTagTables = oControllerPLC.TagTableGroup.TagTables;

            //Boucle de traitement des tables présente sous la racine
            foreach (PlcTagTable oTagTable in oTagTables)
            {
                //On énumère la liste des variables pour récupérer la liste avec l'attribut OPC UA
                ReadAllVariablesInTableWithOPCUAFlag(oTagTable, ref oTiaProjectForCPU, oNodeSource);
            }
        }
        //Lecture de toutes les variables dans une table avec l'attribut OPC UA
        void ReadAllVariablesInTableWithOPCUAFlag(PlcTagTable oTagTable, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            List<string> lsListAttributs = new List<string>
            {
                @"ExternalWritable"
            };
            IList<object> iloListValueAttributes;
            bool bReadOnly = false;

            //Boucle de balayage de touts les tags de la table
            foreach (PlcTag oTag in oTagTable.Tags)
            {
                //Test si un commentaire existe sur ce tag
                if (oTag.Comment.Items.Count > 0)
                {
                    //On teste si l'attribut OPC UA est présent
                    if (oTag.Comment.Items[0].Text.ToUpper().IndexOf(oTiaProjectDefinitions.sCommentarTagVariableMarck.ToUpper()) == 0)
                    {
                        //Lecture de l'attribut ReadOnly
                        iloListValueAttributes = oTag.GetAttributes(lsListAttributs);
                        if ((bool)iloListValueAttributes[0] == false) bReadOnly = true;
                        else bReadOnly = false;
                        //Ajout de la variable dans la liste
                        TiaPortalVariable oVariable = new TiaPortalVariable(oTag.Name, null, oTag.DataTypeName, oTag.DataTypeName, oTiaProjectForCPU.GetNextFolderVariableId(),
                                                                           oNodeSource, oTag.Comment.Items[0].Text, string.Empty, bReadOnly, string.Empty);
                        oTiaProjectForCPU.loListVariablesTags.Add(oVariable);
                    }
                }
            }
        }
        //Recherche des folders et tables de tags depuis la racine du projet
        void SearchAllFolderAndTagTableInRootTagTableFolder(PlcSoftware oControllerPLC, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode odeSource)
        {
            //Boucle de recherches de tout les folder
            foreach (PlcTagTableUserGroup oControllerTargetUserFolder in oControllerPLC.TagTableGroup.Groups)
            {
                //Ajout du folder dans notre arbre
                TreeNode node = new TreeNode(oControllerTargetUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oControllerTargetUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                odeSource?.Nodes.Add(node);
                //Boucle de balayage des tables si ils existent
                EnumTagTableInFolderTagTable(oControllerTargetUserFolder, ref oTiaProjectForCPU, node);
                //Enumération de tous les sous folders dans le folder tagTable
                EnumerateTagTableUserGroup(oControllerTargetUserFolder, ref oTiaProjectForCPU, node);
            }
        }
        //Enumere toutes les tagatable dans le folder tagtable
        void EnumTagTableInFolderTagTable(PlcTagTableUserGroup oControllerTagUserFolder, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            //Boucle de traitement de tous les blocs du repertoire
            foreach (PlcTagTable oTagTable in oControllerTagUserFolder.TagTables)
            {
                ReadAllVariablesInTableWithOPCUAFlag(oTagTable, ref oTiaProjectForCPU, oNodeSource);
            }
        }
        //Enumere les folder tagTable dans le folder tagTable
        private void EnumerateTagTableUserGroup(PlcTagTableUserGroup oControllerTargetUserFolder, ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            foreach (PlcTagTableUserGroup oSubControllerTargetUserFolder in oControllerTargetUserFolder.Groups)
            {
                //Ajout du folder dans la liste
                TreeNode oNode = new TreeNode(oSubControllerTargetUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oSubControllerTargetUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                oNodeSource.Nodes.Add(oNode);
                //Boucle de balayage des tables si ils existent
                EnumTagTableInFolderTagTable(oSubControllerTargetUserFolder, ref oTiaProjectForCPU, oNode);
                EnumerateTagTableUserGroup(oSubControllerTargetUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }
        //Enumération des variables system pour le diagnostic
        private bool EnumerateVariableSystemTags(ref TiaProjectForCPU oTiaProjectForCPU, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    //Insertion de toutes les variables systemes pour le diagnostic
                    ScanAllSystemVariables(ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderSystemVariables());
                }
                catch (Exception e)
                {
                    sErrorText = string.Format(@"Exception in EnumerateVariableSystemTags() '{0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        //Ajoute du repertoire des variables systeme et les variables systeme
        void ScanAllSystemVariables(ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            string sVariableMappingName = string.Empty;
            TiaPortalVariable oTiaPortalVariable = null;

            //Ajout du folder correspondant au repertoire des variables systeme
            TreeNode oNode = new TreeNode(oTiaProjectForCPU.sRootVariableSystemName)
            {
                Tag = new TiaPortalFolder(oTiaProjectForCPU.sRootVariableSystemName,
                                           oTiaProjectForCPU.GetNextFolderVariableId())
            };
            oNodeSource.Nodes.Add(oNode);

            //Ajout de toutes les variables systeme
            //Ajout de "ServiceLevel"
            sVariableMappingName = @"""OPC_UA_Server_State"".""iServiceLevel""";

            // Modification passage en lecture écriture pour gestion Stop des cpus gateway
            oTiaPortalVariable = new TiaPortalVariable(@"ServiceLevel", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Service level de la redondance OPC UA", sVariableMappingName, false, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            // Ajout de "EnableWriteToPLCH"
            sVariableMappingName = @"""OPC_UA_Server_State"".""bEnableWriteToPLCH""";
            oTiaPortalVariable = new TiaPortalVariable(@"EnableWriteToPLCH", null, @"Boolean", @"Boolean", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Valide l'écriture dans le PLC H distant", sVariableMappingName, false, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            // Ajout de "ServerStatus"
            sVariableMappingName = @"""OPC_UA_Server_State"".""iServerStatus""";
            oTiaPortalVariable = new TiaPortalVariable(@"ServerStatus", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Status du serveur OPC au niveau du PLC H distant", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            // Ajout de "PLC_H_Redundant_State"
            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLC_H_Redundant_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLC_H_Redundant_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la redondance du PLC_H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            // Ajout de "PLc_CPU_1_State"
            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLc_CPU_1_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLc_CPU_1_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la CPU 1 du PLC H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            // Ajout de "PLc_CPU_2_State"
            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLc_CPU_2_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLc_CPU_2_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la CPU 2 du PLC H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);
        }
        #endregion
    }
    // Classe de représentation d'une interface process Tia Portal
    public class HMATiaPortalProcess
    {
        #region Variables
        private TiaPortalProcess oTiaPortalProcess;
        public TiaPortalProcess GetTiaPortalProcess() { return oTiaPortalProcess; }
        #endregion

        // Constructeur de la classe
        public HMATiaPortalProcess(TiaPortalProcess oTiaPortalProcess)
        {
            this.oTiaPortalProcess = oTiaPortalProcess;
        }

        public override string ToString()
        {
            return string.Format("{0}", Path.GetFileNameWithoutExtension(oTiaPortalProcess.ProjectPath.FullName));
        }
    }
    // Classe de représentation d'un device dans un projet TIA Portal
    public class HMATiaPortalDevice
    {
        #region Variables

        private Device oTiaPortalDevice;
        public Device GetPortalDevice() { return oTiaPortalDevice; }

        #endregion

        // Constructeur
        public HMATiaPortalDevice(Device oDevice)
        {
            this.oTiaPortalDevice = oDevice;
        }

        // Pour ajout dans la combobox de sélection des devices Tia Portal
        public override string ToString()
        {
            return string.Format("{0}", oTiaPortalDevice.Name);
        }
    }
}
