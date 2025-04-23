using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using GlobalsOPCUA;
using OpennessV16;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Tags;

namespace AideAuDiagnostic.TiaExplorer
{
    /// <summary>
    /// Handles the exploration and analysis of a TIA Portal project, including project and station selection,
    /// device information retrieval, and providing access to project definitions, openness interface, and Excel/FC data.
    /// </summary>
    public class ExploreTiaPLC
    {
        #region VARIABLES

        /// <summary>
        /// Project definitions object for the TIA Portal project.
        /// </summary>
        private readonly PLC_ProjectDefinitions oTiaProjectDefinitions;

        /// <summary>
        /// Gets the TIA Portal project definitions.
        /// </summary>
        public PLC_ProjectDefinitions GetTiaProjectDefinitions() { return oTiaProjectDefinitions; }

        /// <summary>
        /// Openness interface for TIA Portal.
        /// </summary>
        public HMATIAOpenness_V16 oTiainterface = new HMATIAOpenness_V16();

        /// <summary>
        /// Gets the TIA Portal openness interface.
        /// </summary>
        public HMATIAOpenness_V16 GetTiainterface() { return oTiainterface; }

        /// <summary>
        /// Stores whether a TIA Portal project is currently selected.
        /// </summary>
        public bool bTiaPortalProjectIsSelected = false;

        /// <summary>
        /// Gets whether a TIA Portal project is selected.
        /// </summary>
        public bool GetTiaPortalProjectIsSelected() { return bTiaPortalProjectIsSelected; }

        /// <summary>
        /// XML document handler for block export and parsing.
        /// </summary>
        private readonly XmlDocument oXmlDocument;

        /// <summary>
        /// Excel data dictionary loaded for block/tag mapping.
        /// </summary>
        private readonly Dictionary<Tuple<int, int>, object> dData;

        /// <summary>
        /// Data collection list for function code (FC) extraction.
        /// </summary>
        private readonly List<string> lsDataCollection;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ExploreTiaPLC"/> class.
        /// </summary>
        /// <param name="oTiaProjectDefinitions">Project definitions instance for the TIA Portal project.</param>
        /// <param name="dData">Excel data dictionary for mapping.</param>
        /// <param name="lsDataCollection">List for collecting data about FCs.</param>
        public ExploreTiaPLC(PLC_ProjectDefinitions oTiaProjectDefinitions, Dictionary<Tuple<int, int>, object> dData, List<string> lsDataCollection)
        {
            this.oTiaProjectDefinitions = oTiaProjectDefinitions;
            this.oXmlDocument = new XmlDocument();

            this.dData = dData;

            this.lsDataCollection = lsDataCollection;

        }

        #region TARGET SELECTION

        /// <summary>
        /// Allows the user to select a TIA Portal project, either currently open or by specifying a path.
        /// </summary>
        /// <param name="sError">Outputs any error encountered during selection.</param>
        /// <returns>True if a project was successfully selected, otherwise false.</returns>
        public bool ChooseTiaProject(ref string sError)
        {
            bool bRet;
            string sinfo = string.Empty;
            bool bCriticalError = false;

            sError = string.Empty;

            TiaPortalProjectSelection oTiaSelection = new TiaPortalProjectSelection();

            // Retrieve list of running TIA Portal project instances
            List<TiaPortalProcess> loTiaPortalCurrentProcess = HMATIAOpennessCurrentInstance.GetCurrentTiaPortalInstance();

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

            oTiaSelection.ShowDialog();

            if (oTiaSelection.bOneProjectSelected == true)
            {
                switch (oTiaSelection.iTiaProjectSelectType)
                {
                    case TiaPortalProjectSelection.TiaProjectSelectionType.CurrentTiaProject:
                        if (oTiainterface.AttachTiaPortalInstance(oTiaSelection.GetSelectCurrentProject().GetTiaPortalProcess(), ref sinfo) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }

                        oTiainterface.SetTIAPortalProject(oTiaSelection.GetSelectCurrentProject().GetTiaPortalProcess().ProjectPath.FullName);
                        if (oTiainterface.OpenCurrentTIAProjectFromInstance(ref sinfo, ref bCriticalError) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }
                        break;

                    case TiaPortalProjectSelection.TiaProjectSelectionType.NewTiaProject:
                        oTiainterface = new HMATIAOpenness_V16(oTiaProjectDefinitions.bWithUITiaPortal, ref sinfo);

                        oTiainterface.SetTIAPortalProject(oTiaSelection.sNewTiaPortalSelectionPath);
                        if (oTiainterface.OpenTIAProject(oTiaProjectDefinitions.GetUserName(), oTiaProjectDefinitions.GetUncryptPasswordUser(), ref sinfo, ref bCriticalError) == false)
                        {
                            sError = sinfo;
                            bRet = false;
                        }
                        break;
                }
                bTiaPortalProjectIsSelected = true;
                bRet = true;
            }
            else
            {
                sError = @"No Tia project selected";
                bRet = false;
            }
            return bRet;
        }

