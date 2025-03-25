using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Protobuf.WellKnownTypes;
using OpennessV16;
using Siemens.Engineering.CrossReference;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;

namespace ReceptionDeProjet
{
    public class CompareTIA
    {
        public CompareTIA()
        { }
        public List<Automate> GetPlcDevicesInfo(HMATIAOpenness_V16 tiaInterface, string sError)
        {
            var resultAutomates = new List<Automate>();

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

                        // Affichage de debug
                        DebugObBlocks(automate);

                        resultAutomates.Add(automate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de l'accès aux propriétés de sécurité: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations des PLC : {ex.Message}");
            }

            return resultAutomates;
        }
        private DeviceItem FindMainModule(Device device)
        {
            foreach (var item in device.DeviceItems)
            {
                if (item.GetAttribute("Name").ToString().Contains("AP"))
                {
                    return item;
                }
            }
            return null;
        }
        private Automate CreateAutomateFromModule(DeviceItem mainModule)
        {
            var automate = new Automate
            {
                Name = mainModule.GetAttribute("Name").ToString(),
                Reference = mainModule.GetAttribute("OrderNumber").ToString(),
                Firmware = mainModule.GetAttribute("FirmwareVersion").ToString(),
                WatchDog = int.Parse(mainModule.GetAttribute("CycleMaximumCycleTime").ToString()),
                // Vérifier si "ControlAccess" doit utiliser une autre propriété selon le contexte
                ControlAccess = mainModule.GetAttribute("CycleMaximumCycleTime").ToString() == "PlcAccessControlConfiguration",
                WebServer = (bool)mainModule.GetAttribute("WebserverActivate"),
                Restart = int.Parse(mainModule.GetAttribute("StartupActionAfterPowerOn").ToString()) == 3,
                CadenceM0 = (bool)mainModule.GetAttribute("SystemMemoryByte"),
                CadenceM1 = (bool)mainModule.GetAttribute("ClockMemoryByte"),
                LocalHour = int.Parse(mainModule.GetAttribute("TimeOfDayLocalTimeZone").ToString()) == 25,
                HourChange = (bool)mainModule.GetAttribute("TimeOfDayActivateDaylightSavingTime")
            };

            // Analyse des sous-éléments (MMC, écran, etc.)
            foreach (var item in mainModule.DeviceItems)
            {
                var itemName = item.GetAttribute("Name").ToString();
                if (itemName.Contains("Dispositif de lecture"))
                {
                    automate.MMCLife = int.Parse(item.GetAttribute("DiagnosticsAgingSimaticMemoryCardThreshold").ToString());
                }
                else if (itemName.Contains("Ecran"))
                {
                    automate.ScreenWrite = (bool)item.GetAttribute("DisplayWriteAccess");
                }
            }

            return automate;
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
            foreach (MyOB myOB in automate.oOBs)
            {
                iNbLTU += myOB.iNbLTU;
                iNbBloc += myOB.iNbBloc;
                if (myOB.iID == 1) iTotalBlocOB1 = myOB.iNbBloc;
            }
            automate.StandardLTU =(iNbLTU*100)/ iNbBloc;

            // Bloc OB1
            automate.BlocOb1 = (iTotalBlocOB1*100)/ iNbBloc;
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
            Console.WriteLine($"Pourcentage de blocs LTU dans l'automate : {automate.StandardLTU}%");
            Console.WriteLine($"Pourcentage de blocs OB1 dans l'automate : {automate.BlocOb1}%");
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
            return (double)ltuCount / totalBlocs * 100.0;
        }
        private void PIDOB1(Automate automate)
        {
            foreach (MyOB myOb in automate.oOBs)
            {
                if (myOb.iID == 1)
                {
                    foreach (MyBloc myBloc in myOb.oBlocList)
                    {
                        if (myBloc.sName == "PID")
                        {
                            automate.OB1PID = true;
                            break;
                        }
                    }
                }
            }
        }
        private void BlockProtection(DeviceItem mainModule, Automate automate)
        {
            foreach(var block in GetAllBlocksFromCPU(mainModule))
            {
                if (bool.Parse(block.GetAttribute("IsKnowHowProtected").ToString())) automate.AddProtectedBloc(block.GetAttribute("Name").ToString());
            }
        }
    }

