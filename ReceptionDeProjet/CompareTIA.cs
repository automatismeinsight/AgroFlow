using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
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
        protected string sCdcFilePath = null;
        List<Automate> oDevicesCdc = new List<Automate>();
        List<Automate> oDevicesProject = new List<Automate>();
        public CompareTIA() {
            //bool bRet = ReadExcel();
        }

        private bool ReadExcel(ref int iNbDevice)
        {
            try
            {
                using (XLWorkbook wb = new XLWorkbook(sCdcFilePath))
                {
                    // Récupère la première feuille
                    var ws = wb.Worksheets.First();
                    var range = ws.RangeUsed();

                    // Parcours des lignes
                    for (int i = 2; i < range.RowCount() + 1; i++)
                    {
                        iNbDevice++;

                        var automate = new Automate
                        {
                            // Variable parametrable
                            Name = ws.Cell(i, 0).Value.ToString(),
                            Reference = ws.Cell(i, 1).Value.ToString(),
                            Firmware = ws.Cell(i, 2).Value.ToString(),
                            WatchDog = (int)ws.Cell(i, 3).Value.GetNumber(),
                            Ntp = (int)ws.Cell(i, 4).Value.GetNumber(),
                            ControlAccess = ParseBooleanValue(ws.Cell(i, 5).Value.ToString()),
                            ApiHmiCom = ParseBooleanValue(ws.Cell(i, 6).Value.ToString()),
                            OnlineAccess = ParseBooleanValue(ws.Cell(i, 7).Value.ToString())
                        };
                        oDevicesCdc.Add(automate);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }
        }
        private bool ParseBooleanValue(string value)
        {
            return bool.TryParse(value, out bool result) && result;
        }
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
                switch (block.GetType().ToString())
                {
                    case "Siemens.Engineering.SW.Blocks.FC":
                        var vFcNumber = int.Parse(block.GetAttribute("Number").ToString());
                        var vFcbject = new MyOB
                        {
                            Name = block.GetAttribute("Name").ToString(),
                            ID = vFcNumber
                        };
                        break;
                    case "Siemens.Engineering.SW.Blocks.OB":
                        var obNumber = int.Parse(block.GetAttribute("Number").ToString());
                        var obObject = new MyOB
                        {
                            Name = block.GetAttribute("Name").ToString(),
                            ID = obNumber
                        };

                        // Recherche des références par croisement
                        var crossRefService = block.GetService<CrossReferenceService>() as CrossReferenceService;
                        var crossRefResult = crossRefService?.GetCrossReferences(CrossReferenceFilter.AllObjects);
                        var sourceObject = crossRefResult?.Sources?.FirstOrDefault(s => s.Name == block.Name);

                        if (sourceObject?.References == null)
                        {
                            automate.AddOb(obObject);
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
                                Type = blockType,
                                Name = blockType == "Instruction"
                                    ? referenceObject.GetAttribute("Name").ToString().Split(' ')[0]
                                    : referenceObject.GetAttribute("Name").ToString()
                            };

                            if (myBloc.Type == "FC")
                            {
                                MyBloc myBlocFC = null;
                                myBloc.AddBloc(myBlocFC);

                            }
                            obObject.AddBloc(myBloc);
                        }
                        obObject.Ltu100 = CalculerPourcentageDeBlocsLTU(obObject);
                        automate.AddOb(obObject);
                        break;
                    default:
                        break;
                }                
            }
        }
        private void DebugObBlocks(Automate automate)
        {
            // Exemple d'affichage de debug
            foreach (var myObTest in automate.oOBs)
            {
                foreach (var myBlocTest in myObTest.oBlocList)
                {
                    foreach (var myBlocTest2 in myBlocTest.oBlocList)
                    {
                        Console.WriteLine($"OB : {myObTest.Name}, Bloc : {myBlocTest.Name}, Bloc2 : {myBlocTest2.Name}");
                    }
                    Console.WriteLine($"OB : {myObTest.Name}, Bloc : {myBlocTest.Name}");
                }
                Console.WriteLine($"%LTU dans l'OB{myObTest.ID} = {myObTest.Ltu100}");
            }
        }
        public List<PlcBlock> GetAllBlocksFromCPU(DeviceItem cpuDeviceItem)
        {
            var allBlocks = new List<PlcBlock>();

            // Récupération du conteneur de software pour accéder aux blocs
            var softwareContainer = cpuDeviceItem.GetService<SoftwareContainer>() as SoftwareContainer;
            var plcSoftware = softwareContainer?.Software as PlcSoftware;

            if (plcSoftware == null)
            {
                throw new Exception("Impossible de récupérer PlcSoftware pour la CPU spécifiée.");
            }

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
            int totalBlocs = myOb.oBlocList.Count;
            int ltuCount = myOb.oBlocList.Count(b => b.Name.StartsWith("LTU", StringComparison.OrdinalIgnoreCase));

            // Calculer le pourcentage
            return (double)ltuCount / totalBlocs * 100.0;
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
        private bool bProgramProtection = false;
        private bool bCPUDefault = false;
        private bool bOB1PID = false;
        private int iMMCLife = 80;
        private int iLoadMemory = 80;
        private int iWorkCodeMemory = 80;
        private int iWorkDataMemory = 80;
        private int iInstantVar = 7;
        #endregion
        public Automate() {
            oOBs = new List<MyOB>();
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
        #endregion
    }

    public class MyOB
    {
        private int iID;
        private string sName;
        private int iNbNetwork;
        private double iLtu100;
        public List<MyBloc> oBlocList;
        public List<MyBloc> oBlocListFC;
        public MyOB()
        {
            oBlocList = new List<MyBloc>();
            oBlocListFC = new List<MyBloc>();
        }

        public int ID { get => iID; set => iID = value; }
        public int NbNetwork { get => iNbNetwork; set => iNbNetwork = value; }
        public string Name { get => sName; set => sName = value; }
        public double Ltu100 { get => iLtu100; set => iLtu100 = value; }
        public void AddBloc(MyBloc obloc)
        {
            oBlocList.Add(obloc);
        }
        public void AddBlocFC(MyBloc obloc)
        {
            oBlocListFC.Add(obloc);
        }
    }

    public class MyBloc
    {
        private string sName;
        private int iNetwork;
        private string iType;
        public List<MyBloc> oBlocList;

        public MyBloc()
        {
            if(iType == "FC") oBlocList = new List<MyBloc>();
        }
        public string Name { get => sName; set => sName = value; }
        public int Network { get => iNetwork; set => iNetwork = value; }
        public string Type { get => iType; set => iType = value; }
        public void AddBloc(MyBloc obloc)
        {
            oBlocList.Add(obloc);
        }
    }
}