        /// <summary>
        /// Prompts the user to select a PLC station from the TIA Portal project.
        /// </summary>
        /// <param name="sStationName">Outputs the selected station name.</param>
        /// <param name="sError">Outputs any error encountered during selection.</param>
        /// <returns>True if a station was successfully selected, otherwise false.</returns>
        public bool GetTiaStationFromProjectList(ref string sStationName, ref string sError)
        {
            bool bRet = true;
            List<Device> loListDevice = new List<Device>();

            while (true)
            {
                sStationName = string.Empty;
                if (oTiainterface.EnumerationDevice(ref loListDevice, ref sError) == false)
                {
                    bRet = false;
                    break;
                }

                TiaPortalStationSelection tiaportalstationselection = new TiaPortalStationSelection();

                foreach (Device device in loListDevice)
                {
                    tiaportalstationselection.dDictionnaryTiaStationList.Add(device.Name, new HMATiaPortalDevice(device));
                }

                tiaportalstationselection.ShowDialog();

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
        #region CPU CODE READING

        /// <summary>
        /// Enumerates all folders, blocks, tags, and parameters for OPC UA export and mapping
        /// for a specific PLC station in the TIA project.
        /// </summary>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="sStationNameForPLC">The name of the PLC station to analyze.</param>
        /// <param name="sErrorText">Outputs any error encountered during enumeration.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool EnumerateFoldersBlocksParametersAndTagsForOPCUA(
            ref TiaProjectForCPU oTiaProjectForCPU,
            string sStationNameForPLC,
            ref string sErrorText)
        {
            bool bRet = true;
            List<Device> loListDevice = new List<Device>();
            Device oStationPLC = null;
            PlcSoftware oControllerPLC = null;
            lsDataCollection.Clear();

            sErrorText = string.Empty;

            while (true)
            {
                if (oTiainterface.EnumerationDevice(ref loListDevice, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                foreach (Device oDevice in loListDevice)
                {
                    if (oDevice.Name == sStationNameForPLC)
                    {
                        oStationPLC = oDevice;
                        break;
                    }
                }

                if (oStationPLC == null)
                {
                    sErrorText = string.Format("Station '{0}' for PLC not found !", sStationNameForPLC);
                    bRet = false;
                    break;
                }

                if (EnumerateBlocksAndParameterOPCUAMarck(ref oTiaProjectForCPU, oStationPLC, ref oControllerPLC, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                if (EnumerateTagsWithOPCUAMarck(ref oTiaProjectForCPU, oControllerPLC, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                if (EnumerateVariableSystemTags(ref oTiaProjectForCPU, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                break;
            }

            return bRet;
        }

        /// <summary>
        /// Enumerates all PLC blocks and parameters with the OPC UA mark for the given station.
        /// </summary>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oStationPLC">The PLC device object.</param>
        /// <param name="oControllerPLC">Reference to the PLC software controller object.</param>
        /// <param name="sErrorText">Outputs any error encountered during enumeration.</param>
        /// <returns>True if successful, otherwise false.</returns>
        bool EnumerateBlocksAndParameterOPCUAMarck(
            ref TiaProjectForCPU oTiaProjectForCPU,
            Device oStationPLC,
            ref PlcSoftware oControllerPLC,
            ref string sErrorText)
        {
            bool bRet = true;
            PlcSoftware oControllertarget = null;

            while (true)
            {
                DeviceItem oDeviceItemToGetService = TryToFoundDeviceItemInDevice(oStationPLC);
                if (oDeviceItemToGetService == null)
                {
                    sErrorText = "Impossible to found PLC device item in device";
                    bRet = false;
                    break;
                }

                SoftwareContainer oSoftwareContainer = oDeviceItemToGetService.GetService<SoftwareContainer>() as SoftwareContainer;
                oControllertarget = oSoftwareContainer.Software as PlcSoftware;
                if (oControllertarget == null)
                {
                    sErrorText = "controllertarget is null in this device";
                    bRet = false;
                    break;
                }

                oControllerPLC = oControllertarget;
                oTiaProjectForCPU.sControllerPLCName = oControllerPLC.Name;

                try
                {
                    SearchAllBlocInRootProgramFolder(oControllertarget, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderBlocks());
                    SearchAllFolderAndBlocInRootProgramFolder(oControllertarget, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderBlocks());
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Exception in EnumerateBlocksAndParameters() '{0}'", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }

            return bRet;
        }

        /// <summary>
        /// Searches for the CPU device item within a device.
        /// </summary>
        /// <param name="oPLCHStation">The PLC device to search.</param>
        /// <returns>The first found CPU DeviceItem, or null if not found.</returns>
        public DeviceItem TryToFoundDeviceItemInDevice(Device oPLCHStation)
        {
            DeviceItem oDeviceitem = null;
            List<DeviceItem> loListDeviceItem = new List<DeviceItem>();
            string sError = string.Empty;
            SoftwareContainer oSoftwarecontainer;

            if (oTiainterface.EnumerationDeviceItems(oPLCHStation, ref loListDeviceItem, ref sError) == true)
            {
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

        /// <summary>
        /// Scans the root program folder for all blocks, exporting FC blocks and enumerating their OPC UA parameters.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The source tree node for the blocks.</param>
        void SearchAllBlocInRootProgramFolder(
            PlcSoftware oControllerTarget,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            List<PlcBlock> loListBlocks = new List<PlcBlock>();
            string sErrorText = string.Empty;
            string sNewName = string.Empty;

            if (oTiainterface.EnumerateBlockUserProgramForPlc(oControllerTarget, ref loListBlocks, ref sErrorText) == true)
            {
                foreach (PlcBlock oBloc in loListBlocks)
                {
                    if (oBloc is FC)
                    {
                        TiaPortalBloc blocTIA = new TiaPortalBloc(
                            oBloc.Name,
                            oNodeSource,
                            oTiaProjectForCPU.GetNextFolderVariableId(),
                            sNewName
                        );
                        ExportTiaBlockDBAndEnumerateOPCUAParameters(oControllerTarget, (oBloc as FC), blocTIA, ref oTiaProjectForCPU);
                    }
                }
            }
        }

        /// <summary>
        /// Exports a FC block to XML and enumerates its OPC UA parameters.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oDB">The FC block to export.</param>
        /// <param name="oBlocTIA">The TIA Portal block representation.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <returns>True if export and parameter enumeration succeeded, otherwise false.</returns>
        bool ExportTiaBlockDBAndEnumerateOPCUAParameters(
            PlcSoftware oControllerTarget,
            FC oDB,
            TiaPortalBloc oBlocTIA,
            ref TiaProjectForCPU oTiaProjectForCPU)
        {
            bool bRet = true;
            string sErrorText = string.Empty;
            string sXmlDBFile = oTiaProjectDefinitions.sPathApplication + @"HMADBExport.Xml";

            while (true)
            {
                try
                {
                    if (File.Exists(sXmlDBFile)) File.Delete(sXmlDBFile);
                    bRet = oTiainterface.ExportBlocToXml(oDB, sXmlDBFile, ref sErrorText);
                    if (!bRet)
                    {
                        break;
                    }
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
        /// <summary>
        /// Parses the XML file exported from a FC block and extracts block and parameter information
        /// to update the data collection for diagnostic code generation.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="sXmlFile">The path to the exported XML file.</param>
        /// <param name="oBlocTIA">The TIA Portal block representation.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <returns>True if parsing succeeds, otherwise false.</returns>
        bool ParseTiaXmlDBFile(
            PlcSoftware oControllerTarget,
            string sXmlFile,
            TiaPortalBloc oBlocTIA,
            ref TiaProjectForCPU oTiaProjectForCPU)
        {
            bool bRet = true;
            string sCurrentPath = string.Empty;

            while (true)
            {
                if (!File.Exists(sXmlFile))
                {
                    bRet = false;
                    break;
                }
                oXmlDocument.Load(sXmlFile);

                foreach (XmlNode oNodeDocument in oXmlDocument.ChildNodes)
                {
                    if (oNodeDocument.Name == "Document")
                    {
                        foreach (XmlNode oNode in oNodeDocument.ChildNodes)
                        {
                            if (oNode.Name == "SW.Blocks.FC")
                            {
                                foreach (XmlNode oNodebloc in oNode.ChildNodes)
                                {
                                    if (oNodebloc.Name == "AttributeList")
                                    {
                                        foreach (XmlNode oNodeAttributeList in oNodebloc.ChildNodes)
                                        {
                                            if (oNodeAttributeList.Name == "Name")
                                            {
                                                lsDataCollection.Add("//" + oNodeAttributeList.InnerText.ToString());
                                            }
                                        }
                                    }
                                    if (oNodebloc.Name == "ObjectList")
                                    {
                                        foreach (XmlNode oNodeobjectlist in oNodebloc.ChildNodes)
                                        {
                                            if (oNodeobjectlist.Name == "SW.Blocks.CompileUnit")
                                            {
                                                EnumerateMembers(
                                                    oNodeobjectlist,
                                                    oControllerTarget,
                                                    oTiaProjectForCPU,
                                                    oBlocTIA,
                                                    ref sCurrentPath,
                                                    true,
                                                    dData,
                                                    lsDataCollection
                                                );
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

        /// <summary>
        /// Enumerates all members (parameters, blocks, wires) found in a compile unit XML node,
        /// maps connections, and fills the data collection list for diagnostic code generation.
        /// </summary>
        /// <param name="oListOfNodeMembers">The XML node representing the list of compile unit members.</param>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oBlocTIA">The TIA Portal block representation.</param>
        /// <param name="sCurrentPath">Reference to the current XML parsing path.</param>
        /// <param name="bFirstLevel">Indicates whether this is the first recursion level.</param>
        /// <param name="dData">Excel data dictionary for mapping.</param>
        /// <param name="lsDataCollection">List for collecting data about FCs.</param>
        public void EnumerateMembers(
            XmlNode oListOfNodeMembers,
            PlcSoftware oControllerTarget,
            TiaProjectForCPU oTiaProjectForCPU,
            TiaPortalBloc oBlocTIA,
            ref string sCurrentPath,
            bool bFirstLevel,
            Dictionary<Tuple<int, int>, object> dData,
            List<string> lsDataCollection
        )
        {
            var variableByUID = new Dictionary<string, VariableInfo>();
            var wires = new List<WireInfo>();
            var blocks = new List<BlockInfo>();

            foreach (XmlNode nodeCompileUnit in oListOfNodeMembers.ChildNodes)
            {
                if (nodeCompileUnit.Name == "AttributeList")
                {
                    foreach (XmlNode nodeAttributeList in nodeCompileUnit)
                    {
                        if (nodeAttributeList.Name == "NetworkSource")
                        {
                            foreach (XmlNode nodeNetworkSource in nodeAttributeList)
                            {
                                if (nodeNetworkSource.Name == "FlgNet")
                                {
                                    foreach (XmlNode nodeFlgNet in nodeNetworkSource)
                                    {
                                        if (nodeFlgNet.Name == "Parts")
                                        {
                                            foreach (XmlNode nodeParts in nodeFlgNet)
                                            {
                                                if (nodeParts.Name == "Access")
                                                {
                                                    string accessUid = nodeParts.Attributes["UId"].Value;
                                                    foreach (XmlNode nodeAccess in nodeParts)
                                                    {
                                                        if (nodeAccess.Name == "Symbol")
                                                        {
                                                            string dbName = null;
                                                            string varName = null;
                                                            bool firstComponent = true;
                                                            foreach (XmlNode nodeSymbol in nodeAccess)
                                                            {
                                                                if (nodeSymbol.Name == "Component")
                                                                {
                                                                    if (firstComponent)
                                                                    {
                                                                        dbName = nodeSymbol.Attributes["Name"].Value;
                                                                        firstComponent = false;
                                                                    }
                                                                    else
                                                                    {
                                                                        varName = nodeSymbol.Attributes["Name"].Value;
                                                                    }
                                                                }
                                                            }
                                                            if (!string.IsNullOrEmpty(dbName))
                                                            {
                                                                variableByUID[accessUid] = new VariableInfo { DbName = dbName, VarName = varName };
                                                            }
                                                        }
                                                        else if (nodeAccess.Name == "Constant")
                                                        {
                                                            variableByUID[accessUid] = new VariableInfo { DbName = "ConstantType", VarName = null };
                                                        }
                                                    }
                                                }
                                                else if (nodeParts.Name == "Call")
                                                {
                                                    string blockUid = nodeParts.Attributes["UId"].Value;
                                                    string blockName = "";
                                                    string dbiName = "";

                                                    foreach (XmlNode nodeCall in nodeParts)
                                                    {
                                                        if (nodeCall.Name == "CallInfo")
                                                        {
                                                            blockName = nodeCall.Attributes["Name"].Value;
                                                            foreach (XmlNode nodeCallInfo in nodeCall)
                                                            {
                                                                if (nodeCallInfo.Name == "Instance")
                                                                {
                                                                    foreach (XmlNode nodeInstance in nodeCallInfo)
                                                                    {
                                                                        if (nodeInstance.Name == "Component")
                                                                        {
                                                                            dbiName = nodeInstance.Attributes["Name"].Value;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    blocks.Add(new BlockInfo
                                                    {
                                                        UID = blockUid,
                                                        Name = blockName,
                                                        DBiName = dbiName
                                                    });
                                                }
                                            }
                                        }
                                        else if (nodeFlgNet.Name == "Wires")
                                        {
                                            foreach (XmlNode nodeWires in nodeFlgNet)
                                            {
                                                if (nodeWires.Name == "Wire")
                                                {
                                                    string wireSource = null;
                                                    string wireTarget = null;
                                                    string wireName = null;

                                                    foreach (XmlNode nodeWire in nodeWires)
                                                    {
                                                        if (nodeWire.Name == "IdentCon")
                                                        {
                                                            wireSource = nodeWire.Attributes["UId"].Value;
                                                        }
                                                        else if (nodeWire.Name == "NameCon")
                                                        {
                                                            wireTarget = nodeWire.Attributes["UId"].Value;
                                                            wireName = nodeWire.Attributes["Name"].Value;
                                                        }
                                                    }
                                                    if (wireSource != null && wireTarget != null && wireName != null)
                                                    {
                                                        wires.Add(new WireInfo
                                                        {
                                                            SourceUID = wireSource,
                                                            TargetUID = wireTarget,
                                                            Name = wireName
                                                        });
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

            var blockPatteToDb = new Dictionary<string, Dictionary<string, string>>();

            foreach (var block in blocks)
            {
                var patteToDb = new Dictionary<string, string>();
                foreach (var wire in wires.Where(w => w.TargetUID == block.UID))
                {
                    if (variableByUID.TryGetValue(wire.SourceUID, out var varInfo))
                    {
                        if (!string.IsNullOrEmpty(varInfo.DbName) && varInfo.DbName != "ConstantType")
                        {
                            patteToDb[wire.Name] = varInfo.DbName;
                        }
                    }
                }
                blockPatteToDb[block.UID] = patteToDb;
            }

            foreach (var block in blocks)
            {
                foreach (KeyValuePair<Tuple<int, int>, object> keyValue in dData)
                {
                    if (keyValue.Key.Item2 == 1 && block.Name == keyValue.Value.ToString())
                    {
                        lsDataCollection.Add($"\"{block.DBiName}\".sHmiUdt.backJump[0] := '{block.Name}';");
                        int ligneIndice = keyValue.Key.Item1;

                        var excelColumnToDb = new Dictionary<int, string>();

                        foreach (KeyValuePair<Tuple<int, int>, object> keyValueH in dData)
                        {
                            if (keyValueH.Key.Item1 == ligneIndice && keyValueH.Key.Item2 > 1)
                            {
                                var patteName = keyValueH.Value.ToString();
                                int columnIndex = keyValueH.Key.Item2;

                                if (!string.IsNullOrEmpty(patteName)
                                    && blockPatteToDb.TryGetValue(block.UID, out var patteDbMap)
                                    && patteDbMap.TryGetValue(patteName, out var dbName))
                                {
                                    excelColumnToDb[columnIndex] = dbName;
                                }
                            }
                        }

                        var orderedColumns = excelColumnToDb.Keys.OrderBy(k => k).ToList();

                        for (int i = 0; i < orderedColumns.Count; i++)
                        {
                            int columnIndex = orderedColumns[i];
                            lsDataCollection.Add($"\"{block.DBiName}\".sHmiUdt.backJump[{columnIndex - 1}] := '{excelColumnToDb[columnIndex]}';");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively searches for all folders and blocks from the root of the PLC project,
        /// populates the hierarchical structure and enumerates all blocks and subfolders.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The source tree node for the blocks.</param>
        void SearchAllFolderAndBlocInRootProgramFolder(
            PlcSoftware oControllerTarget,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            foreach (PlcBlockUserGroup oblockUserFolder in oControllerTarget.BlockGroup.Groups)
            {
                TreeNode oNode = new TreeNode(oblockUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oblockUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                oNodeSource?.Nodes.Add(oNode);
                EnumBlocksInFolderBlocks(oControllerTarget, oblockUserFolder, ref oTiaProjectForCPU, oNode);
                EnumerateBlockUserFolders(oControllerTarget, oblockUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }

        /// <summary>
        /// Enumerates all blocks inside a folder and exports FC blocks for OPC UA parameter extraction.
        /// Adds blocks to the internal structure and node tree.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oBlocks">The block user group (folder) to enumerate.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for the blocks.</param>
        private void EnumBlocksInFolderBlocks(
            PlcSoftware oControllerTarget,
            PlcBlockUserGroup oBlocks,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            string sNewName = string.Empty;

            foreach (PlcBlock oBloc in oBlocks.Blocks)
            {
                if (oBloc is FC)
                {
                    TreeNode oNode = new TreeNode(oBloc.Name);
                    TiaPortalBloc oTiaBloc = new TiaPortalBloc(oBloc.Name, oNodeSource, oTiaProjectForCPU.GetNextFolderVariableId(), sNewName);
                    ExportTiaBlockDBAndEnumerateOPCUAParameters(oControllerTarget, (oBloc as FC), oTiaBloc, ref oTiaProjectForCPU);
                    oTiaProjectForCPU.loListBlocs.Add(oTiaBloc);
                    oNode.Tag = oTiaBloc;
                    oNodeSource.Nodes.Add(oNode);
                }
            }
        }

        /// <summary>
        /// Recursively enumerates all subfolders within a block user folder and processes their blocks and structures.
        /// </summary>
        /// <param name="oControllerTarget">The PLC software controller.</param>
        /// <param name="oBlockUserFolder">The parent block user group (folder).</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for the subfolders.</param>
        private void EnumerateBlockUserFolders(
            PlcSoftware oControllerTarget,
            PlcBlockUserGroup oBlockUserFolder,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            foreach (PlcBlockUserGroup oSubBlockUserFolder in oBlockUserFolder.Groups)
            {
                TreeNode oNode = new TreeNode(oSubBlockUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oSubBlockUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                oNodeSource.Nodes.Add(oNode);
                EnumBlocksInFolderBlocks(oControllerTarget, oSubBlockUserFolder, ref oTiaProjectForCPU, oNode);
                EnumerateBlockUserFolders(oControllerTarget, oSubBlockUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }

        /// <summary>
        /// Enumerates all tags contained in tag tables with the OPC UA attribute.
        /// Scans all tag tables at the root and recursively in folders.
        /// </summary>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oControllerPLC">The PLC software controller.</param>
        /// <param name="sErrorText">Outputs any error encountered during enumeration.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool EnumerateTagsWithOPCUAMarck(
            ref TiaProjectForCPU oTiaProjectForCPU,
            PlcSoftware oControllerPLC,
            ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    SearchAllVariableTableInRootPlcTagTableFolder(oControllerPLC, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderTags());
                    SearchAllFolderAndTagTableInRootTagTableFolder(oControllerPLC, ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderTags());
                }
                catch (Exception ex)
                {
                    sErrorText = string.Format("Exception in EnumerateTagsWithOPCUAMarck() '{0}'", ex.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }

        /// <summary>
        /// Scans all variable tables at the root PLC tag table folder and enumerates variables with the OPC UA attribute.
        /// </summary>
        /// <param name="oControllerPLC">The PLC software controller.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for variables.</param>
        private void SearchAllVariableTableInRootPlcTagTableFolder(
            PlcSoftware oControllerPLC,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            PlcTagTableComposition oTagTables = oControllerPLC.TagTableGroup.TagTables;

            foreach (PlcTagTable oTagTable in oTagTables)
            {
                ReadAllVariablesInTableWithOPCUAFlag(oTagTable, ref oTiaProjectForCPU, oNodeSource);
            }
        }

        /// <summary>
        /// Reads all variables in a given tag table and adds those with the OPC UA attribute to the project configuration.
        /// </summary>
        /// <param name="oTagTable">The tag table to scan.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for variables.</param>
        void ReadAllVariablesInTableWithOPCUAFlag(
            PlcTagTable oTagTable,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            List<string> lsListAttributs = new List<string>
    {
        "ExternalWritable"
    };
            IList<object> iloListValueAttributes;
            bool bReadOnly = false;

            foreach (PlcTag oTag in oTagTable.Tags)
            {
                if (oTag.Comment.Items.Count > 0)
                {
                    if (oTag.Comment.Items[0].Text.ToUpper().IndexOf(oTiaProjectDefinitions.sCommentarTagVariableMarck.ToUpper()) == 0)
                    {
                        iloListValueAttributes = oTag.GetAttributes(lsListAttributs);
                        if ((bool)iloListValueAttributes[0] == false) bReadOnly = true;
                        else bReadOnly = false;
                        TiaPortalVariable oVariable = new TiaPortalVariable(
                            oTag.Name,
                            null,
                            oTag.DataTypeName,
                            oTag.DataTypeName,
                            oTiaProjectForCPU.GetNextFolderVariableId(),
                            oNodeSource,
                            oTag.Comment.Items[0].Text,
                            string.Empty,
                            bReadOnly,
                            string.Empty
                        );
                        oTiaProjectForCPU.loListVariablesTags.Add(oVariable);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively scans all folders and tag tables from the root, enumerating all variables with the OPC UA attribute.
        /// </summary>
        /// <param name="oControllerPLC">The PLC software controller.</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="odeSource">The parent tree node for folders/tables.</param>
        void SearchAllFolderAndTagTableInRootTagTableFolder(
            PlcSoftware oControllerPLC,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode odeSource)
        {
            foreach (PlcTagTableUserGroup oControllerTargetUserFolder in oControllerPLC.TagTableGroup.Groups)
            {
                TreeNode node = new TreeNode(oControllerTargetUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oControllerTargetUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                odeSource?.Nodes.Add(node);
                EnumTagTableInFolderTagTable(oControllerTargetUserFolder, ref oTiaProjectForCPU, node);
                EnumerateTagTableUserGroup(oControllerTargetUserFolder, ref oTiaProjectForCPU, node);
            }
        }

        /// <summary>
        /// Enumerates all tag tables in a given tag table user group and scans their variables.
        /// </summary>
        /// <param name="oControllerTagUserFolder">The tag table user group (folder).</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for variables.</param>
        void EnumTagTableInFolderTagTable(
            PlcTagTableUserGroup oControllerTagUserFolder,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            foreach (PlcTagTable oTagTable in oControllerTagUserFolder.TagTables)
            {
                ReadAllVariablesInTableWithOPCUAFlag(oTagTable, ref oTiaProjectForCPU, oNodeSource);
            }
        }

        /// <summary>
        /// Recursively enumerates all subgroups within a tag table user group and scans contained tag tables.
        /// </summary>
        /// <param name="oControllerTargetUserFolder">The parent tag table user group (folder).</param>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for subgroups.</param>
        private void EnumerateTagTableUserGroup(
            PlcTagTableUserGroup oControllerTargetUserFolder,
            ref TiaProjectForCPU oTiaProjectForCPU,
            TreeNode oNodeSource)
        {
            foreach (PlcTagTableUserGroup oSubControllerTargetUserFolder in oControllerTargetUserFolder.Groups)
            {
                TreeNode oNode = new TreeNode(oSubControllerTargetUserFolder.Name)
                {
                    Tag = new TiaPortalFolder(oSubControllerTargetUserFolder.Name, oTiaProjectForCPU.GetNextFolderVariableId())
                };

                oNodeSource.Nodes.Add(oNode);
                EnumTagTableInFolderTagTable(oSubControllerTargetUserFolder, ref oTiaProjectForCPU, oNode);
                EnumerateTagTableUserGroup(oSubControllerTargetUserFolder, ref oTiaProjectForCPU, oNode);
            }
        }

        /// <summary>
        /// Enumerates system variables for diagnostics and adds them to the project configuration.
        /// </summary>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="sErrorText">Outputs any error encountered during enumeration.</param>
        /// <returns>True if successful, otherwise false.</returns>
        private bool EnumerateVariableSystemTags(
            ref TiaProjectForCPU oTiaProjectForCPU,
            ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    ScanAllSystemVariables(ref oTiaProjectForCPU, oTiaProjectForCPU.GetRootNodefolderSystemVariables());
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Exception in EnumerateVariableSystemTags() '{0}'", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        /// <summary>
        /// Adds all system variable folders and variables for diagnostics to the project configuration.
        /// </summary>
        /// <param name="oTiaProjectForCPU">Reference to the TIA project CPU configuration.</param>
        /// <param name="oNodeSource">The parent tree node for the system variables.</param>
        void ScanAllSystemVariables(ref TiaProjectForCPU oTiaProjectForCPU, TreeNode oNodeSource)
        {
            string sVariableMappingName = string.Empty;
            TiaPortalVariable oTiaPortalVariable = null;

            TreeNode oNode = new TreeNode(oTiaProjectForCPU.sRootVariableSystemName)
            {
                Tag = new TiaPortalFolder(oTiaProjectForCPU.sRootVariableSystemName,
                                           oTiaProjectForCPU.GetNextFolderVariableId())
            };
            oNodeSource.Nodes.Add(oNode);

            sVariableMappingName = @"""OPC_UA_Server_State"".""iServiceLevel""";

            oTiaPortalVariable = new TiaPortalVariable(@"ServiceLevel", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Service level de la redondance OPC UA", sVariableMappingName, false, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            sVariableMappingName = @"""OPC_UA_Server_State"".""bEnableWriteToPLCH""";
            oTiaPortalVariable = new TiaPortalVariable(@"EnableWriteToPLCH", null, @"Boolean", @"Boolean", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Valide l'écriture dans le PLC H distant", sVariableMappingName, false, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            sVariableMappingName = @"""OPC_UA_Server_State"".""iServerStatus""";
            oTiaPortalVariable = new TiaPortalVariable(@"ServerStatus", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Status du serveur OPC au niveau du PLC H distant", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLC_H_Redundant_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLC_H_Redundant_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la redondance du PLC_H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLc_CPU_1_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLc_CPU_1_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la CPU 1 du PLC H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);

            sVariableMappingName = @"""OPC_UA_Server_State"".""iPLc_CPU_2_State""";
            oTiaPortalVariable = new TiaPortalVariable(@"PLc_CPU_2_State", null, @"Int16", @"Int16", oTiaProjectForCPU.GetNextFolderVariableId(),
                                                      oNode, @"Etat de la CPU 2 du PLC H", sVariableMappingName, true, string.Empty);
            oTiaProjectForCPU.loListVariablesSystem.Add(oTiaPortalVariable);
        }
        #endregion
    }

    /// <summary>
    /// Represents information about a wire (connection) in a PLC FC block network.
    /// </summary>
    public class WireInfo
    {
        /// <summary>
        /// Gets or sets the UID of the source of the wire.
        /// </summary>
        public string SourceUID { get; set; }

        /// <summary>
        /// Gets or sets the UID of the target of the wire.
        /// </summary>
        public string TargetUID { get; set; }

        /// <summary>
        /// Gets or sets the name of the wire (connection).
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents information about a block in the PLC project for mapping and code generation.
    /// </summary>
    public class BlockInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier (UID) of the block.
        /// </summary>
        public string UID { get; set; }

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the DB instance name associated with the block.
        /// </summary>
        public string DBiName { get; set; }
    }

    /// <summary>
    /// Represents information about a variable for block/parameter mapping.
    /// </summary>
    public class VariableInfo
    {
        /// <summary>
        /// Gets or sets the name of the parent DB (data block).
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// Gets or sets the name of the variable within the DB.
        /// </summary>
        public string VarName { get; set; }
    }

    /// <summary>
    /// Represents a wrapper interface for a TIA Portal process.
    /// </summary>
    public class HMATiaPortalProcess
    {
        private TiaPortalProcess oTiaPortalProcess;

        /// <summary>
        /// Gets the wrapped TiaPortalProcess instance.
        /// </summary>
        public TiaPortalProcess GetTiaPortalProcess() { return oTiaPortalProcess; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMATiaPortalProcess"/> class.
        /// </summary>
        /// <param name="oTiaPortalProcess">Underlying TIA Portal process.</param>
        public HMATiaPortalProcess(TiaPortalProcess oTiaPortalProcess)
        {
            this.oTiaPortalProcess = oTiaPortalProcess;
        }

        /// <summary>
        /// Returns the display name of the TIA project for selection controls.
        /// </summary>
        /// <returns>Project name without extension.</returns>
        public override string ToString()
        {
            return string.Format("{0}", Path.GetFileNameWithoutExtension(oTiaPortalProcess.ProjectPath.FullName));
        }
    }

    /// <summary>
    /// Represents a wrapper interface for a TIA Portal device.
    /// </summary>
    public class HMATiaPortalDevice
    {
        private Device oTiaPortalDevice;

        /// <summary>
        /// Gets the wrapped Device instance.
        /// </summary>
        public Device GetPortalDevice() { return oTiaPortalDevice; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMATiaPortalDevice"/> class.
        /// </summary>
        /// <param name="oDevice">The underlying TIA Portal device.</param>
        public HMATiaPortalDevice(Device oDevice)
        {
            this.oTiaPortalDevice = oDevice;
        }

        /// <summary>
        /// Returns the display name of the device for selection controls.
        /// </summary>
        /// <returns>Device name as string.</returns>
        public override string ToString()
        {
            return string.Format("{0}", oTiaPortalDevice.Name);
        }
    }
}
