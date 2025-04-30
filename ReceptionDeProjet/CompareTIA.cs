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
    /// Provides methods for analyzing and comparing PLC devices information from a TIA Portal project.
    /// </summary>
    public class CompareTIA
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareTIA"/> class.
        /// </summary>
        public CompareTIA()
        { }

        /// <summary>
        /// Retrieves information about PLC devices from the given TIA Portal interface and constructs a <see cref="Project"/> object.
        /// </summary>
        /// <param name="tiaInterface">The TIA Openness interface used to interact with the TIA Portal project.</param>
        /// <param name="sError">A string for reporting or handling errors during the process.</param>
        /// <returns>
        /// A <see cref="Project"/> instance containing information about the project and its PLC devices.
        /// </returns>
        /// <remarks>
        /// This method collects project metadata, language, version, and analyzes each device. 
        /// It extracts main module properties, executes various analyses, and fills the project structure with detailed device data.
        /// </remarks>
        public Project GetPlcDevicesInfo(HMATIAOpenness_V16 tiaInterface, string sError)
        {
            var resultProject = new Project
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

            try
            {
                foreach (var device in tiaInterface.m_oTiaProject.Devices)
                {
                    if (device.DeviceItems.Count <= 1)
                    {
                        Console.WriteLine("Empty device detected");
                        continue;
                    }

                    var mainModule = FindMainModule(device);
                    if (mainModule == null)
                    {
                        Console.WriteLine("Main module not found");
                        continue;
                    }
                    try
                    {
                        var automate = CreateAutomateFromModule(mainModule);

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
                            BlockProtection(mainModule, automate);
                        }
                        catch
                        {
                            Console.WriteLine("Error during block protection; Automate: " + automate.sName);
                        }

                        try
                        {
                            InterfaceNetwork(mainModule, automate);
                        }
                        catch
                        {
                            Console.WriteLine("Error retrieving network interface; Automate: " + automate.sName);
                        }

                        try
                        {
                            DebugObBlocks(automate);
                        }
                        catch
                        {
                            Console.WriteLine("Error during debug display; Automate: " + automate.sName);
                        }

                        resultProject.AddAutomate(automate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing security properties: {ex.Message} {device.Name}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving PLC information: {ex.Message}");
            }

            return resultProject;
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
        /// Creates and populates an <see cref="Automate"/> object from the specified main module.
        /// </summary>
        /// <param name="mainModule">The main module from which to extract properties.</param>
        /// <returns>
        /// An instance of <see cref="Automate"/> filled with properties and configuration details from the main module.
        /// </returns>
        /// <remarks>
        /// This method extracts and organizes hardware, firmware, configuration, and sub-item information for the automate.
        /// </remarks>
        private Automate CreateAutomateFromModule(DeviceItem mainModule)
        {
            string gamme = FindGamme(mainModule);
            string[] gammesAutorisees = { "S7-1200", "S7-1500", "ET 200SP" };

            var automate = new Automate
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
            Console.WriteLine($"Gamme : {sGamme}");
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
            catch (Exception e)
            {
                OnlineProvider onlineProvider = mainModule.GetService<OnlineProvider>();
                onlineProvider.GoOffline();
                return "False";
            }
        }

        /// <summary>
        /// Analyzes OB and FC blocks from the specified main module, and populates the automate with the related information.
        /// </summary>
        /// <param name="mainModule">The <see cref="DeviceItem"/> representing the CPU/main module.</param>
        /// <param name="automate">The <see cref="Automate"/> object to be populated with block analysis results.</param>
        /// <remarks>
        /// This method analyzes all Function (FC) and Organization Block (OB) objects, extracts their cross-references, 
        /// and builds up the internal structure of the automate including nested and referenced blocks.
        /// </remarks>
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

                if (block.GetType().ToString() == "Siemens.Engineering.SW.Blocks.InstanceDB")
                {
                    // Placeholder for InstanceDB analysis if needed in the future
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

        /// <summary>
        /// Calculates statistics for LTU and OB blocks in the specified automate.
        /// </summary>
        /// <param name="automate">The <see cref="Automate"/> object containing OBs and block data.</param>
        /// <remarks>
        /// The method calculates the percentage of LTU blocks, and the percentage of blocks in OB1 and OB35 relative to the total.
        /// </remarks>
        private void CalculStatPLC(Automate automate)
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
        /// Outputs debug information for OB blocks and their nested bloc structures within the automate.
        /// </summary>
        /// <param name="automate">The <see cref="Automate"/> whose OB blocks will be debugged and displayed.</param>
        /// <remarks>
        /// Shows detailed block structure and LTU percentage for each OB, as well as summary statistics for the automate.
        /// </remarks>
        private void DebugObBlocks(Automate automate)
        {
            var sb = new StringBuilder();

            foreach (var myObTest in automate.oOBs)
            {
                sb.AppendLine($">OB{myObTest.iID} : {myObTest.sName}");

                foreach (var myBlocTest in myObTest.oBlocList)
                {
                    AppendBlocInfo(sb, myBlocTest, 1);
                }

                sb.AppendLine($"%LTU in OB{myObTest.iID} = {myObTest.Ltu100}");
            }

            Console.WriteLine(sb.ToString());
            Console.WriteLine($"LTU blocks percentage in automate: {automate.iStandardLTU}%");
            Console.WriteLine($"OB1 blocks percentage in automate: {automate.iBlocOb1}%");
            Console.WriteLine($"Number of protected blocks: {automate.ProtectedBlocs.Count}");
        }

        /// <summary>
        /// Recursively appends information about a block and its sub-blocks to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> receiving output.</param>
        /// <param name="myBloc">The <see cref="MyBloc"/> to display.</param>
        /// <param name="niveau">The current recursion depth for indentation.</param>
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
        /// Retrieves all <see cref="PlcBlock"/> objects from the specified CPU device item, including those in subfolders.
        /// </summary>
        /// <param name="cpuDeviceItem">The <see cref="DeviceItem"/> representing the CPU.</param>
        /// <returns>
        /// A list of all <see cref="PlcBlock"/> objects found in the device and its group hierarchy.
        /// </returns>
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
        /// Checks for the presence of PID blocks in OB1 and updates the automate accordingly.
        /// </summary>
        /// <param name="automate">The <see cref="Automate"/> object to update.</param>
        private void PIDOB1(Automate automate)
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
        /// Checks each block for know-how protection and updates the automate's protected block list.
        /// </summary>
        /// <param name="mainModule">The <see cref="DeviceItem"/> representing the main module.</param>
        /// <param name="automate">The <see cref="Automate"/> object to update.</param>
        private void BlockProtection(DeviceItem mainModule, Automate automate)
        {
            automate.sProgramProtection = "No protection found";
            foreach (var block in GetAllBlocksFromCPU(mainModule))
            {
                if (bool.Parse(block.GetAttribute("IsKnowHowProtected").ToString()))
                    automate.AddProtectedBloc(block.GetAttribute("Name").ToString());
            }
        }

        /// <summary>
        /// Extracts network interface and VLAN/subnet information from the main module and updates the automate accordingly.
        /// </summary>
        /// <param name="mainModule">The <see cref="DeviceItem"/> representing the main module.</param>
        /// <param name="automate">The <see cref="Automate"/> object to update with network interface details.</param>
        /// <remarks>
        /// This method processes PROFINET interfaces, retrieves NTP server settings, VLAN, and connected device details for both X1 and X2 interfaces when available.
        /// </remarks>
        private void InterfaceNetwork(DeviceItem mainModule, Automate automate)
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
                            NodeAssociation nodeSubnet = subnet.GetAttribute("Nodes") as NodeAssociation;
                            if (nodeSubnet != null)
                            {
                                string sNameConnectedDevice;
                                foreach (Node nodeX in nodeSubnet)
                                {
                                    sNameConnectedDevice = nodeX.GetAttribute("PnDeviceName").ToString().Split('.')[0];
                                    if (string.Compare(sNameConnectedDevice, automate.sName, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        automate.sConnectedDeviceX1 = sNameConnectedDevice;
                                    }
                                }
                            }
                            else
                            {
                                automate.sConnectedDeviceX1 = "Appareil introuvable";
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

                        Subnet subnet2 = node?.GetAttribute("ConnectedSubnet") as Subnet;
                        if (subnet2 != null)
                        {

                            automate.sVlanX2 = subnet2.GetAttribute("Name").ToString();
                            NodeAssociation nodeSubnet = subnet2.GetAttribute("Nodes") as NodeAssociation;
                            if (nodeSubnet != null)
                            {
                                string sNameConnectedDevice;
                                foreach (Node nodeX in nodeSubnet)
                                {
                                    sNameConnectedDevice = nodeX.GetAttribute("PnDeviceName").ToString().Split('.')[0];
                                    if (string.Compare(sNameConnectedDevice, automate.sName, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        automate.sConnectedDeviceX2 = sNameConnectedDevice;
                                    }
                                }
                            }
                            else
                            {
                                automate.sConnectedDeviceX2 = "Appareil introuvable";
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
    }

/// <summary>
/// Represents a project containing PLCs, HMIs, SCADAs, and related project information.
/// </summary>
public class Project
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
        public List<Automate> oAutomates;
        /// <summary>
        /// Gets the list of HMIs in the project.
        /// </summary>
        public List<HMI> oHMIs;
        /// <summary>
        /// Gets the list of SCADAs in the project.
        /// </summary>
        public List<SCADA> oSCADAs;

        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class.
        /// </summary>
        public Project()
        {
            oAutomates = new List<Automate>();
            oHMIs = new List<HMI>();
            oSCADAs = new List<SCADA>();
        }

        /// <summary>
        /// Adds a PLC automate to the project.
        /// </summary>
        /// <param name="automate">The <see cref="Automate"/> to add.</param>
        public void AddAutomate(Automate automate)
        {
            oAutomates.Add(automate);
        }

        /// <summary>
        /// Adds an HMI to the project.
        /// </summary>
        /// <param name="hmi">The <see cref="HMI"/> to add.</param>
        public void AddHMI(HMI hmi)
        {
            oHMIs.Add(hmi);
        }

        /// <summary>
        /// Adds a SCADA to the project.
        /// </summary>
        /// <param name="scada">The <see cref="SCADA"/> to add.</param>
        public void AddSCADA(SCADA scada)
        {
            oSCADAs.Add(scada);
        }
    }

    /// <summary>
    /// Represents a network or endpoint connection.
    /// </summary>
    public class Connexion
    {
        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        public string sName { get; set; }
        /// <summary>
        /// Gets or sets the endpoint address.
        /// </summary>
        public string sEndPoint { get; set; }
        /// <summary>
        /// Gets or sets the partner name.
        /// </summary>
        public string sPartener { get; set; }
        /// <summary>
        /// Gets or sets the connection type.
        /// </summary>
        public string sType { get; set; }
    }

    /// <summary>
    /// Represents a PLC automate with identification, network, and statistical properties.
    /// </summary>
    public class Automate
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
        
        public List<MyTag> oTagsIn { get; set; }
        public List<MyTag> oTagsOut { get; set; }
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
        /// Initializes a new instance of the <see cref="Automate"/> class.
        /// </summary>
        public Automate()
        {
            oOBs = new List<MyOB>();
            oFCs = new List<MyFC>();
            ProtectedBlocs = new List<string>();
            oTagsIn = new List<MyTag>();
            oTagsOut = new List<MyTag>();
            oTagsMem = new List<MyTag>();
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

        public void AddTagIn(MyTag tag)
        {
            oTagsIn.Add(tag);
        }

        public void AddTagOut(MyTag tag)
        {
            oTagsOut.Add(tag);
        }
        public void AddTagMem(MyTag tag)
        {
            oTagsMem.Add(tag);
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
    /// Represents a Data Block (DB) that inherits from <see cref="MyOB"/>.
    /// </summary>
    public class MyDB : MyOB
    {
        /// <summary>
        /// Gets or sets the last instance name.
        /// </summary>
        public string sLastInst { get; set; }
        /// <summary>
        /// Gets or sets the first instance name.
        /// </summary>
        public string sFirstInst { get; set; }
    }

    public class MyTag
    {
        public string sName { get; set; }
        public string sType { get; set; }
        public string sAddress { get; set; }
    }

    /// <summary>
    /// Represents an HMI device.
    /// </summary>
    public class HMI
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
        /// Gets or sets the X2 interface address.
        /// </summary>
        public string sInterfaceX2 { get; set; }
        /// <summary>
        /// Gets or sets the VLAN associated with X2.
        /// </summary>
        public string sVlanX2 { get; set; }
    }

    /// <summary>
    /// Represents a SCADA device.
    /// </summary>
    public class SCADA
    {
    }

}
