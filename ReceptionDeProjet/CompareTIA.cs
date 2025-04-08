using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Protobuf.WellKnownTypes;
using OpennessV16;
using Siemens.Engineering.CrossReference;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Online;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using System.Globalization;

namespace ReceptionDeProjet
{
    public class CompareTIA
    {
        public CompareTIA()
        { }
        public Project GetPlcDevicesInfo(HMATIAOpenness_V16 tiaInterface, string sError)
        {
            // Création de l'objet Project
            var resultProject = new Project
            {
                sName = tiaInterface.m_oTiaProject.Name,
                sProjectPath = tiaInterface.m_oTiaProject.Path.ToString(),
                sSimulation = tiaInterface.m_oTiaProject.GetAttribute("IsSimulationDuringBlockCompilationEnabled").ToString(),
                sDateCreation = tiaInterface.m_oTiaProject.GetAttribute("CreationTime").ToString(),
                sSize = tiaInterface.m_oTiaProject.GetAttribute("Size").ToString()
            };

            //Ajout de la version
            resultProject.sVersion = resultProject.sProjectPath.Split('p').Last();

            //Ajout des devices
            try
            {
                foreach (var device in tiaInterface.m_oTiaProject.Devices)
                {
                    // Évite de traiter les devices vides
                    if (device.DeviceItems.Count <= 1)
                    {
                        Console.WriteLine("Le device est vide");
                        continue;
                    }

                    // Recherche du module principal
                    var mainModule = FindMainModule(device);
                    if (mainModule == null)
                    {
                        Console.WriteLine("Erreur module vide");
                        continue;
                    }
                    // Création de l'objet Automate et récupération des propriétés
                    try
                    {
                        var automate = CreateAutomateFromModule(mainModule);

                        // Analyse et récupération des blocs OB
                        AnalyzeObBlocks(mainModule, automate);

                        // Calcul des statistiques
                        CalculStatPLC(automate);

                        // Vérification de la présence de PID dans OB1
                        PIDOB1(automate);

                        // Protection des blocs
                        BlockProtection(mainModule, automate);

                        // Interface réseau
                        InterfaceNetwork(mainModule, automate);

                        // Affichage de debug
                        DebugObBlocks(automate);

                        resultProject.AddAutomate(automate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de l'accès aux propriétés de sécurité: {ex.Message} {device.Name}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations des PLC : {ex.Message}");
            }

            return resultProject;
        }
        private DeviceItem FindMainModule(Device device)
        {
            foreach (var item in device.DeviceItems)
            {
                if (item.GetAttribute("Classification").ToString() == "CPU")
                {
                    return item;
                }
            }
            return null;
        }
        private Automate CreateAutomateFromModule(DeviceItem mainModule)
        {
            string gamme = FindGamme(mainModule);
            string[] gammesAutorisees = { "S7-1200", "S7-1500", "ET200SP" };

            if (!gammesAutorisees.Any(g => gamme.Contains(g)))
            {
                Console.WriteLine("Le device n'est pas dans la gamme");

                var automateBis = new Automate
                {
                    sName = mainModule.GetAttribute("Name").ToString(),
                    sGamme = gamme + " | Hors gamme",
                };
                return automateBis;
            }

            var automate = new Automate
            {
                sName = mainModule.GetAttribute("Name").ToString(),
                sGamme = gamme,
                sReference = mainModule.GetAttribute("OrderNumber").ToString(),
                sFirmware = mainModule.GetAttribute("FirmwareVersion").ToString(),
                sWatchDog = mainModule.GetAttribute("CycleMaximumCycleTime").ToString(),
                sWebServer = mainModule.GetAttribute("WebserverActivate").ToString(),
                sRestart = mainModule.GetAttribute("StartupActionAfterPowerOn").ToString(),
                sCadenceM0 = mainModule.GetAttribute("SystemMemoryByte").ToString(),
                sCadenceM1 = mainModule.GetAttribute("ClockMemoryByte").ToString(),
                sLocalHour = mainModule.GetAttribute("TimeOfDayLocalTimeZone").ToString(),
                sHourChange = mainModule.GetAttribute("TimeOfDayActivateDaylightSavingTime").ToString()
            };

            PlcAccessControlConfigurationProvider provider = mainModule.GetService<PlcAccessControlConfigurationProvider>();
            if (provider != null)
            {
                var configuration = provider.GetAttribute("PlcAccessControlConfiguration");
                automate.sControlAccess = configuration.ToString();
            }else automate.sControlAccess = "Option non disponible";


            // Analyse des sous-éléments (MMC, écran, etc.)
            foreach (var item in mainModule.DeviceItems)
                {
                    var itemName = item.GetAttribute("Name").ToString();
                    if (itemName.Contains("Dispositif de lecture") || itemName.Contains("Card reader"))
                    {
                        if (item.GetAttribute("DiagnosticsAgingSimaticMemoryCard").ToString() == "True")
                        {
                            automate.sMMCLife = item.GetAttribute("DiagnosticsAgingSimaticMemoryCardThreshold").ToString();
                        }
                        else automate.sMMCLife = "Option desactivé";
                    }
                    else if (itemName.Contains("Ecran") || itemName.Contains("display"))
                    {
                        automate.sScreenWrite = item.GetAttribute("DisplayWriteAccess").ToString();
                    }
                }

            automate.sOnlineAccess = TestOnlineAccess(mainModule);

            return automate;
        }
        private string FindGamme(DeviceItem mainModule)
        {
            string sGamme = "Non trouvé";
            
            Device parent = (Device)mainModule.Parent;
            sGamme = parent.GetAttribute("TypeName").ToString();
            Console.WriteLine($"Gamme : {sGamme}"); 

            return sGamme;
        }
        private string TestOnlineAccess(DeviceItem mainModule)
        {
            try
            {
                OnlineProvider onlineProvider = mainModule.GetService<OnlineProvider>();
                if (onlineProvider == null) return "False"; 
                if (onlineProvider.Configuration.IsConfigured)
                {
                    onlineProvider.GoOnline();
                }
                onlineProvider.GoOffline();
                return "True";
            }
            catch (Exception e)
            {
                OnlineProvider onlineProvider = mainModule.GetService<OnlineProvider>();
                onlineProvider.GoOffline();
                Console.WriteLine($"Erreur lors de la récupération de l'OnlineAccess : {e.Message}");
                return "False";
            }
        }
        private void AnalyzeObBlocks(DeviceItem mainModule, Automate automate)
        {
            foreach (var block in GetAllBlocksFromCPU(mainModule))
            {
                if (block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.FC")
                {
                    Console.WriteLine($"FC : {block.GetAttribute("Name")}");
                    var vFcNumber = int.Parse(block.GetAttribute("Number").ToString());
                    var vFcObject = new MyFC
                    {
                        sName = block.GetAttribute("Name").ToString(),
                        iID = vFcNumber,
                        sType = "FC",
                    };

                    // Recherche des références par croisement
                    var crossRefServiceFC = block.GetService<CrossReferenceService>() as CrossReferenceService;
                    var crossRefResultFC = crossRefServiceFC?.GetCrossReferences(CrossReferenceFilter.AllObjects);
                    var sourceObjectFC = crossRefResultFC?.Sources?.FirstOrDefault(s => s.Name == block.Name);

                    if (sourceObjectFC?.References == null)
                    {
                        automate.AddFc(vFcObject);
                        continue;
                    }

                    foreach (var referenceObject in sourceObjectFC.References)
                    {
                        var refType = referenceObject.GetAttribute("TypeName").ToString();
                        var refAddress = referenceObject.GetAttribute("Address").ToString();

                        foreach (var location in referenceObject.Locations)
                        {
                            if(!(location.GetAttribute("ReferenceType").ToString() == "UsedBy"))
                            {
                                // Choix du type selon la référence
                                if (refType != "Instruction" && !refAddress.Contains("FC") && !refAddress.Contains("FB"))
                                    continue;

                                var blockType = refType == "Instruction" ? "Instruction"
                                                : refAddress.Contains("FC") ? "FC"
                                                : refAddress.Contains("FB") ? "FB"
                                                : null;
                                if (blockType == null) continue;

                                var myBloc = new MyBloc
                                {
                                    sType = blockType,
                                    sName = blockType == "Instruction"
                                        ? referenceObject.GetAttribute("Name").ToString().Split(' ')[0]
                                        : referenceObject.GetAttribute("Name").ToString(),
                                };
                                vFcObject.AddBloc(myBloc);
                                vFcObject.iNbBloc++;
                                if (myBloc.sName.StartsWith("LTU", StringComparison.OrdinalIgnoreCase)) vFcObject.iNbLTU++;
                            }
                        }
                    }
                    automate.AddFc(vFcObject);
                }
            
                if(block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.InstanceDB")
                {

                }
            }

            foreach (MyFC fc in automate.oFCs)
            {
                var blocsACopier = fc.oInternalBlocs.ToList(); // Copie de la liste avant itération

                foreach (MyBloc bloc in blocsACopier)
                {
                    if (bloc.sType == "FC")
                    {
                        foreach (MyFC fcInternal in automate.oFCs)
                        {
                            if (fcInternal.sName == bloc.sName)
                            {
                                fc.AddBloc(fcInternal);
                                fc.iNbLTU += fcInternal.iNbLTU;
                                fc.iNbBloc += fcInternal.iNbBloc;
                                fc.oInternalBlocs.Remove(bloc); // Modification autorisée car on itère sur une copie
                            }
                        }
                    }
                }
            }

            foreach (var block in GetAllBlocksFromCPU(mainModule))
            {
                if (block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.OB")
                {
                    var obNumber = int.Parse(block.GetAttribute("Number").ToString());
                    var vObObject = new MyOB
                    {
                        sName = block.GetAttribute("Name").ToString(),
                        iID = obNumber
                    };

                    // Recherche des références par croisement
                    var crossRefService = block.GetService<CrossReferenceService>() as CrossReferenceService;
                    var crossRefResult = crossRefService?.GetCrossReferences(CrossReferenceFilter.AllObjects);
                    var sourceObject = crossRefResult?.Sources?.FirstOrDefault(s => s.Name == block.Name);

                    if (sourceObject?.References == null)
                    {
                        automate.AddOb(vObObject);
                        continue;
                    }

                    foreach (var referenceObject in sourceObject.References)
                    {
                        var refType = referenceObject.GetAttribute("TypeName").ToString();
                        var refAddress = referenceObject.GetAttribute("Address").ToString();

                        // Choix du type selon la référence
                        if (refType != "Instruction" && !refAddress.Contains("FC") && !refAddress.Contains("FB"))
                            continue;

                        var blockType = refType == "Instruction" ? "Instruction"
                                        : refAddress.Contains("FC") ? "FC"
                                        : refAddress.Contains("FB") ? "FB"
                                        : null;

                        if (blockType == null) continue;

                        var myBloc = new MyBloc
                        {
                            sType = blockType,
                            sName = blockType == "Instruction"
                                ? referenceObject.GetAttribute("Name").ToString().Split(' ')[0]
                                : referenceObject.GetAttribute("Name").ToString(),
                        };

                        if (myBloc.sType == "FC")
                        {
                            Console.WriteLine($"Bloc FC : {myBloc.sName}");
                            foreach (var fc in automate.oFCs)
                            {
                                if (fc.sName == myBloc.sName)
                                {
                                    Console.WriteLine($"Bloc FC interne : {fc.sName}");
                                    vObObject.AddBloc(fc);
                                }
                            }

                        }
                        else vObObject.AddBloc(myBloc);
                    }
                    vObObject.Ltu100 = CalculerPourcentageDeBlocsLTU(vObObject);
                    automate.AddOb(vObObject);
                }
            }
        }
        private void CalculStatPLC(Automate automate)
        {
            //% LTU
            int iNbLTU = 0;
            int iNbBloc = 0;
            int iTotalBlocOB1 = 0;
            int iTotalBlocOB35 = 0;
            foreach (MyOB myOB in automate.oOBs)
            {
                iNbLTU += myOB.iNbLTU;
                iNbBloc += myOB.iNbBloc;
                if (myOB.iID == 1) iTotalBlocOB1 = myOB.iNbBloc;
                if (myOB.iID == 35) iTotalBlocOB35 = myOB.iNbBloc;
            }
            if (iNbBloc == 0)
            {
                automate.iStandardLTU = 0;
                automate.iBlocOb1 = 0;
                automate.iBlocOb35 = 0;
            }
            else {
                automate.iStandardLTU = (iNbLTU * 100) / iNbBloc;
                automate.iBlocOb1 = (iTotalBlocOB1 * 100) / iNbBloc;
                automate.iBlocOb35 = (iTotalBlocOB35 * 100) / iNbBloc;
            } 
        }
        private void DebugObBlocks(Automate automate)
        {
            var sb = new StringBuilder();

            // Parcours des OBs
            foreach (var myObTest in automate.oOBs)
            {
                sb.AppendLine($">OB{myObTest.iID} : {myObTest.sName}");

                // Appel de la fonction récursive pour afficher les blocs
                foreach (var myBlocTest in myObTest.oBlocList)
                {
                    AppendBlocInfo(sb, myBlocTest, 1);
                }

                sb.AppendLine($"%LTU dans l'OB{myObTest.iID} = {myObTest.Ltu100}");
            }

            Console.WriteLine(sb.ToString());
            Console.WriteLine($"Pourcentage de blocs LTU dans l'automate : {automate.iStandardLTU}%");
            Console.WriteLine($"Pourcentage de blocs OB1 dans l'automate : {automate.iBlocOb1}%");
            Console.WriteLine($"Nombre de blocs protégés : {automate.ProtectedBlocs.Count}");
        }
        // Fonction récursive pour afficher les blocs et sous-blocs
        private void AppendBlocInfo(StringBuilder sb, MyBloc myBloc, int niveau)
        {
            string indentation = new string('\t', niveau);
            sb.AppendLine($"{indentation}>---{myBloc.sType} : {myBloc.sName}");

            if (myBloc is MyFC fcBloc)
            {
                // Création d'une copie avant l'itération
                var internalBlocs = fcBloc.oInternalBlocs.ToList();

                foreach (var subBloc in internalBlocs)
                {
                    AppendBlocInfo(sb, subBloc, niveau + 1);
                }
            }
        }
        public List<PlcBlock> GetAllBlocksFromCPU(DeviceItem cpuDeviceItem)
        {
            var allBlocks = new List<PlcBlock>();

            // Récupération du conteneur de software pour accéder aux blocs
            var softwareContainer = cpuDeviceItem.GetService<SoftwareContainer>() as SoftwareContainer;
            var plcSoftware = softwareContainer?.Software as PlcSoftware ?? throw new Exception("Impossible de récupérer PlcSoftware pour la CPU spécifiée.");

            // 1) Récupérer tout ce qui est dans la racine
            allBlocks.AddRange(plcSoftware.BlockGroup.Blocks);

            // 2) Parcourir récursivement les sous-dossiers
            foreach (PlcBlockUserGroup userGroup in plcSoftware.BlockGroup.Groups)
            {
                GatherBlocksFromGroup(userGroup, allBlocks);
            }

            return allBlocks;
        }
        // Méthode récursive pour parcourir les sous-dossiers
        private void GatherBlocksFromGroup(PlcBlockUserGroup group, List<PlcBlock> allBlocks)
        {
            // Ajouter les blocs directement dans le dossier
            foreach (PlcBlock block in group.Blocks)
            {
                allBlocks.Add(block);
            }

            // Parcourir les sous-groupes
            foreach (PlcBlockUserGroup subGroup in group.Groups)
            {
                GatherBlocksFromGroup(subGroup, allBlocks);
            }
        }
        public double CalculerPourcentageDeBlocsLTU(MyOB myOb)
        {
            if (myOb == null || myOb.oBlocList == null || myOb.oBlocList.Count == 0) return 0.0;
            // Compter le nombre de blocs dont le nom commence par "LTU"
            int totalBlocs = 0; // myOb.oBlocList.Count;
            int ltuCount = 0; // myOb.oBlocList.Count(b => b.sName.StartsWith("LTU", StringComparison.OrdinalIgnoreCase));

            foreach (var bloc in myOb.oBlocList)
            {
                
                totalBlocs++;
                if (bloc.sName.StartsWith("LTU", StringComparison.OrdinalIgnoreCase)) ltuCount++;
                if (bloc.sType == "FC")
                {
                    totalBlocs += ((MyFC)bloc).iNbBloc;
                    ltuCount += ((MyFC)bloc).iNbLTU;
                }
            }

            myOb.iNbLTU = ltuCount;
            myOb.iNbBloc = totalBlocs;

            // Calculer le pourcentage
            if(totalBlocs == 0) return 0.0;
            else return (double)ltuCount / totalBlocs * 100.0;
        }
        private void PIDOB1(Automate automate)
        {
            automate.sOB1PID = "Aucun bloc PID trouvé";
            foreach (MyOB myOb in automate.oOBs)
            {
                if (myOb.iID == 1)
                {
                    foreach (MyBloc myBloc in myOb.oBlocList)
                    {
                        if (myBloc.sName.Contains("PID"))
                        {
                            automate.sOB1PID = myBloc.sName;
                            break;
                        }
                    }
                }
            }
        }
        private void BlockProtection(DeviceItem mainModule, Automate automate)
        {
            automate.sProgramProtection = "Aucune protection trouvée";
            foreach (var block in GetAllBlocksFromCPU(mainModule))
            {
                if (bool.Parse(block.GetAttribute("IsKnowHowProtected").ToString())) automate.AddProtectedBloc(block.GetAttribute("Name").ToString());
            }
        }
        private void InterfaceNetwork(DeviceItem mainModule, Automate automate)
        {
            foreach(var item in mainModule.DeviceItems)
            {
                if (item.GetAttribute("Name").ToString().Contains("PROFINET"))
                {
                    if (item.GetAttribute("Name").ToString().Contains("1"))
                    {
                        var networkInterface = item.GetService<NetworkInterface>();
                        if (networkInterface?.GetAttribute("TimeSynchronizationNtp").ToString() == "True")
                        {
                            automate.sNtpServer1 = networkInterface?.GetAttribute("TimeSynchronizationServer1").ToString();
                            automate.sNtpServer2 = networkInterface?.GetAttribute("TimeSynchronizationServer2").ToString();
                            automate.sNtpServer3 = networkInterface?.GetAttribute("TimeSynchronizationServer3").ToString();
                        }
                        else
                        {
                            automate.sNtpServer1 = "Option désactivée";
                            automate.sNtpServer2 = "Option désactivée";
                            automate.sNtpServer3 = "Option désactivée";
                        }

                        var node = networkInterface.Nodes.FirstOrDefault();

                        automate.sInterfaceX1 = node?.GetAttribute("Address").ToString();
                        if (node?.GetAttribute("ConnectedSubnet") != null)
                        {
                            automate.sVlanX1 = node?.GetAttribute("ConnectedSubnet").ToString();
                        }
                        else
                        {
                            automate.sVlanX1 = "Pas de VLAN";

                        }
                    }

                    else if (item.GetAttribute("Name").ToString().Contains("2"))
                    {
                        var networkInterface = item.GetService<NetworkInterface>();
                        automate.sInterfaceX2 = networkInterface?.Nodes.FirstOrDefault()?.GetAttribute("Address").ToString();
                        if (networkInterface?.Nodes.FirstOrDefault()?.GetAttribute("ConnectedSubnet") != null)
                        {

                            automate.sVlanX2 = networkInterface?.Nodes.FirstOrDefault()?.GetAttribute("ConnectedSubnet").ToString();
                        }
                        else
                        {
                            automate.sVlanX2 = "Pas de VLAN";
                        }
                    }
                }                
            }
        }
    }


    public class Project
    {
        public string sName { get; set; }
        public string sProjectPath { get; set; }
        public string sVersion { get; set; }
        public string sDateCreation { get; set; }
        public string sLanguage { get; set; }
        public string sSize { get; set; }
        public string sSimulation { get; set; }

        //Liste des réseaux et liaisons


        //Liste des automates
        public List<Automate> oAutomates;
        public List<HMI> oHMIs;
        public List<SCADA> oSCADAs;

        public Project()
        {
            oAutomates = new List<Automate>();
            oHMIs = new List<HMI>();
            oSCADAs = new List<SCADA>();
        }

        public void AddAutomate(Automate automate)
        {
            oAutomates.Add(automate);
        }

        public void AddHMI(HMI hmi)
        {
            oHMIs.Add(hmi);
        }

        public void AddSCADA(SCADA scada)
        {
            oSCADAs.Add(scada);
        }
    }
    public class Connexion
    {
        public string sName { get; set; }
        public string sEndPoint { get; set; }
        public string sPartener { get; set; }
        public string sType { get; set; }
    }
    public class Automate
    {
        #region VARIABLES
        //Variable d'identification
        public string sName { get; set; }
        public string sGamme { get; set; }
        public string sReference { get; set; }
        public string sFirmware { get; set; }

        //Variable d'heure
        public string sNtpServer1 { get; set; }
        public string sNtpServer2 { get; set; }
        public string sNtpServer3 { get; set; }

        public string sLocalHour { get; set; }
        public string sHourChange { get; set; }

        //Variable réseau
        public string sInterfaceX1 { get; set; }
        public string sVlanX1 { get; set; }
        public string sInterfaceX2 { get; set; }
        public string sVlanX2 { get; set; }

        //Variable systéme
        public string sMMCLife { get; set; }
        public string sWatchDog { get; set; }
        public string sRestart { get; set; }
        public string sCadenceM0 { get; set; }
        public string sCadenceM1 { get; set; }

        //Variables d'accès au programme
        public string sProgramProtection { get; set; }
        public string sWebServer { get; set; }
        public string sControlAccess { get; set; }
        public string sApiHmiCom { get; set; } 
        public string sOnlineAccess { get; set; }
        public string sScreenWrite { get; set; }
        public string sInstantVar { get; set; }

        //Statistiques
        public int iStandardLTU { get; set; }
        public string sOB1PID { get; set; }
        public int iBlocOb1 { get; set; }   
        public int iBlocOb35 { get; set; }

        public List<MyOB> oOBs;
        public List<MyFC> oFCs;
        public List<string> ProtectedBlocs;
        #endregion
        public Automate()
        {
            oOBs = new List<MyOB>();
            oFCs = new List<MyFC>();
            ProtectedBlocs = new List<string>();
        }

        #region GETTER & SETTER
        


        public void AddOb(MyOB oOb)
        {
            oOBs.Add(oOb);
        }

        public void AddFc(MyFC oFc)
        {
            oFCs.Add(oFc);
        }

        public void AddProtectedBloc(string blocName)
        {
            ProtectedBlocs.Add(blocName);
        }
        #endregion
    }

    // Classe de base : MyBloc
    public class MyBloc
    {
        public string sName { get; set; }
        public string sType { get; set; }
        public int iNetwork { get; set; }
    }

    // Classe MyOB qui hérite de MyBloc
    public class MyOB : MyBloc
    {
        public int iID { get; set; }
        public int iNbNetwork { get; set; }
        public double Ltu100 { get; set; }
        public int iNbLTU { get; set; }
        public int iNbBloc { get; set; }
        public List<MyBloc> oBlocList { get; set; }

        public MyOB()
        {
            oBlocList = new List<MyBloc>();
        }

        public void AddBloc(MyBloc bloc)
        {
            oBlocList.Add(bloc);
        }
    }

    // Classe MyFC qui hérite de MyBloc
    public class MyFC : MyBloc
    {
        public int iID { get; set; }
        public int iNbLTU { get; set; }
        public int iNbBloc { get; set; }
        public List<MyBloc> oInternalBlocs { get; set; }
        

        public MyFC()
        {
            oInternalBlocs = new List<MyBloc>();
        }

        public void AddBloc(MyBloc bloc)
        {
            oInternalBlocs.Add(bloc);
        }
    }

    public class MyDB : MyOB
    {
        public string sLastInst { get; set; }
        public string sFirstInst { get; set; }
    }

    public class HMI
    {
        //Variable d'identification
        public string sName { get; set; }
        public string sReference { get; set; }
        public string sFirmware { get; set; }

        //Variable d'heure

        //Variable réseau
        public string sInterfaceX1 { get; set; }
        public string sVlanX1 { get; set; }
        public string sInterfaceX2 { get; set; }
        public string sVlanX2 { get; set; }
    }

    public class SCADA
    {
    }
}
