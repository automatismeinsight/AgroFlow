using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpennessV16;
using Siemens.Engineering.CrossReference;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Online;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering;

namespace ReceptionDeProjet
{
    /// <summary>
    /// Provides utility methods for analyzing and extracting information from a TIA Portal project.
    /// This class enables traversal and comparison of CPUs, their blocks, HMIs, SCADAs, switches, and other devices linked to the project.
    /// It is designed to facilitate the retrieval, organization, and comparison of automation project data for documentation, diagnostics, or migration purposes.
    /// </summary>
    public class CompareTIA
    {
        /// <summary>
        /// Temporary list of all variators in the project
        /// </summary>
        public List<MyVariator> oVariators = new List<MyVariator>();
        /// <summary>
        /// Temporary list of all InOuts in the project
        /// </summary>
        public List<MyInOut> oInOuts = new List<MyInOut>();
        /// <summary>
        /// Temporary list of all blocks in current CPU
        /// </summary>
        public List<PlcBlock> oCurrentCpuBloks = new List<PlcBlock>();
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareTIA"/> class.
        /// </summary>
        public CompareTIA()
        { }
        /// <summary>
        /// Extracts comprehensive information from a TIA Portal project using the provided openness interface,
        /// and constructs a <see cref="MyProject"/> object containing key metadata and embedded devices.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> interface representing the TIA Portal project to analyze.
        /// </param>
        /// <param name="sError">
        /// A string used to collect error messages encountered during extraction and analysis of project components.
        /// </param>
        /// <returns>
        /// A <see cref="MyProject"/> instance populated with project metadata (name, path, creation date, version, size, language)
        /// and detailed lists of PLCs, variators, InOuts, HMIs, SCADA systems, and switches found in the project.
        /// </returns>
        /// <remarks>
        /// This method gathers general project attributes and iteratively analyzes each device within the TIA project,
        /// invoking dedicated methods for variators, I/O modules, PLCs, HMIs, SCADA, and switches. Errors encountered
        /// during any extraction phase are aggregated in <paramref name="sError"/> for reporting or debugging purposes.
        /// </remarks>
        public MyProject GetTiaProjectContains(HMATIAOpenness_V16 tiaInterface, string sError)
        {
            var resultProject = new MyProject
            {
                sName = tiaInterface.m_oTiaProject.Name,
                sProjectPath = tiaInterface.m_oTiaProject.Path.ToString(),
                sSimulation = tiaInterface.m_oTiaProject.GetAttribute("IsSimulationDuringBlockCompilationEnabled").ToString(),
                sDateCreation = tiaInterface.m_oTiaProject.GetAttribute("CreationTime").ToString(),
                sSize = tiaInterface.m_oTiaProject.GetAttribute("Size").ToString()
            };

            resultProject.sVersion = resultProject.sProjectPath.Split('p').Last();
            Language oLangueProjet = tiaInterface.m_oTiaProject.LanguageSettings.GetAttribute("ReferenceLanguage") as Language;
            resultProject.sLanguage = oLangueProjet.GetAttribute("Culture").ToString().Split('-')[0];

            //Variator
            try
            {
                GetVariatorList(tiaInterface);
            }
            catch (Exception ex)
            {
                sError += ($"Erreur récupération d'information Variateur : {ex.Message}\n");
            }
            //InOut
            try
            {
                GetInOutsList(tiaInterface);
            }
            catch (Exception ex)
            {
                sError += ($"Erreur récupération d'information InOut : {ex.Message}\n");
            }
            //PLC
            try
            {
                foreach (var device in tiaInterface.m_oTiaProject.Devices)
                {
                    var resultAutomate = GetAutomateContains(device, resultProject);
                    if (resultAutomate != null)
                    {
                        resultProject.AddAutomate(resultAutomate);
                    }
                    else
                    {
                        continue;
                    }
                }

            }
            catch (Exception ex)
            {
                sError = ($"Erreur récupération d'information PLC : {ex.Message}\n");
            }
            //IHM
            try
            {
                GetHmiProject(tiaInterface, resultProject);
            }
            catch (Exception ex)
            {
                sError += ($"Erreur récupération d'information IHM : {ex.Message}\n");
            }
            //SCADA
            try
            {
                GetScadaProject(tiaInterface, resultProject);
            }
            catch (Exception ex)
            {
                sError += ($"Erreur récupération d'information SCADA : {ex.Message}\n");
            }
            //Switch
            try
            {
                GetSwitchProject(tiaInterface, resultProject);
            }
            catch (Exception ex)
            {
                sError += ($"Erreur récupération d'information Switch : {ex.Message}\n");
            }

            return resultProject;
        }
        /// <summary>
        /// Scans the TIA Portal project for SINAMICS variators, extracts their key attributes, and adds them to the collection of variators.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> interface representing the TIA Portal project to analyze.
        /// </param>
        /// <remarks>
        /// This method iterates through all device groups and devices in the project to identify devices of type SINAMICS. For each matching device,
        /// it inspects device items to extract variator details such as name, reference, type, and PROFINET network settings (IP address, VLAN, and master controller).
        /// Found variators are added to the <c>oVariators</c> collection. Only devices with a valid PROFINET interface and associated master are included.
        /// </remarks>
        public void GetVariatorList(HMATIAOpenness_V16 tiaInterface)
        {
            foreach (DeviceGroup deviceGroup in tiaInterface.m_oTiaProject.DeviceGroups)
            {
                foreach (Device device in deviceGroup.Devices)
                {
                    if (device.GetAttribute("TypeName").ToString().Contains("SINAMIC"))
                    {
                        foreach (DeviceItem deviceItem in device.DeviceItems)
                        {
                            if (deviceItem.GetAttribute("TypeName").ToString().Count() > 1)
                            {
                                MyVariator variator = new MyVariator
                                {
                                    sName = deviceItem.GetAttribute("Name").ToString(),
                                    sReference = deviceItem.GetAttribute("OrderNumber").ToString(),
                                    sGamme = deviceItem.GetAttribute("TypeName").ToString()
                                };

                                foreach (var item in deviceItem.DeviceItems)
                                {
                                    if (item.GetAttribute("Name").ToString().Contains("PROFINET"))
                                    {
                                        var networkInterface = item.GetService<NetworkInterface>();
                                        var node = networkInterface.Nodes.FirstOrDefault();
                                        variator.sInterfaceX1 = node?.GetAttribute("Address").ToString();
                                        Subnet subnet = node?.GetAttribute("ConnectedSubnet") as Subnet;
                                        if (subnet != null)
                                        {
                                            variator.sVlanX1 = subnet.GetAttribute("Name").ToString();
                                        }

                                        IoConnector ioConnector = networkInterface.IoConnectors.FirstOrDefault();
                                        IoSystem ioSystem = ioConnector?.GetAttribute("ConnectedToIoSystem") as IoSystem;
                                        HwIdentifier hwIdentifier = ioSystem?.HwIdentifiers.FirstOrDefault();

                                        HwIdentifierControllerAssociation hwIdentifierControllers = hwIdentifier?.GetAttribute("HwIdentifierControllers") as HwIdentifierControllerAssociation;
                                        HwIdentifierController hwIdentifierController = hwIdentifierControllers?.FirstOrDefault();

                                        if (hwIdentifierController != null)
                                        {
                                            variator.sMasterName = hwIdentifierController.GetAttribute("Name").ToString();
                                            oVariators.Add(variator);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Scans the TIA Portal project for I/O modules (excluding SINAMICS devices), extracts their key attributes, and adds them to the collection of InOut modules.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> interface representing the TIA Portal project to analyze.
        /// </param>
        /// <remarks>
        /// This method iterates through all device groups and devices in the project, filtering out SINAMICS devices. 
        /// For each I/O module identified (matching "IM " in the type name), it extracts details such as name, reference, range, and network configuration (IP address, VLAN, and master controller). 
        /// It also analyzes the I/O submodules to count the number of digital and analog inputs/outputs. 
        /// Found I/O modules are added to the <c>oInOuts</c> collection if all required attributes are available.
        /// </remarks>
        public void GetInOutsList(HMATIAOpenness_V16 tiaInterface)
        {
            foreach (DeviceGroup deviceGroup in tiaInterface.m_oTiaProject.DeviceGroups)
            {
                foreach (Device device in deviceGroup.Devices)
                {
                    if (!device.GetAttribute("TypeName").ToString().Contains("SINAMIC"))
                    {
                        foreach (DeviceItem deviceItem in device.DeviceItems)
                        {
                            if (deviceItem.GetAttribute("TypeName").ToString().Contains("IM "))
                            {
                                MyInOut inOut = new MyInOut
                                {
                                    sName = deviceItem.GetAttribute("Name").ToString(),
                                    sReference = deviceItem.GetAttribute("TypeIdentifier").ToString().Split(':')[1],
                                    sGamme = deviceItem.GetAttribute("ShortDesignation").ToString()
                                };

                                foreach (var item in deviceItem.DeviceItems)
                                {
                                    try
                                    {
                                        if (item.GetAttribute("Label").ToString().Contains("X1"))
                                        {
                                            var networkInterface = item.GetService<NetworkInterface>();
                                            var node = networkInterface.Nodes.FirstOrDefault();
                                            inOut.sInterfaceX1 = node?.GetAttribute("Address").ToString();
                                            Subnet subnet = node?.GetAttribute("ConnectedSubnet") as Subnet;
                                            if (subnet != null)
                                            {
                                                inOut.sVlanX1 = subnet.GetAttribute("Name").ToString();
                                            }

                                            IoConnector ioConnector = null;

                                            foreach (IoConnector connector in networkInterface.IoConnectors)
                                            {
                                                try
                                                {
                                                    string test = connector.GetAttribute("PnDeviceNumber").ToString();
                                                    if (test != null)
                                                    {
                                                        ioConnector = connector;
                                                        break;
                                                    }
                                                }
                                                catch
                                                {
                                                    continue;
                                                }
                                            }

                                            if (ioConnector == null)
                                            {
                                                continue;
                                            }
                                            IoSystem ioSystem = ioConnector?.GetAttribute("ConnectedToIoSystem") as IoSystem;
                                            HwIdentifier hwIdentifier = ioSystem?.HwIdentifiers.FirstOrDefault();

                                            HwIdentifierControllerAssociation hwIdentifierControllers = hwIdentifier?.GetAttribute("HwIdentifierControllers") as HwIdentifierControllerAssociation;
                                            HwIdentifierController hwIdentifierController = hwIdentifierControllers?.FirstOrDefault();

                                            if (hwIdentifierController != null)
                                            {
                                                inOut.sMasterName = hwIdentifierController.GetAttribute("Name").ToString();
                                                oInOuts.Add(inOut);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                                foreach (DeviceItem deviceItem2 in device.DeviceItems)
                                {
                                    string sType = deviceItem2.GetAttribute("TypeName").ToString().Split(' ')[0];

                                    switch (sType)
                                    {
                                        case "DI":
                                            inOut.iDI++;
                                            break;
                                        case "DQ":
                                            inOut.iDQ++;
                                            break;
                                        case "AI":
                                            inOut.iAI++;
                                            break;
                                        case "AQ":
                                            inOut.iAQ++;
                                            break;
                                        default:
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Extracts detailed information about a PLC device from the provided <see cref="Device"/> instance and constructs a <see cref="MyAutomate"/> object.
        /// </summary>
        /// <param name="device">
        /// The <see cref="Device"/> within the TIA Portal project to be analyzed as a PLC.
        /// </param>
        /// <param name="projet">
        /// The parent <see cref="MyProject"/> object used for context and additional associations.
        /// </param>
        /// <returns>
        /// A <see cref="MyAutomate"/> instance containing detailed information about the PLC, or <c>null</c> if the device is not valid or the main module is not found.
        /// </returns>
        /// <remarks>
        /// This method first checks if the device contains multiple items and identifies the main module. If valid, it initializes a new <see cref="MyAutomate"/> object,
        /// then proceeds to gather PLC block data, analyze OB blocks, calculate statistics, check for PID controllers, and retrieve network interface information.
        /// It also attempts to associate slave variators and I/O modules. Errors encountered during each extraction phase are logged to the console,
        /// but do not interrupt the overall process, ensuring partial results can still be returned.
        /// </remarks>
        public MyAutomate GetAutomateContains(Device device, MyProject projet)
        {
            if (device.DeviceItems.Count <= 1)
            {
                return null;
            }

            var mainModule = FindMainModule(device);
            if (mainModule == null)
            {
                return null;
            }
            try
            {
                MyAutomate automate = CreateAutomateFromModule(mainModule);

                try
                {
                    GetAllBlocksFromPlc(mainModule);
                }
                catch
                {
                    Console.WriteLine("Error retrieving blocks from PLC; Automate: " + automate.sName);
                }

                try
                {
                    AnalyzeObBlocks(mainModule, automate);
                }
                catch
                {
                    Console.WriteLine("Error analyzing OB blocks; Automate: " + automate.sName);
                }

                try
                {
                    CalculStatPLC(automate);
                }
                catch
                {
                    Console.WriteLine("Error calculating PLC statistics; Automate: " + automate.sName);
                }

                try
                {
                    PIDOB1(automate);
                }
                catch
                {
                    Console.WriteLine("Error checking PID in OB1; Automate: " + automate.sName);
                }

                try
                {
                    InterfaceNetwork(mainModule, automate, projet);
                    if (automate.sInterfaceX2 == null)
                    {
                        GetCpModule(automate, device, projet);
                    }
                }
                catch
                {
                    Console.WriteLine("Error retrieving network interface; Automate: " + automate.sName);
                }

                try
                {
                    GetSlaveVariator(automate);
                }
                catch
                {
                    Console.WriteLine("Error retrieving slave variator; Automate: " + automate.sName);
                }
                try
                {
                    GetSlaveInOut(automate);
                }
                catch
                {
                    Console.WriteLine("Error retrieving InOuts; Automate: " + automate.sName);
                }

                return automate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing security properties: {ex.Message} {device.Name}");
                return null;
            }
        }

        /// <summary>
        /// Finds and returns the main module (CPU) from a given device.
        /// </summary>
        /// <param name="device">The device to search for the main module.</param>
        /// <returns>
        /// The main <see cref="DeviceItem"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public DeviceItem FindMainModule(Device device)
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
        /// <summary>
        /// Creates and populates a <see cref="MyAutomate"/> object with detailed attributes extracted from the specified PLC main module.
        /// </summary>
        /// <param name="mainModule">
        /// The <see cref="DeviceItem"/> representing the main module of the PLC from which information will be extracted.
        /// </param>
        /// <returns>
        /// A <see cref="MyAutomate"/> instance filled with key hardware and configuration details (such as name, type, reference, firmware, watchdog, web server, restart behavior, memory, timezone, access control, MMC status, display access, and online connectivity).
        /// If the PLC type is not within the authorized list, its range is marked as "Out of range".
        /// </returns>
        /// <remarks>
        /// This method attempts to retrieve a wide range of attributes from the main PLC module and its device items, including core metadata, memory and web server settings, security and access control, MMC diagnostics, display write access, and online access status.
        /// Robust error handling is employed: if an attribute is unavailable, a default message is set instead. Only PLC types “S7-1200”, “S7-1500”, and “ET 200SP” are considered authorized; others are flagged accordingly.
        /// </remarks>
        private MyAutomate CreateAutomateFromModule(DeviceItem mainModule)
        {
            string gamme = FindGamme(mainModule);
            string[] gammesAutorisees = { "S7-1200", "S7-1500", "ET 200SP" };

            var automate = new MyAutomate
            {
                sName = mainModule.GetAttribute("Name").ToString(),
                sGamme = gamme,
                sReference = mainModule.GetAttribute("OrderNumber").ToString(),
                sFirmware = mainModule.GetAttribute("FirmwareVersion").ToString()
            };

            try
            {
                automate.sWatchDog = mainModule.GetAttribute("CycleMaximumCycleTime").ToString();
            }
            catch
            {
                automate.sWatchDog = "Not found";
            }
            try
            {
                automate.sWebServer = mainModule.GetAttribute("WebserverActivate").ToString();
            }
            catch
            {
                automate.sWebServer = "Not found";
            }
            try
            {
                automate.sRestart = mainModule.GetAttribute("StartupActionAfterPowerOn").ToString();
            }
            catch
            {
                automate.sRestart = "Not found";
            }
            try
            {
                automate.sCadenceM0 = mainModule.GetAttribute("SystemMemoryByte").ToString();
            }
            catch
            {
                automate.sCadenceM0 = "Not found";
            }
            try
            {
                automate.sCadenceM1 = mainModule.GetAttribute("ClockMemoryByte").ToString();
            }
            catch
            {
                automate.sCadenceM1 = "Not found";
            }
            try
            {
                automate.sLocalHour = mainModule.GetAttribute("TimeOfDayLocalTimeZone").ToString();
            }
            catch
            {
                automate.sLocalHour = "Not found";
            }
            try
            {
                automate.sHourChange = mainModule.GetAttribute("TimeOfDayActivateDaylightSavingTime").ToString();
            }
            catch
            {
                automate.sHourChange = "Not found";
            }

            if (!gammesAutorisees.Any(g => gamme.Contains(g)))
            {
                automate.sName = mainModule.GetAttribute("Name").ToString();
                automate.sGamme = gamme + " | Out of range";
            }

            try
            {
                PlcAccessControlConfigurationProvider provider = mainModule.GetService<PlcAccessControlConfigurationProvider>();
                if (provider != null)
                {
                    var configuration = provider.GetAttribute("PlcAccessControlConfiguration");
                    automate.sControlAccess = configuration.ToString();
                }
                else automate.sControlAccess = "Option not available";
            }
            catch
            {
                automate.sControlAccess = "Option not available";
            }

            foreach (var item in mainModule.DeviceItems)
            {
                var itemName = item.GetAttribute("Name").ToString();
                if (itemName.Contains("Dispositif de lecture") || itemName.Contains("Card reader"))
                {
                    try
                    {
                        if (item.GetAttribute("DiagnosticsAgingSimaticMemoryCard").ToString() == "True")
                        {
                            automate.sMMCLife = item.GetAttribute("DiagnosticsAgingSimaticMemoryCardThreshold").ToString();
                        }
                        else automate.sMMCLife = "Option disabled";
                    }
                    catch
                    {
                        automate.sMMCLife = "Not found";
                    }
                }
                try
                {
                    if (itemName.Contains("Ecran") || itemName.Contains("display"))
                    {
                        automate.sScreenWrite = item.GetAttribute("DisplayWriteAccess").ToString();
                    }
                    else automate.sScreenWrite = "Option not available";
                }
                catch
                {
                    automate.sScreenWrite = "Option not available";
                }

                try
                {
                    automate.sOnlineAccess = TestOnlineAccess(mainModule);
                }
                catch
                {
                    automate.sOnlineAccess = "Connection unavailable";
                }
            }

            return automate;
        }
        /// <summary>
        /// Determines the product range (series) of the specified main module.
        /// </summary>
        /// <param name="mainModule">The main <see cref="DeviceItem"/> for which to find the range.</param>
        /// <returns>
        /// A <see cref="string"/> representing the range of the device (e.g., "S7-1200", "S7-1500"), or "Non trouvé" if not found.
        /// </returns>
        private string FindGamme(DeviceItem mainModule)
        {
            string sGamme = "Non trouvé";
            Device parent = (Device)mainModule.Parent;
            sGamme = parent.GetAttribute("TypeName").ToString();
            return sGamme;
        }
        /// <summary>
        /// Tests the online accessibility of the given main module.
        /// </summary>
        /// <param name="mainModule">The <see cref="DeviceItem"/> to test online access for.</param>
        /// <returns>
        /// "True" if the online connection can be established; otherwise, "False".
        /// </returns>
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
                else
                {
                    return "False";
                }
                onlineProvider.GoOffline();
                return "True";
            }
            catch
            {
                OnlineProvider onlineProvider = mainModule.GetService<OnlineProvider>();
                onlineProvider.GoOffline();
                return "False";
            }
        }
        /// <summary>
        /// Retrieves all blocks from the specified PLC main module and updates the current CPU blocks collection.
        /// </summary>
        /// <param name="mainModule">
        /// The <see cref="DeviceItem"/> representing the main module of the PLC from which to retrieve all blocks.
        /// </param>
        /// <remarks>
        /// This method clears the current CPU blocks collection and replaces it with the blocks obtained from the provided main module,
        /// by invoking <c>GetAllBlocksFromCPU</c>. It is primarily used to refresh the internal list of blocks associated with the active PLC.
        /// </remarks>
        private void GetAllBlocksFromPlc(DeviceItem mainModule)
        {
            oCurrentCpuBloks.Clear();
            oCurrentCpuBloks = GetAllBlocksFromCPU(mainModule);
        }
        /// <summary>
        /// Analyzes OB and FC blocks from the specified main module, and populates the automate with the related information.
        /// </summary>
        /// <param name="mainModule">The <see cref="DeviceItem"/> representing the CPU/main module.</param>
        /// <param name="automate">The <see cref="MyAutomate"/> object to be populated with block analysis results.</param>
        /// <remarks>
        /// This method analyzes all Function (FC) and Organization Block (OB) objects, extracts their cross-references, 
        /// and builds up the internal structure of the automate including nested and referenced blocks.
        /// </remarks>
        private void AnalyzeObBlocks(DeviceItem mainModule, MyAutomate automate)
        {
            automate.sProgramProtection = "No program protection";
            foreach (var block in oCurrentCpuBloks)
            {
                if (bool.Parse(block.GetAttribute("IsKnowHowProtected").ToString()))
                    automate.AddProtectedBloc(block.GetAttribute("Name").ToString());
                if (block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.FC")
                {
                    var vFcNumber = int.Parse(block.GetAttribute("Number").ToString());
                    var vFcObject = new MyFC
                    {
                        sName = block.GetAttribute("Name").ToString(),
                        iID = vFcNumber,
                        sType = "FC",
                    };

                    var crossRefServiceFC = block.GetService<CrossReferenceService>();
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
                            if (!(location.GetAttribute("ReferenceType").ToString() == "UsedBy"))
                            {
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
                var blocsACopier = fc.oInternalBlocs.ToList();

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
                                fc.oInternalBlocs.Remove(bloc);
                            }
                        }
                    }
                }
            }

            foreach (var block in oCurrentCpuBloks)
            {
                if (block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.OB")
                {
                    var obNumber = int.Parse(block.GetAttribute("Number").ToString());
                    var vObObject = new MyOB
                    {
                        sName = block.GetAttribute("Name").ToString(),
                        iID = obNumber
                    };

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
                            foreach (var fc in automate.oFCs)
                            {
                                if (fc.sName == myBloc.sName)
                                {
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
        /// <summary>
        /// Calculates and updates PLC statistics for the specified <see cref="MyAutomate"/> instance based on its OB (Organization Block) data.
        /// </summary>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> object whose OB statistics will be computed and updated.
        /// </param>
        /// <remarks>
        /// This method iterates through all OBs associated with the PLC to accumulate the total number of LTUs and blocks, 
        /// as well as to identify the number of blocks in OB1 and OB35. It then calculates the percentage of LTUs, OB1 blocks, 
        /// and OB35 blocks relative to the total block count. The computed values are assigned to the corresponding properties 
        /// of the <paramref name="automate"/> object. If there are no blocks, all statistics are set to zero.
        /// </remarks>
        private void CalculStatPLC(MyAutomate automate)
        {
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
            else
            {
                automate.iStandardLTU = (iNbLTU * 100) / iNbBloc;
                automate.iBlocOb1 = (iTotalBlocOB1 * 100) / iNbBloc;
                automate.iBlocOb35 = (iTotalBlocOB35 * 100) / iNbBloc;
            }
        }
        /// <summary>
        /// Appends formatted information about a block and its hierarchy to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">
        /// The <see cref="StringBuilder"/> instance to which the block information will be appended.
        /// </param>
        /// <param name="myBloc">
        /// The <see cref="MyBloc"/> object representing the block whose information and hierarchy are to be rendered.
        /// </param>
        /// <param name="niveau">
        /// The current indentation level, used to visually represent block hierarchy in the output.
        /// </param>
        /// <remarks>
        /// This method formats and appends the type and name of the provided block, using indentation to indicate hierarchy. 
        /// If the block is an <see cref="MyFC"/> (function block) with internal sub-blocks, the method recursively appends each sub-block's information, increasing the indentation level for each recursion.
        /// </remarks>
        private void AppendBlocInfo(StringBuilder sb, MyBloc myBloc, int niveau)
        {
            string indentation = new string('\t', niveau);
            sb.AppendLine($"{indentation}>---{myBloc.sType} : {myBloc.sName}");

            if (myBloc is MyFC fcBloc)
            {
                var internalBlocs = fcBloc.oInternalBlocs.ToList();

                foreach (var subBloc in internalBlocs)
                {
                    AppendBlocInfo(sb, subBloc, niveau + 1);
                }
            }
        }
        /// <summary>
        /// Retrieves all PLC blocks from the specified CPU device item, including blocks from user-defined groups.
        /// </summary>
        /// <param name="cpuDeviceItem">
        /// The <see cref="DeviceItem"/> representing the CPU from which to extract all PLC blocks.
        /// </param>
        /// <returns>
        /// A list of <see cref="PlcBlock"/> objects containing all blocks found in the CPU and its user groups.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the <see cref="PlcSoftware"/> cannot be retrieved from the specified device item.
        /// </exception>
        /// <remarks>
        /// This method obtains the <see cref="PlcSoftware"/> associated with the CPU, collects all direct blocks,
        /// and recursively gathers blocks from all user-defined block groups using <c>GatherBlocksFromGroup</c>.
        /// </remarks>
        public List<PlcBlock> GetAllBlocksFromCPU(DeviceItem cpuDeviceItem)
        {
            var allBlocks = new List<PlcBlock>();

            var softwareContainer = cpuDeviceItem.GetService<SoftwareContainer>();
            var plcSoftware = softwareContainer?.Software as PlcSoftware ?? throw new Exception("Unable to retrieve PlcSoftware for the specified CPU.");

            allBlocks.AddRange(plcSoftware.BlockGroup.Blocks);

            foreach (PlcBlockUserGroup userGroup in plcSoftware.BlockGroup.Groups)
            {
                GatherBlocksFromGroup(userGroup, allBlocks);
            }

            return allBlocks;
        }
        /// <summary>
        /// Recursively gathers all <see cref="PlcBlock"/> objects from the specified group and its subgroups.
        /// </summary>
        /// <param name="group">The <see cref="PlcBlockUserGroup"/> to search.</param>
        /// <param name="allBlocks">The list to which found blocks are added.</param>
        private void GatherBlocksFromGroup(PlcBlockUserGroup group, List<PlcBlock> allBlocks)
        {
            foreach (PlcBlock block in group.Blocks)
            {
                allBlocks.Add(block);
            }

            foreach (PlcBlockUserGroup subGroup in group.Groups)
            {
                GatherBlocksFromGroup(subGroup, allBlocks);
            }
        }
        /// <summary>
        /// Calculates the percentage of LTU blocks in the specified <see cref="MyOB"/> block list.
        /// </summary>
        /// <param name="myOb">The <see cref="MyOB"/> object for which to calculate the percentage.</param>
        /// <returns>
        /// A <see cref="double"/> representing the LTU block percentage. Returns 0.0 if no blocks are present.
        /// </returns>
        public double CalculerPourcentageDeBlocsLTU(MyOB myOb)
        {
            if (myOb == null || myOb.oBlocList == null || myOb.oBlocList.Count == 0) return 0.0;

            int totalBlocs = 0;
            int ltuCount = 0;

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

            if (totalBlocs == 0) return 0.0;
            else return (double)ltuCount / totalBlocs * 100.0;
        }
        /// <summary>
        /// Searches for a PID block within OB1 among the OBs of the specified <see cref="MyAutomate"/> instance and updates its PID information.
        /// </summary>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> object whose OB1 block list will be searched for a PID block.
        /// </param>
        /// <remarks>
        /// This method iterates through the OBs of the provided <paramref name="automate"/> to locate OB1 (identified by <c>iID == 1</c>).
        /// It then examines each block in OB1 for a name containing "PID". If found, the name is assigned to <c>sOB1PID</c>; otherwise,
        /// <c>sOB1PID</c> is set to "No PID block found".
        /// </remarks>
        private void PIDOB1(MyAutomate automate)
        {
            automate.sOB1PID = "No PID block found";
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
        /// <summary>
        /// Extracts and sets the network interface information (PROFINET) for the specified PLC, populating its IP, VLAN, and NTP server attributes.
        /// Also updates the associated project's network connection list with found interfaces.
        /// </summary>
        /// <param name="mainModule">
        /// The <see cref="DeviceItem"/> representing the main PLC module whose network interfaces are to be analyzed.
        /// </param>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> instance to receive interface, VLAN, and NTP server information.
        /// </param>
        /// <param name="projet">
        /// The <see cref="MyProject"/> instance whose connections list (<c>oConnexions</c>) will be updated with the discovered connections and devices.
        /// </param>
        /// <remarks>
        /// This method inspects all device items for PROFINET interfaces (X1 and X2), extracting their IP addresses and VLANs (subnets).
        /// For X1, it attempts to read the NTP configuration (up to three servers) and handles various error and disabled states with clear messages.
        /// It ensures that the <paramref name="projet"/>'s connection list is properly updated, creating new connections if needed,
        /// or adding the device to existing ones based on VLAN or IP range heuristics (e.g., "Reseau_Usine").
        /// All attributes are set with appropriate fallback values if not found.
        /// </remarks>
        private void InterfaceNetwork(DeviceItem mainModule, MyAutomate automate, MyProject projet)
        {
            foreach (var item in mainModule.DeviceItems)
            {
                if (item.GetAttribute("Name").ToString().Contains("PROFINET"))
                {
                    if (item.GetAttribute("Name").ToString().Contains("1"))
                    {
                        var networkInterface = item.GetService<NetworkInterface>();
                        try
                        {
                            if (networkInterface?.GetAttribute("TimeSynchronizationNtp").ToString() == "True")
                            {
                                if (networkInterface?.GetAttribute("TimeSynchronizationServer1").ToString() != "")
                                {
                                    automate.sNtpServer1 = networkInterface?.GetAttribute("TimeSynchronizationServer1").ToString();
                                }
                                else automate.sNtpServer1 = "Non";
                                if (networkInterface?.GetAttribute("TimeSynchronizationServer2").ToString() != "")
                                {
                                    automate.sNtpServer2 = networkInterface?.GetAttribute("TimeSynchronizationServer2").ToString();
                                }
                                else automate.sNtpServer2 = "Non";
                                if (networkInterface?.GetAttribute("TimeSynchronizationServer3").ToString() != "")
                                {
                                    automate.sNtpServer3 = networkInterface?.GetAttribute("TimeSynchronizationServer3").ToString();
                                }
                                else automate.sNtpServer3 = "Non";
                            }
                            else
                            {
                                automate.sNtpServer1 = "Option désactivée";
                                automate.sNtpServer2 = "Option désactivée";
                                automate.sNtpServer3 = "Option désactivée";
                            }
                        }
                        catch
                        {
                            automate.sNtpServer1 = "Non trouvé";
                            automate.sNtpServer2 = "Non trouvé";
                            automate.sNtpServer3 = "Non trouvé";
                        }

                        var node = networkInterface.Nodes.FirstOrDefault();

                        automate.sInterfaceX1 = node?.GetAttribute("Address").ToString();

                        Subnet subnet = node?.GetAttribute("ConnectedSubnet") as Subnet;

                        if (subnet != null)
                        {
                            automate.sVlanX1 = subnet.GetAttribute("Name").ToString();
                            string sNameConnectedDevice = automate.sName + "/X1";

                            MyConnexion connexion = projet.oConnexions.Find(oConnexion => oConnexion.sName == automate.sVlanX1);
                            MyConnexion connexion2 = projet.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                            if (connexion != null)
                            {
                                connexion.AddConnectedDevice(sNameConnectedDevice);
                            }
                            else if ((automate.sInterfaceX1.Contains("10.127.") || automate.sInterfaceX1.Contains("172.29.")) && connexion2 != null)
                            {
                                connexion2.AddConnectedDevice(sNameConnectedDevice);
                            }
                            else
                            {
                                string connexionName = automate.sVlanX1;
                                if (automate.sInterfaceX1.Contains("10.127.") || automate.sInterfaceX1.Contains("172.29."))
                                {
                                    connexionName = "Reseau_Usine";
                                }
                                projet.AddConnexion(new MyConnexion
                                {
                                    sName = connexionName,
                                    oConnectedDevices = new List<string> { sNameConnectedDevice }
                                });
                            }
                        }
                        else
                        {
                            automate.sVlanX1 = "Pas de VLAN";

                        }
                    }
                    else if (item.GetAttribute("Name").ToString().Contains("2"))
                    {
                        var networkInterface = item.GetService<NetworkInterface>();
                        var node = networkInterface.Nodes.FirstOrDefault();

                        automate.sInterfaceX2 = node?.GetAttribute("Address").ToString();
                        string sNameConnectedDevice = automate.sName + "/X2";

                        Subnet subnet2 = node?.GetAttribute("ConnectedSubnet") as Subnet;
                        if (subnet2 != null)
                        {
                            automate.sVlanX2 = subnet2.GetAttribute("Name").ToString();

                            MyConnexion connexion = projet.oConnexions.Find(oConnexion => oConnexion.sName == automate.sVlanX2);
                            MyConnexion connexion2 = projet.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                            if (connexion != null)
                            {
                                connexion.AddConnectedDevice(sNameConnectedDevice);
                            }
                            else if ((automate.sInterfaceX2.Contains("10.127.") || automate.sInterfaceX2.Contains("172.29.")) && connexion2 != null)
                            {
                                connexion2.AddConnectedDevice(sNameConnectedDevice);
                            }
                            else
                            {
                                string connexionName = automate.sVlanX2;
                                if (automate.sInterfaceX2.Contains("10.127.") || automate.sInterfaceX2.Contains("172.29."))
                                {
                                    connexionName = "Reseau_Usine";
                                }
                                projet.AddConnexion(new MyConnexion
                                {
                                    sName = connexionName,
                                    oConnectedDevices = new List<string> { sNameConnectedDevice }
                                });
                            }
                        }
                        else
                        {
                            automate.sVlanX2 = "Pas de VLAN";
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Associates all slave variators with the specified <see cref="MyAutomate"/> instance, based on master name matching.
        /// </summary>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> object to which slave variators will be added if their <c>sMasterName</c> matches the automate's name.
        /// </param>
        /// <remarks>
        /// This method iterates through the global <c>oVariators</c> collection, and for each variator whose <c>sMasterName</c> matches the automate's name,
        /// adds it to the automate using <c>AddVariator</c>. If no matching variators are found, no changes are made.
        /// </remarks>
        private void GetSlaveVariator(MyAutomate automate)
        {
            if (oVariators.Find(oVariators => oVariators.sMasterName == automate.sName) != null)
            {
                foreach (MyVariator item in oVariators)
                {
                    if (item.sMasterName == automate.sName)
                    {
                        automate.AddVariator(item);
                    }
                }
            }
        }
        /// <summary>
        /// Associates all slave I/O modules with the specified <see cref="MyAutomate"/> instance, based on master name matching.
        /// </summary>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> object to which slave I/O modules will be added if their <c>sMasterName</c> matches the automate's name.
        /// </param>
        /// <remarks>
        /// This method iterates through the global <c>oInOuts</c> collection, and for each I/O module whose <c>sMasterName</c> matches the automate's name,
        /// adds it to the automate using <c>AddInOut</c>. If no matching I/O modules are found, no changes are made.
        /// </remarks>
        private void GetSlaveInOut(MyAutomate automate)
        {
            if (oInOuts.Find(oInOuts => oInOuts.sMasterName == automate.sName) != null)
            {
                foreach (MyInOut item in oInOuts)
                {
                    if (item.sMasterName == automate.sName)
                    {
                        automate.AddInOut(item);
                    }
                }
            }
        }
        /// <summary>
        /// Extracts and sets secondary network interface (X2) information for the specified PLC, using CP (Communication Processor) modules found in the parent device.
        /// Also updates the associated project's network connection list with the discovered connection.
        /// </summary>
        /// <param name="automate">
        /// The <see cref="MyAutomate"/> instance to receive interface X2 and VLAN X2 information.
        /// </param>
        /// <param name="parentDevice">
        /// The parent <see cref="Device"/> containing device items, from which CP modules will be searched.
        /// </param>
        /// <param name="project">
        /// The <see cref="MyProject"/> instance whose connections list (<c>oConnexions</c>) will be updated with the discovered connection and device.
        /// </param>
        /// <remarks>
        /// This method iterates through device items of the parent device to find CP modules. For each CP module, it checks child items for a label containing "X1",
        /// retrieves network interface details, and updates the automate's X2 interface address and VLAN. It ensures the project’s connection list is updated accordingly,
        /// matching on VLAN name or IP patterns for factory networks ("10.127." or "172.29."). In case of missing VLAN or errors, fallback values are set and iteration continues.
        /// </remarks>
        private void GetCpModule(MyAutomate automate, Device parentDevice, MyProject project)
        {
            foreach (var item in parentDevice.DeviceItems)
            {
                if (item.GetAttribute("TypeName").ToString().Split(' ')[0] == "CP")
                {
                    foreach (var item2 in item.DeviceItems)
                    {
                        try
                        {
                            string test = item2.GetAttribute("Label").ToString();
                            if (test.Contains("X1"))
                            {
                                NetworkInterface networkInterface = item2.GetService<NetworkInterface>();
                                var node = networkInterface.Nodes.FirstOrDefault();
                                if (node != null)
                                {
                                    automate.sInterfaceX2 = "Via CP : " + node?.GetAttribute("Address").ToString();
                                    string sNameConnectedDevice = automate.sName + "/X2";

                                    Subnet subnet2 = node?.GetAttribute("ConnectedSubnet") as Subnet;
                                    if (subnet2 != null)
                                    {
                                        automate.sVlanX2 = subnet2.GetAttribute("Name").ToString();

                                        MyConnexion connexion = project.oConnexions.Find(oConnexion => oConnexion.sName == automate.sVlanX2);
                                        MyConnexion connexion2 = project.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                                        if (connexion != null)
                                        {
                                            connexion.AddConnectedDevice(sNameConnectedDevice);
                                        }
                                        else if ((automate.sInterfaceX2.Contains("10.127.") || automate.sInterfaceX2.Contains("172.29.")) && connexion2 != null)
                                        {
                                            connexion2.AddConnectedDevice(sNameConnectedDevice);
                                        }
                                        else
                                        {
                                            string connexionName = automate.sVlanX2;
                                            if (automate.sInterfaceX2.Contains("10.127.") || automate.sInterfaceX2.Contains("172.29."))
                                            {
                                                connexionName = "Reseau_Usine";
                                            }
                                            project.AddConnexion(new MyConnexion
                                            {
                                                sName = connexionName,
                                                oConnectedDevices = new List<string> { sNameConnectedDevice }
                                            });
                                        }
                                    }
                                    else
                                    {
                                        automate.sVlanX2 = "Pas de VLAN";
                                    }
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Extracts all HMI devices from the TIA project and adds them to the specified <see cref="MyProject"/> instance, including network interface and VLAN details.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> instance representing the TIA project interface containing devices.
        /// </param>
        /// <param name="project">
        /// The <see cref="MyProject"/> instance to which detected HMI devices and their network information will be added.
        /// </param>
        /// <remarks>
        /// This method searches all devices in the TIA project for those whose name contains "HMI". For each matching device item with a valid <c>TypeIdentifier</c>,
        /// it creates a <see cref="MyHmi"/> object and extracts its name, reference, and firmware version. The method then searches for network interfaces labeled "X1" and "X2",
        /// populates the HMI's interface addresses and VLANs, and updates the project's connection list accordingly. Fallback values are provided if VLANs are not found.
        /// Robust error handling ensures the process continues even if some attributes are missing or exceptions are thrown.
        /// </remarks>
        public void GetHmiProject(HMATIAOpenness_V16 tiaInterface, MyProject project)
        {
            foreach (var device in tiaInterface.m_oTiaProject.Devices)
            {
                if (device.GetAttribute("Name").ToString().Contains("HMI"))
                {
                    foreach (var deviceItem in device.DeviceItems)
                    {
                        try
                        {
                            if (deviceItem.GetAttribute("TypeIdentifier").ToString().Count() > 1)
                            {
                                MyHmi ihm = new MyHmi
                                {
                                    sName = deviceItem.GetAttribute("Name").ToString(),
                                };
                                string sTemp = deviceItem.GetAttribute("TypeIdentifier").ToString().Split(':')[1];
                                ihm.sReference = sTemp.Split('/')[0].Trim();
                                ihm.sFirmware = sTemp.Split('/')[1].Trim();
                                project.AddHMI(ihm);

                                //Interface Réseau
                                foreach (var deviceItem2 in device.DeviceItems)
                                {
                                    foreach (var deviceItem3 in deviceItem2.DeviceItems)
                                    {
                                        try
                                        {
                                            if (deviceItem3.GetAttribute("Label").ToString().Equals("X1"))
                                            {
                                                var networkInterface = deviceItem3.GetService<NetworkInterface>();

                                                foreach (var node in networkInterface.Nodes)
                                                {
                                                    ihm.sInterfaceX1 = node.GetAttribute("Address").ToString();
                                                    Subnet subnet = node.GetAttribute("ConnectedSubnet") as Subnet;
                                                    if (subnet != null)
                                                    {
                                                        ihm.sVlanX1 = subnet.GetAttribute("Name").ToString();

                                                        string sNameConnectedDevice = ihm.sName + "/X1";

                                                        MyConnexion connexion = project.oConnexions.Find(oConnexion => oConnexion.sName == ihm.sVlanX1);
                                                        MyConnexion connexion2 = project.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                                                        if (connexion != null)
                                                        {
                                                            connexion.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else if ((ihm.sInterfaceX1.Contains("10.127.") || ihm.sInterfaceX1.Contains("172.29.")) && connexion2 != null)
                                                        {
                                                            connexion2.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else
                                                        {
                                                            string connexionName = ihm.sVlanX1;
                                                            if (ihm.sInterfaceX1.Contains("10.127.") || ihm.sInterfaceX1.Contains("172.29."))
                                                            {
                                                                connexionName = "Reseau_Usine";
                                                            }
                                                            project.AddConnexion(new MyConnexion
                                                            {
                                                                sName = connexionName,
                                                                oConnectedDevices = new List<string> { sNameConnectedDevice }
                                                            });

                                                        }
                                                    }
                                                    else
                                                    {
                                                        ihm.sVlanX1 = "Pas de VLAN";
                                                    }
                                                }
                                            }
                                            else if (deviceItem3.GetAttribute("Label").ToString().Equals("X2"))
                                            {
                                                var networkInterface = deviceItem3.GetService<NetworkInterface>();

                                                foreach (var node in networkInterface.Nodes)
                                                {
                                                    ihm.sInterfaceX2 = node.GetAttribute("Address").ToString();
                                                    Subnet subnet = node.GetAttribute("ConnectedSubnet") as Subnet;
                                                    if (subnet != null)
                                                    {
                                                        ihm.sVlanX2 = subnet.GetAttribute("Name").ToString();

                                                        string sNameConnectedDevice = ihm.sName + "/X2";

                                                        MyConnexion connexion = project.oConnexions.Find(oConnexion => oConnexion.sName == ihm.sVlanX2);
                                                        MyConnexion connexion2 = project.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                                                        if (connexion != null)
                                                        {
                                                            connexion.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else if ((ihm.sInterfaceX2.Contains("10.127.") || ihm.sInterfaceX2.Contains("172.29.")) && connexion2 != null)
                                                        {
                                                            connexion2.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else
                                                        {
                                                            string connexionName = ihm.sVlanX2;
                                                            if (ihm.sInterfaceX2.Contains("10.127.") || ihm.sInterfaceX2.Contains("172.29."))
                                                            {
                                                                connexionName = "Reseau_Usine";
                                                            }
                                                            project.AddConnexion(new MyConnexion
                                                            {
                                                                sName = connexionName,
                                                                oConnectedDevices = new List<string> { sNameConnectedDevice }
                                                            });
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ihm.sVlanX2 = "Pas de VLAN";
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Extracts all SCADA devices from the TIA project and adds them to the specified <see cref="MyProject"/> instance, including network interface and VLAN details.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> instance representing the TIA project interface containing devices.
        /// </param>
        /// <param name="project">
        /// The <see cref="MyProject"/> instance to which detected SCADA devices and their network information will be added.
        /// </param>
        /// <remarks>
        /// This method searches all devices in the TIA project for those whose type name contains "SIMATIC PC". For each matching device item with a
        /// <c>TypeIdentifier</c> containing "6AV", it creates a <see cref="MyScada"/> object and extracts its name, reference, and firmware version.
        /// The method then searches for network interfaces labeled "X1", populates the SCADA's interface address and VLAN, and updates the project's
        /// connection list accordingly. Fallback values are provided if VLANs are not found. Robust error handling ensures the process continues
        /// even if some attributes are missing or exceptions are thrown.
        /// </remarks>
        public void GetScadaProject(HMATIAOpenness_V16 tiaInterface, MyProject project)
        {
            foreach (var device in tiaInterface.m_oTiaProject.Devices)
            {
                if (device.GetAttribute("TypeName").ToString().Contains("SIMATIC PC"))
                {
                    foreach (var deviceItem in device.DeviceItems)
                    {
                        try
                        {
                            if (deviceItem.GetAttribute("TypeIdentifier").ToString().Contains("6AV"))
                            {
                                MyScada scada = new MyScada
                                {
                                    sName = deviceItem.GetAttribute("Name").ToString(),
                                };
                                string sTemp = deviceItem.GetAttribute("TypeIdentifier").ToString().Split(':')[1];
                                scada.sReference = sTemp.Split('/')[0].Trim();
                                scada.sFirmware = sTemp.Split('/')[1].Trim();
                                project.AddSCADA(scada);

                                foreach (var deviceItem2 in device.DeviceItems)
                                {
                                    foreach (var deviceItem3 in deviceItem2.DeviceItems)
                                    {
                                        try
                                        {
                                            if (deviceItem3.GetAttribute("Label").ToString().Equals("X1"))
                                            {
                                                var networkInterface = deviceItem3.GetService<NetworkInterface>();

                                                foreach (var node in networkInterface.Nodes)
                                                {
                                                    scada.sInterfaceX1 = node.GetAttribute("Address").ToString();
                                                    Subnet subnet = node.GetAttribute("ConnectedSubnet") as Subnet;
                                                    if (subnet != null)
                                                    {
                                                        scada.sVlanX1 = subnet.GetAttribute("Name").ToString();

                                                        string sNameConnectedDevice = scada.sName + "/X1";

                                                        MyConnexion connexion = project.oConnexions.Find(oConnexion => oConnexion.sName == scada.sVlanX1);
                                                        MyConnexion connexion2 = project.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                                                        if (connexion != null)
                                                        {

                                                            connexion.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else if ((scada.sInterfaceX1.Contains("10.127.") || scada.sInterfaceX1.Contains("172.29.")) && connexion2 != null)
                                                        {
                                                            connexion2.AddConnectedDevice(sNameConnectedDevice);
                                                        }
                                                        else
                                                        {
                                                            string connexionName = scada.sVlanX1;
                                                            if (scada.sInterfaceX1.Contains("10.127.") || scada.sInterfaceX1.Contains("172.29."))
                                                            {
                                                                connexionName = "Reseau_Usine";
                                                            }
                                                            project.AddConnexion(new MyConnexion
                                                            {
                                                                sName = connexionName,
                                                                oConnectedDevices = new List<string> { sNameConnectedDevice }
                                                            });
                                                        }

                                                    }
                                                    else
                                                    {
                                                        scada.sVlanX1 = "Pas de VLAN";
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }

                                    }
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Extracts all SCALANCE switch devices from the TIA project and adds them to the specified <see cref="MyProject"/> instance, including network interface and VLAN details.
        /// </summary>
        /// <param name="tiaInterface">
        /// The <see cref="HMATIAOpenness_V16"/> instance representing the TIA project interface containing device groups.
        /// </param>
        /// <param name="project">
        /// The <see cref="MyProject"/> instance to which detected switch devices and their network information will be added.
        /// </param>
        /// <remarks>
        /// This method searches all device groups and devices in the TIA project for those whose type name contains "SCALANCE". For each matching device item with a valid
        /// <c>OrderNumber</c>, it creates a <see cref="MySwitch"/> object and extracts its name, reference, product range, and firmware version. The method then searches for child device items whose type name contains "interface",
        /// populates the switch's interface address and VLAN, and updates the project's connection list accordingly. Fallbacks and error handling ensure the process continues even if some attributes are missing.
        /// </remarks>
        public void GetSwitchProject(HMATIAOpenness_V16 tiaInterface, MyProject project)
        {
            foreach (DeviceGroup deviceGroup in tiaInterface.m_oTiaProject.DeviceGroups)
            {
                foreach (Device device in deviceGroup.Devices)
                {
                    if (device.GetAttribute("TypeName").ToString().Contains("SCALANCE"))
                    {
                        foreach (DeviceItem deviceItem in device.DeviceItems)
                        {
                            if (deviceItem.GetAttribute("OrderNumber").ToString().Count() > 1)
                            {
                                MySwitch switchDevice = new MySwitch
                                {
                                    sName = deviceItem.GetAttribute("Name").ToString(),
                                    sReference = deviceItem.GetAttribute("OrderNumber").ToString(),
                                    sGamme = deviceItem.GetAttribute("TypeName").ToString(),
                                    sFirmware = deviceItem.GetAttribute("FirmwareVersion").ToString()
                                };

                                foreach (DeviceItem deviceItem1 in deviceItem.DeviceItems)
                                {
                                    Console.WriteLine(deviceItem1.GetAttribute("TypeName").ToString());
                                    if (deviceItem1.GetAttribute("TypeName").ToString().Contains("interface"))
                                    {
                                        var networkInterface = deviceItem1.GetService<NetworkInterface>();
                                        var node = networkInterface.Nodes.FirstOrDefault();
                                        switchDevice.sInterfaceX1 = node?.GetAttribute("Address").ToString();
                                        Subnet subnet = node?.GetAttribute("ConnectedSubnet") as Subnet;
                                        if (subnet != null)
                                        {
                                            switchDevice.sVlanX1 = subnet.GetAttribute("Name").ToString();

                                            string sNameConnectedDevice = switchDevice.sName + "/X1";

                                            MyConnexion connexion = project.oConnexions.Find(oConnexion => oConnexion.sName == switchDevice.sVlanX1);
                                            MyConnexion connexion2 = project.oConnexions.Find(oConnexion => oConnexion.sName == "Reseau_Usine");
                                            if (connexion != null)
                                            {

                                                connexion.AddConnectedDevice(sNameConnectedDevice);
                                            }
                                            else if ((switchDevice.sInterfaceX1.Contains("10.127.") || switchDevice.sInterfaceX1.Contains("172.29.")) && connexion2 != null)
                                            {
                                                connexion2.AddConnectedDevice(sNameConnectedDevice);
                                            }
                                            else
                                            {
                                                string connexionName = switchDevice.sVlanX1;
                                                if (switchDevice.sInterfaceX1.Contains("10.127.") || switchDevice.sInterfaceX1.Contains("172.29."))
                                                {
                                                    connexionName = "Reseau_Usine";
                                                }
                                                project.AddConnexion(new MyConnexion
                                                {
                                                    sName = connexionName,
                                                    oConnectedDevices = new List<string> { sNameConnectedDevice }
                                                });
                                            }
                                        }
                                    }
                                }
                                project.AddSwitch(switchDevice);
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Represents a project containing PLCs, HMIs, SCADAs, switches, network connections, and related project information.
    /// </summary>
    public class MyProject
    {
        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the project file path.
        /// </summary>
        public string sProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the version of the project.
        /// </summary>
        public string sVersion { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the project.
        /// </summary>
        public string sDateCreation { get; set; }

        /// <summary>
        /// Gets or sets the reference language of the project.
        /// </summary>
        public string sLanguage { get; set; }

        /// <summary>
        /// Gets or sets the project file size.
        /// </summary>
        public string sSize { get; set; }

        /// <summary>
        /// Gets or sets whether simulation is enabled during block compilation.
        /// </summary>
        public string sSimulation { get; set; }

        /// <summary>
        /// Gets the list of PLC automates in the project.
        /// </summary>
        public List<MyAutomate> oAutomates;

        /// <summary>
        /// Gets the list of HMIs in the project.
        /// </summary>
        public List<MyHmi> oHMIs;

        /// <summary>
        /// Gets the list of SCADAs in the project.
        /// </summary>
        public List<MyScada> oSCADAs;

        /// <summary>
        /// Gets the list of switches in the project.
        /// </summary>
        public List<MySwitch> oSwitchs;

        /// <summary>
        /// Gets the list of network connections in the project.
        /// </summary>
        public List<MyConnexion> oConnexions;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyProject"/> class.
        /// </summary>
        public MyProject()
        {
            oAutomates = new List<MyAutomate>();
            oHMIs = new List<MyHmi>();
            oSCADAs = new List<MyScada>();
            oSwitchs = new List<MySwitch>();
            oConnexions = new List<MyConnexion>();
        }

        /// <summary>
        /// Adds a PLC automate to the project.
        /// </summary>
        /// <param name="automate">The <see cref="MyAutomate"/> to add.</param>
        public void AddAutomate(MyAutomate automate)
        {
            oAutomates.Add(automate);
        }

        /// <summary>
        /// Adds an HMI to the project.
        /// </summary>
        /// <param name="hmi">The <see cref="MyHmi"/> to add.</param>
        public void AddHMI(MyHmi hmi)
        {
            oHMIs.Add(hmi);
        }

        /// <summary>
        /// Adds a SCADA to the project.
        /// </summary>
        /// <param name="scada">The <see cref="MyScada"/> to add.</param>
        public void AddSCADA(MyScada scada)
        {
            oSCADAs.Add(scada);
        }

        /// <summary>
        /// Adds a switch to the project.
        /// </summary>
        /// <param name="switchItem">The <see cref="MySwitch"/> to add.</param>
        public void AddSwitch(MySwitch switchItem)
        {
            oSwitchs.Add(switchItem);
        }

        /// <summary>
        /// Adds a network connection to the project.
        /// </summary>
        /// <param name="connexion">The <see cref="MyConnexion"/> to add.</param>
        public void AddConnexion(MyConnexion connexion)
        {
            oConnexions.Add(connexion);
        }
    }
    /// <summary>
    /// Represents a network or endpoint connection, including its name and the list of connected devices.
    /// </summary>
    public class MyConnexion
    {
        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets the list of device names connected to this connection.
        /// </summary>
        public List<string> oConnectedDevices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyConnexion"/> class.
        /// </summary>
        public MyConnexion()
        {
            oConnectedDevices = new List<string>();
        }

        /// <summary>
        /// Adds a device name to the list of connected devices.
        /// </summary>
        /// <param name="deviceName">The name of the device to add.</param>
        public void AddConnectedDevice(string deviceName)
        {
            oConnectedDevices.Add(deviceName);
        }
    }
    /// <summary>
    /// Represents a PLC automate with identification, network, configuration, and statistical properties.
    /// </summary>
    public class MyAutomate
    {
        #region Identification properties
        /// <summary>
        /// Gets or sets the automate name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the product range.
        /// </summary>
        public string sGamme { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the firmware version.
        /// </summary>
        public string sFirmware { get; set; }
        #endregion

        #region Time properties
        /// <summary>
        /// Gets or sets the first NTP server address.
        /// </summary>
        public string sNtpServer1 { get; set; }

        /// <summary>
        /// Gets or sets the second NTP server address.
        /// </summary>
        public string sNtpServer2 { get; set; }

        /// <summary>
        /// Gets or sets the third NTP server address.
        /// </summary>
        public string sNtpServer3 { get; set; }

        /// <summary>
        /// Gets or sets the local time zone.
        /// </summary>
        public string sLocalHour { get; set; }

        /// <summary>
        /// Gets or sets the daylight saving time change indicator.
        /// </summary>
        public string sHourChange { get; set; }
        #endregion

        #region Network properties
        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X1.
        /// </summary>
        public string sConnectedDeviceX1 { get; set; }

        /// <summary>
        /// Gets or sets the X2 interface address.
        /// </summary>
        public string sInterfaceX2 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X2.
        /// </summary>
        public string sConnectedDeviceX2 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X2.
        /// </summary>
        public string sVlanX2 { get; set; }
        #endregion

        #region System properties
        /// <summary>
        /// Gets or sets the MMC card lifetime threshold.
        /// </summary>
        public string sMMCLife { get; set; }

        /// <summary>
        /// Gets or sets the watchdog setting.
        /// </summary>
        public string sWatchDog { get; set; }

        /// <summary>
        /// Gets or sets the startup action after power on.
        /// </summary>
        public string sRestart { get; set; }

        /// <summary>
        /// Gets or sets the system memory byte (M0).
        /// </summary>
        public string sCadenceM0 { get; set; }

        /// <summary>
        /// Gets or sets the clock memory byte (M1).
        /// </summary>
        public string sCadenceM1 { get; set; }
        #endregion

        #region Program access properties
        /// <summary>
        /// Gets or sets the program protection information.
        /// </summary>
        public string sProgramProtection { get; set; }

        /// <summary>
        /// Gets or sets the web server activation state.
        /// </summary>
        public string sWebServer { get; set; }

        /// <summary>
        /// Gets or sets the PLC access control configuration.
        /// </summary>
        public string sControlAccess { get; set; }

        /// <summary>
        /// Gets or sets the HMI communication API state.
        /// </summary>
        public string sApiHmiCom { get; set; }

        /// <summary>
        /// Gets or sets the online access state.
        /// </summary>
        public string sOnlineAccess { get; set; }

        /// <summary>
        /// Gets or sets the screen write access state.
        /// </summary>
        public string sScreenWrite { get; set; }

        /// <summary>
        /// Gets or sets the instant variable status.
        /// </summary>
        public string sInstantVar { get; set; }
        #endregion

        #region Statistics properties
        /// <summary>
        /// Gets or sets the standard LTU percentage.
        /// </summary>
        public int iStandardLTU { get; set; }

        /// <summary>
        /// Gets or sets the name of the PID block found in OB1, if any.
        /// </summary>
        public string sOB1PID { get; set; }

        /// <summary>
        /// Gets or sets the percentage of blocks in OB1.
        /// </summary>
        public int iBlocOb1 { get; set; }

        /// <summary>
        /// Gets or sets the percentage of blocks in OB35.
        /// </summary>
        public int iBlocOb35 { get; set; }
        #endregion

        #region Tags

        /// <summary>
        /// Gets the list of input tags.
        /// </summary>
        public List<MyTag> oTagsIn { get; set; }

        /// <summary>
        /// Gets the list of output tags.
        /// </summary>
        public List<MyTag> oTagsOut { get; set; }

        /// <summary>
        /// Gets the list of memory tags.
        /// </summary>
        public List<MyTag> oTagsMem { get; set; }

        #endregion

        /// <summary>
        /// Gets the list of OBs for this automate.
        /// </summary>
        public List<MyOB> oOBs;

        /// <summary>
        /// Gets the list of FCs for this automate.
        /// </summary>
        public List<MyFC> oFCs;

        /// <summary>
        /// Gets the list of protected block names.
        /// </summary>
        public List<string> ProtectedBlocs;

        /// <summary>
        /// Gets the list of variators associated with this automate.
        /// </summary>
        public List<MyVariator> oVariators { get; set; }

        /// <summary>
        /// Gets the list of input/output modules associated with this automate.
        /// </summary>
        public List<MyInOut> oInOuts { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAutomate"/> class.
        /// </summary>
        public MyAutomate()
        {
            oOBs = new List<MyOB>();
            oFCs = new List<MyFC>();
            ProtectedBlocs = new List<string>();
            oTagsIn = new List<MyTag>();
            oTagsOut = new List<MyTag>();
            oTagsMem = new List<MyTag>();
            oVariators = new List<MyVariator>();
            oInOuts = new List<MyInOut>();
        }

        /// <summary>
        /// Adds an <see cref="MyOB"/> to the automate.
        /// </summary>
        /// <param name="oOb">The OB to add.</param>
        public void AddOb(MyOB oOb)
        {
            oOBs.Add(oOb);
        }

        /// <summary>
        /// Adds an <see cref="MyFC"/> to the automate.
        /// </summary>
        /// <param name="oFc">The FC to add.</param>
        public void AddFc(MyFC oFc)
        {
            oFCs.Add(oFc);
        }

        /// <summary>
        /// Adds a block name to the protected blocks list.
        /// </summary>
        /// <param name="blocName">The block name to add.</param>
        public void AddProtectedBloc(string blocName)
        {
            ProtectedBlocs.Add(blocName);
        }

        /// <summary>
        /// Adds an input tag to the automate.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTagIn(MyTag tag)
        {
            oTagsIn.Add(tag);
        }

        /// <summary>
        /// Adds an output tag to the automate.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTagOut(MyTag tag)
        {
            oTagsOut.Add(tag);
        }

        /// <summary>
        /// Adds a memory tag to the automate.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTagMem(MyTag tag)
        {
            oTagsMem.Add(tag);
        }

        /// <summary>
        /// Adds a variator to the automate.
        /// </summary>
        /// <param name="variator">The variator to add.</param>
        public void AddVariator(MyVariator variator)
        {
            oVariators.Add(variator);
        }

        /// <summary>
        /// Adds an input/output module to the automate.
        /// </summary>
        /// <param name="inout">The input/output module to add.</param>
        public void AddInOut(MyInOut inout)
        {
            oInOuts.Add(inout);
        }
    }
    /// <summary>
    /// Represents a generic PLC block (base class).
    /// </summary>
    public class MyBloc
    {
        /// <summary>
        /// Gets or sets the block name.
        /// </summary>
        public string sName { get; set; }
        /// <summary>
        /// Gets or sets the block type (e.g., OB, FC, FB).
        /// </summary>
        public string sType { get; set; }
        /// <summary>
        /// Gets or sets the network number.
        /// </summary>
        public int iNetwork { get; set; }
    }
    /// <summary>
    /// Represents an Organization Block (OB), inheriting from <see cref="MyBloc"/>.
    /// </summary>
    public class MyOB : MyBloc
    {
        /// <summary>
        /// Gets or sets the OB ID (number).
        /// </summary>
        public int iID { get; set; }
        /// <summary>
        /// Gets or sets the number of networks in this OB.
        /// </summary>
        public int iNbNetwork { get; set; }
        /// <summary>
        /// Gets or sets the LTU block percentage in this OB.
        /// </summary>
        public double Ltu100 { get; set; }
        /// <summary>
        /// Gets or sets the number of LTU blocks in this OB.
        /// </summary>
        public int iNbLTU { get; set; }
        /// <summary>
        /// Gets or sets the number of blocks in this OB.
        /// </summary>
        public int iNbBloc { get; set; }
        /// <summary>
        /// Gets the list of contained blocs (FC/FB/Instruction).
        /// </summary>
        public List<MyBloc> oBlocList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyOB"/> class.
        /// </summary>
        public MyOB()
        {
            oBlocList = new List<MyBloc>();
        }

        /// <summary>
        /// Adds a block to the OB's block list.
        /// </summary>
        /// <param name="bloc">The block to add.</param>
        public void AddBloc(MyBloc bloc)
        {
            oBlocList.Add(bloc);
        }
    }
    /// <summary>
    /// Represents a Function block (FC) inheriting from <see cref="MyBloc"/>.
    /// </summary>
    public class MyFC : MyBloc
    {
        /// <summary>
        /// Gets or sets the FC ID (number).
        /// </summary>
        public int iID { get; set; }
        /// <summary>
        /// Gets or sets the number of LTU blocks in this FC.
        /// </summary>
        public int iNbLTU { get; set; }
        /// <summary>
        /// Gets or sets the number of blocks in this FC.
        /// </summary>
        public int iNbBloc { get; set; }
        /// <summary>
        /// Gets the list of internal blocs (FCs, FBs, etc.) used inside this FC.
        /// </summary>
        public List<MyBloc> oInternalBlocs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyFC"/> class.
        /// </summary>
        public MyFC()
        {
            oInternalBlocs = new List<MyBloc>();
        }

        /// <summary>
        /// Adds a block to the FC's internal block list.
        /// </summary>
        /// <param name="bloc">The block to add.</param>
        public void AddBloc(MyBloc bloc)
        {
            oInternalBlocs.Add(bloc);
        }
    }
    /// <summary>
    /// Represents a tag (variable) used in a PLC program, including its name, type, address, and an optional comment.
    /// </summary>
    public class MyTag
    {
        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the data type of the tag (e.g., BOOL, INT, REAL).
        /// </summary>
        public string sType { get; set; }

        /// <summary>
        /// Gets or sets the address of the tag (e.g., "DB1.DBX0.0", "I0.0").
        /// </summary>
        public string sAddress { get; set; }

        /// <summary>
        /// Gets or sets the comment or description associated with the tag.
        /// </summary>
        public string sComment { get; set; }
    }
    /// <summary>
    /// Represents an HMI (Human-Machine Interface) device with identification, firmware, and network properties.
    /// </summary>
    public class MyHmi
    {
        /// <summary>
        /// Gets or sets the HMI name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the firmware version.
        /// </summary>
        public string sFirmware { get; set; }

        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X1.
        /// </summary>
        public string sConnectedDeviceX1 { get; set; }

        /// <summary>
        /// Gets or sets the X2 interface address.
        /// </summary>
        public string sInterfaceX2 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X2.
        /// </summary>
        public string sVlanX2 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X2.
        /// </summary>
        public string sConnectedDeviceX2 { get; set; }
    }
    /// <summary>
    /// Represents a SCADA (Supervisory Control and Data Acquisition) device, including identification, firmware, and network properties.
    /// </summary>
    public class MyScada
    {
        /// <summary>
        /// Gets or sets the SCADA device name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the product range or series.
        /// </summary>
        public string sGamme { get; set; }

        /// <summary>
        /// Gets or sets the firmware version.
        /// </summary>
        public string sFirmware { get; set; }

        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X1.
        /// </summary>
        public string sConnectedDeviceX1 { get; set; }

        /// <summary>
        /// Gets or sets the X2 interface address.
        /// </summary>
        public string sInterfaceX2 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X2.
        /// </summary>
        public string sVlanX2 { get; set; }

        /// <summary>
        /// Gets or sets the name of the device connected to X2.
        /// </summary>
        public string sConnectedDeviceX2 { get; set; }
    }
    /// <summary>
    /// Represents a variator (variable speed drive) device with identification and network properties.
    /// </summary>
    public class MyVariator
    {
        /// <summary>
        /// Gets or sets the name of the variator.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference of the variator.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the product range or series of the variator.
        /// </summary>
        public string sGamme { get; set; }

        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the master device associated with this variator.
        /// </summary>
        public string sMasterName { get; set; }
    }
    /// <summary>
    /// Represents a network switch device with identification, firmware, and network interface properties.
    /// </summary>
    public class MySwitch
    {
        /// <summary>
        /// Gets or sets the switch name.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference of the switch.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the product range or series of the switch.
        /// </summary>
        public string sGamme { get; set; }

        /// <summary>
        /// Gets or sets the firmware version of the switch.
        /// </summary>
        public string sFirmware { get; set; }

        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }
    }
    /// <summary>
    /// Represents an input/output (I/O) module, including identification, network, master device, and channel count properties.
    /// </summary>
    public class MyInOut
    {
        /// <summary>
        /// Gets or sets the name of the I/O module.
        /// </summary>
        public string sName { get; set; }

        /// <summary>
        /// Gets or sets the hardware reference of the I/O module.
        /// </summary>
        public string sReference { get; set; }

        /// <summary>
        /// Gets or sets the product range or series of the I/O module.
        /// </summary>
        public string sGamme { get; set; }

        /// <summary>
        /// Gets or sets the X1 interface address.
        /// </summary>
        public string sInterfaceX1 { get; set; }

        /// <summary>
        /// Gets or sets the VLAN associated with X1.
        /// </summary>
        public string sVlanX1 { get; set; }

        /// <summary>
        /// Gets or sets the name of the master device associated with this I/O module.
        /// </summary>
        public string sMasterName { get; set; }

        /// <summary>
        /// Gets or sets the number of analog inputs (AI).
        /// </summary>
        public int iAI { get; set; }

        /// <summary>
        /// Gets or sets the number of analog outputs (AQ).
        /// </summary>
        public int iAQ { get; set; }

        /// <summary>
        /// Gets or sets the number of digital inputs (DI).
        /// </summary>
        public int iDI { get; set; }

        /// <summary>
        /// Gets or sets the number of digital outputs (DQ).
        /// </summary>
        public int iDQ { get; set; }
    }
}