    public class Automate
    {
        #region VARIABLES
        // Variables Paramétrables
        private string sName;
        private string sReference;
        private string sFirmware;
        private int iWatchDog;
        private int iNtp;
        private int iStandtardLTU; // Idem ???
        private int iBLocOb1; // Input ???
        private bool bControlAccess;
        private bool bApiHmiCom;
        private bool bOnlineAccess;

        // Variables fixes 
        private string sInterfaceX1;
        private string sInterfaceX2;
        private string sDiagnosticTampon;
        private bool bWebServer = true;
        private bool bRestart = true;
        private bool bCadenceM0 = true;
        private bool bCadenceM1 = true;
        private bool bScreenWrite = false;
        private bool bLocalHour = true;
        private bool bHourChange = true;
        private bool bCPUTimeSet = true;
        public List<MyOB> oOBs;
        public List<MyFC> oFCs;
        public List<string> ProtectedBlocs;
        private bool bProgramProtection = false;
        private bool bCPUDefault = false;
        private bool bOB1PID = false;
        private int iMMCLife = 80;
        private int iLoadMemory = 80;
        private int iWorkCodeMemory = 80;
        private int iWorkDataMemory = 80;
        private int iInstantVar = 7;
        #endregion
        public Automate()
        {
            oOBs = new List<MyOB>();
            oFCs = new List<MyFC>();
            ProtectedBlocs = new List<string>();
        }

        #region GETTER & SETTER
        // Variables Paramétrables
        public string Name { get => sName; set => sName = value; }
        public string Reference { get => sReference; set => sReference = value; }
        public string Firmware { get => sFirmware; set => sFirmware = value; }
        public int WatchDog { get => iWatchDog; set => iWatchDog = value; }
        public int Ntp { get => iNtp; set => iNtp = value; }
        public int StandardLTU { get => iStandtardLTU; set => iStandtardLTU = value; }
        public int BlocOb1 { get => iBLocOb1; set => iBLocOb1 = value; }
        public bool ControlAccess { get => bControlAccess; set => bControlAccess = value; }
        public bool ApiHmiCom { get => bApiHmiCom; set => bApiHmiCom = value; }
        public bool OnlineAccess { get => bOnlineAccess; set => bOnlineAccess = value; }

        // Variables fixes
        public string InterfaceX1 { get => sInterfaceX1; set => sInterfaceX1 = value; }
        public string InterfaceX2 { get => sInterfaceX2; set => sInterfaceX2 = value; }
        public string DiagnosticTampon { get => sDiagnosticTampon; set => sDiagnosticTampon = value; }
        public bool LocalHour { get => bLocalHour; set => bLocalHour = value; }
        public bool WebServer { get => bWebServer; set => bWebServer = value; }
        public bool Restart { get => bRestart; set => bRestart = value; }
        public bool CadenceM0 { get => bCadenceM0; set => bCadenceM0 = value; }
        public bool CadenceM1 { get => bCadenceM1; set => bCadenceM1 = value; }
        public bool ScreenWrite { get => bScreenWrite; set => bScreenWrite = value; }
        public bool HourChange { get => bHourChange; set => bHourChange = value; }
        public bool CPUTimeSet { get => bCPUTimeSet; set => bCPUTimeSet = value; }
        public bool ProgramProtection { get => bProgramProtection; set => bProgramProtection = value; }
        public bool CPUDefault { get => bCPUDefault; set => bCPUDefault = value; }
        public bool OB1PID { get => bOB1PID; set => bOB1PID = value; }
        public int MMCLife { get => iMMCLife; set => iMMCLife = value; }
        public int LoadMemory { get => iLoadMemory; set => iLoadMemory = value; }
        public int WorkCodeMemory { get => iWorkCodeMemory; set => iWorkCodeMemory = value; }
        public int WorkDataMemory { get => iWorkDataMemory; set => iWorkDataMemory = value; }
        public int InstantVar { get => iInstantVar; set => iInstantVar = value; }

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
}
