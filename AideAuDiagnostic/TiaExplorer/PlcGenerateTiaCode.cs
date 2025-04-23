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

namespace AideAuDiagnostic.TiaExplorer
{
    /// <summary>
    /// Provides logic for generating and compiling PLC code in a TIA Portal project,
    /// including code export, FC block creation, and diagnostic support.
    /// </summary>
    internal class PlcGenerateTiaCode
    {
        #region CONSTANTS

        /// <summary>
        /// The root folder name for OPC UA blocks in the project.
        /// </summary>
        const string sRootOPCUAFolder = @"AIDE_AU_DIAGNOSTIC";

        /// <summary>
        /// The name of the Function Code (FC) to be generated for diagnostic.
        /// </summary>
        const string sFCName_Receive_From_Gateway_1 = @"FC_Aide_au_diagnostic";

        /// <summary>
        /// The title/description for the generated FC block.
        /// </summary>
        const string sFCTitle_Receive_From_Gateway_1 = @"Update memory map from PLC Gateway 1";

        #endregion

        #region VARIABLES

        /// <summary>
        /// Reference to the project explorer object for TIA Portal.
        /// </summary>
        ExploreTiaPLC oExploreTiaPLC;

        /// <summary>
        /// Reference to the project/program information for the S7-1500H CPU.
        /// </summary>
        private TiaProjectForCPU oTiaProjectForCPU;

        /// <summary>
        /// Sets the current TIA project CPU information.
        /// </summary>
        /// <param name="oTiaProjectForCPU">The CPU/project reference.</param>
        public void SetTiaProjectForCPU(TiaProjectForCPU oTiaProjectForCPU) { this.oTiaProjectForCPU = oTiaProjectForCPU; }

        /// <summary>
        /// List of lines for FC (Function Code) data collection.
        /// </summary>
        private List<string> lsDataCollection;

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="PlcGenerateTiaCode"/>.
        /// </summary>
        /// <param name="oExploreTiaPLC">The TIA Portal project explorer instance.</param>
        /// <param name="lsDataCollection">The FC data collection list.</param>
        public PlcGenerateTiaCode(ExploreTiaPLC oExploreTiaPLC, List<string> lsDataCollection)
        {
            this.oExploreTiaPLC = oExploreTiaPLC;
            this.lsDataCollection = lsDataCollection;
        }

        /// <summary>
        /// Compiles the program for the specified station and checks for compilation errors.
        /// </summary>
        /// <param name="sStationName">Name of the station to compile.</param>
        /// <param name="sErrorText">Outputs any error message.</param>
        /// <returns>True if compilation succeeded without error, otherwise false.</returns>
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
                oThisPlc = GetThisStationByName(sStationName, ref oStationDevice, ref oStationDeviceItem, ref sErrorText);
                if (oThisPlc == null)
                {
                    sErrorText = string.Format("Station with name {0} not found in TIA Project", sStationName);
                    bRet = false;
                    break;
                }
                if (CompileStationAndGetErrors(oThisPlc, ref bOneErrorCompilation, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                break;
            }

            return bRet;
        }

        /// <summary>
        /// Returns the <see cref="PlcSoftware"/> reference and its associated <see cref="Device"/> and <see cref="DeviceItem"/>
        /// for a given station name.
        /// </summary>
        /// <param name="sStationName">The name of the station to search for.</param>
        /// <param name="oStationDevice">Outputs the found device object.</param>
        /// <param name="oStationDeviceItem">Outputs the associated device item.</param>
        /// <param name="sErrorText">Outputs any error message.</param>
        /// <returns>The <see cref="PlcSoftware"/> object, or null if not found.</returns>
        private PlcSoftware GetThisStationByName(string sStationName, ref Device oStationDevice, ref DeviceItem oStationDeviceItem, ref string sErrorText)
        {
            PlcSoftware oThisPlc = null;
            List<Device> ldListDevice = new List<Device>();
            Device oPLCHStation = null;

            while (true)
            {
                try
                {
                    if (oExploreTiaPLC.GetTiainterface().EnumerationDevice(ref ldListDevice, ref sErrorText) == false)
                    {
                        break;
                    }
                    foreach (Device oDevice in ldListDevice)
                    {
                        if (oDevice.Name == sStationName)
                        {
                            oPLCHStation = oDevice;
                            oStationDevice = oDevice;
                            break;
                        }
                    }
                    if (oPLCHStation == null)
                    {
                        sErrorText = string.Format("Station '{0}' for Plc Gateway not found !", sStationName);
                        break;
                    }

                    DeviceItem oDeviceItemToGetService = oExploreTiaPLC.TryToFoundDeviceItemInDevice(oPLCHStation);
                    if (oDeviceItemToGetService == null)
                    {
                        sErrorText = "Impossible to found PLC device item in device";
                        oThisPlc = null;
                        break;
                    }
                    oStationDeviceItem = oDeviceItemToGetService;
                    SoftwareContainer oSoftwareContainer = oDeviceItemToGetService.GetService<SoftwareContainer>() as SoftwareContainer;
                    oThisPlc = oSoftwareContainer.Software as PlcSoftware;
                    if (oThisPlc == null)
                    {
                        sErrorText = "controllertarget is null in this device";
                        break;
                    }
                }
                catch (Exception e)
                {
                    oPLCHStation = null;
                    sErrorText = string.Format("GetThisStationByName() Exception '{0}'", e.Message);
                }
                break;
            }

            return oThisPlc;
        }

        /// <summary>
        /// Compiles the complete station and checks for any compilation errors.
        /// </summary>
        /// <param name="oThisPlc">The PLC software to compile.</param>
        /// <param name="bAtLeastOneError">Outputs true if at least one error occurs.</param>
        /// <param name="sErrorText">Outputs any error message.</param>
        /// <returns>True if compilation succeeded with no errors, otherwise false.</returns>
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
                    sErrorText = string.Format("CompileStation Exception '{0}'", e.Message);
                    break;
                }
                if (bAtLeastOneError == true)
                {
                    sErrorText = "Compilation errors found in station";
                    bRet = false;
                }
                break;
            }
            return bRet;
        }

        /// <summary>
        /// Generates the diagnostic FC code and places it in the correct folder for the given PLC station.
        /// </summary>
        /// <param name="sStationName">Name of the target station.</param>
        /// <param name="iCmptEntree">Outputs the number of entries generated.</param>
        /// <param name="sErrorText">Outputs any error message.</param>
        /// <returns>True if code generation succeeded, otherwise false.</returns>
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
                oThisPlc = GetThisStationByName(sStationName, ref oStationDevice, ref oStationDeviceItem, ref sErrorText);
                if (oThisPlc == null)
                {
                    bRet = false;
                    break;
                }

                oThisOPCUAFolder = GetOrCreateOPCUAFolder(oThisPlc, ref sErrorText);
                if (oThisOPCUAFolder == null)
                {
                    bRet = false;
                    break;
                }

                if (MakeFC_Receive_From_Gateway(
                    oThisPlc,
                    oThisOPCUAFolder,
                    sFCName_Receive_From_Gateway_1,
                    sFCTitle_Receive_From_Gateway_1,
                    ref iCmptEntree,
                    ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }
        /// <summary>
        /// Returns a reference to the OPC UA folder in the PLC project, creating it if it does not exist.
        /// </summary>
        /// <param name="oThisPlc">The PLC software instance.</param>
        /// <param name="sErrorText">Outputs any error encountered during folder retrieval or creation.</param>
        /// <returns>The <see cref="PlcBlockUserGroup"/> representing the OPC UA folder.</returns>
        PlcBlockUserGroup GetOrCreateOPCUAFolder(PlcSoftware oThisPlc, ref string sErrorText)
        {
            PlcBlockUserGroup oGrpOPCUA = null;
            sErrorText = string.Empty;
            bool bRootFound = false;

            while (true)
            {
                try
                {
                    foreach (PlcBlockUserGroup oBlockUserFolder in oThisPlc.BlockGroup.Groups)
                    {
                        if (oBlockUserFolder.Name.Equals(sRootOPCUAFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            oGrpOPCUA = oBlockUserFolder;
                            bRootFound = true;
                            break;
                        }
                    }

                    if (!bRootFound)
                    {
                        oGrpOPCUA = oThisPlc.BlockGroup.Groups.Create(sRootOPCUAFolder);
                    }
                }
                catch (Exception e)
                {
                    oGrpOPCUA = null;
                    sErrorText = string.Format("GetOrCreateOPCUAFolder() Exception '{0}'", e.Message);
                }
                break;
            }
            return oGrpOPCUA;
        }

        /// <summary>
        /// Creates the diagnostic FC block, writes its source code, imports it into the TIA project, and updates the entry counter.
        /// </summary>
        /// <param name="oThisPLC">The PLC software instance.</param>
        /// <param name="oThisPlcUserFolder">The target user folder for the block.</param>
        /// <param name="sFC_Name_Receive_From_Gateway">The FC block name to create.</param>
        /// <param name="sFC_Title_Receive_From_Gateway">The FC block title/description.</param>
        /// <param name="iCmptEntree">Outputs the number of entries generated.</param>
        /// <param name="sErrorText">Outputs any error encountered during generation or import.</param>
        /// <returns>True if block creation and import succeeded, otherwise false.</returns>
        bool MakeFC_Receive_From_Gateway(
            PlcSoftware oThisPLC,
            PlcBlockUserGroup oThisPlcUserFolder,
            string sFC_Name_Receive_From_Gateway,
            string sFC_Title_Receive_From_Gateway,
            ref int iCmptEntree,
            ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            List<string> lsSourceFC = new List<string>();
            string sFCFileName = string.Empty;
            DateTime dtLocalDate = DateTime.Now;

            while (true)
            {
                lsSourceFC.AddRange(MakeFC_Header_For_Receive_From_PLC(sFC_Name_Receive_From_Gateway, sFC_Title_Receive_From_Gateway));

                lsSourceFC.Add(@"// BLOC FONCTION D'AIDE AU DIAGNOSTIC");
                lsSourceFC.Add(@"//");
                lsSourceFC.Add(@"// Ce bloc permet de recepurer toutes les pattes d'entree se trouvant");
                lsSourceFC.Add(@"// dans le fichier excel : Liste_LTU_BackJump.xlsx");
                lsSourceFC.Add(@"//");
                lsSourceFC.Add(@"// DATE ET HEURE DE LA DERNIERE EXECUTION");
                lsSourceFC.Add(@"// DE LA FONCTION :" + dtLocalDate);
                lsSourceFC.Add(@"");
                lsSourceFC.Add(@"");

                bool bTest = false;
                foreach (string sElement in lsDataCollection)
                {
                    if (sElement.Substring(0, 1) != "/")
                    {
                        bTest = true;
                        break;
                    }
                }

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
                    foreach (string sElement in lsDataCollection)
                    {
                        lsSourceFC.Add(sElement);
                        if (!sElement.Contains("//")) iCmptEntree++;
                    }
                    lsSourceFC.Add(@"END_IF;");
                }

                lsSourceFC.AddRange(MakeFC_End());

                if (CreateFCFile(sFC_Name_Receive_From_Gateway, lsSourceFC, ref sFCFileName, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                if (DeleteBlocFCInTiaPortalProject(oThisPlcUserFolder, sFC_Name_Receive_From_Gateway, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                if (ImportSourceDBAndGenerateItInTiaPortalProject(oThisPLC, oThisPlcUserFolder, sFC_Name_Receive_From_Gateway, sFCFileName, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }

                if (File.Exists(sFCFileName) == true) File.Delete(sFCFileName);

                break;
            }
            return bRet;
        }

        /// <summary>
        /// Builds the header lines for the FC block source code.
        /// </summary>
        /// <param name="sFCName">The name of the FC block.</param>
        /// <param name="sFCTitle">The title/description of the FC block.</param>
        /// <returns>A list of strings representing the FC header.</returns>
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

        /// <summary>
        /// Builds the footer/end lines for the FC block source code.
        /// </summary>
        /// <returns>A list of strings representing the FC end.</returns>
        List<string> MakeFC_End()
        {
            List<string> lsEnd = new List<string>
            {
                @"END_FUNCTION"
            };

            return lsEnd;
        }

        /// <summary>
        /// Creates a file for the FC block source code in the application directory.
        /// </summary>
        /// <param name="sBlocName">The block name for the file.</param>
        /// <param name="lsFileLines">The list of code lines to write to the file.</param>
        /// <param name="sFCFileName">Outputs the generated file name.</param>
        /// <param name="sErrorText">Outputs any error encountered during file creation.</param>
        /// <returns>True if the file was created successfully, otherwise false.</returns>
        bool CreateFCFile(string sBlocName, List<string> lsFileLines, ref string sFCFileName, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                try
                {
                    sFCFileName = string.Format(@"{0}{1}.scl", oExploreTiaPLC.GetTiaProjectDefinitions().sPathApplication, sBlocName);
                    if (File.Exists(sFCFileName) == true) File.Delete(sFCFileName);
                    File.WriteAllLines(sFCFileName, lsFileLines.ToArray());

                    break;
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Exception in CreateFCFile() : {0}", e.Message);
                    bRet = false;
                    break;
                }
            }

            return bRet;
        }

        /// <summary>
        /// Deletes the FC block from the TIA Portal project if it already exists in the given folder.
        /// </summary>
        /// <param name="oThisPlcUserFolder">The folder user group in which to search for the block.</param>
        /// <param name="sBlocName">The FC block name to delete.</param>
        /// <param name="sErrorText">Outputs any error encountered during deletion.</param>
        /// <returns>True if deletion was successful or block was not present, otherwise false.</returns>
        bool DeleteBlocFCInTiaPortalProject(PlcBlockUserGroup oThisPlcUserFolder, string sBlocName, ref string sErrorText)
        {
            bool bRet = true;
            List<PlcBlock> loListBlocks = new List<PlcBlock>();

            while (true)
            {
                if (oExploreTiaPLC.GetTiainterface().EnumerateBlockUserProgramForThisFolder(oThisPlcUserFolder, ref loListBlocks, ref sErrorText) == false)
                {
                    bRet = false;
                    break;
                }
                foreach (PlcBlock oBlock in loListBlocks)
                {
                    if (oBlock is FC)
                    {
                        try
                        {
                            if (oBlock.Name.ToUpper() == sBlocName.ToUpper())
                            {
                                oBlock.Delete();
                                break;
                            }
                        }
                        catch { }
                    }
                }

                break;
            }
            return bRet;
        }
        /// <summary>
        /// Imports the FC block source file and generates the block in the specified TIA Portal folder.
        /// </summary>
        /// <param name="oThisPLC">The PLC software instance.</param>
        /// <param name="oThisBlocFolder">The destination folder for the imported block.</param>
        /// <param name="sBlocName">The name of the block to import.</param>
        /// <param name="sDBFileName">The FC block source file name.</param>
        /// <param name="sErrorText">Outputs any error encountered during import or generation.</param>
        /// <returns>True if the block was imported and generated successfully, otherwise false.</returns>
        bool ImportSourceDBAndGenerateItInTiaPortalProject(
            PlcSoftware oThisPLC,
            PlcBlockUserGroup oThisBlocFolder,
            string sBlocName,
            string sDBFileName,
            ref string sErrorText)
        {
            bool bRet = true;
            PlcExternalSource oSource = null;

            while (true)
            {
                try
                {
                    PlcExternalSourceSystemGroup oSystemeSourceFolder = oThisPLC.ExternalSourceGroup as PlcExternalSourceSystemGroup;
                    foreach (PlcExternalSource oSourceRead in oSystemeSourceFolder.ExternalSources)
                    {
                        if (oSourceRead.Name.ToUpper() == sBlocName.ToUpper())
                        {
                            oSourceRead.Delete();
                            break;
                        }
                    }
                    if (oExploreTiaPLC.GetTiainterface().ImportSourceFileToSourceFolder(oSystemeSourceFolder, sBlocName, sDBFileName, ref oSource, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    if (oExploreTiaPLC.GetTiainterface().GenerateBlocFromSourceFile(oSource, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    oSource.Delete();
                    if (MoveBlocFromRootFolderToSpecificFolder(oThisPLC, oThisBlocFolder, sBlocName, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Exception in ImportSourceDBAndGenerateItInTiaPortalProject() : {0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }

        /// <summary>
        /// Moves a block from the root folder to a specific user folder in the TIA Portal project.
        /// </summary>
        /// <param name="oThisPLC">The PLC software instance.</param>
        /// <param name="oThisBlocFolder">The target destination folder.</param>
        /// <param name="sBlocName">The name of the block to move.</param>
        /// <param name="sErrorText">Outputs any error encountered during the move operation.</param>
        /// <returns>True if the block was moved successfully, otherwise false.</returns>
        bool MoveBlocFromRootFolderToSpecificFolder(
            PlcSoftware oThisPLC,
            PlcBlockUserGroup oThisBlocFolder,
            string sBlocName,
            ref string sErrorText)
        {
            bool bRet = true;
            PlcBlock oBlocToMove = null;
            string sXmlBlocToMove = string.Empty;
            PlcBlock oBlocImport = null;

            while (true)
            {
                try
                {
                    oBlocToMove = FindAnBlocInSpecificFolder(true, oThisPLC, oThisBlocFolder, sBlocName);
                    if (oBlocToMove == null)
                    {
                        sErrorText = string.Format("MoveBlocFromRootFolderToSpecificFolder() : Bloc {0} not found in root folder", sBlocName);
                        bRet = false;
                        break;
                    }
                    if (oBlocToMove.IsConsistent == false)
                    {
                        if (oExploreTiaPLC.GetTiainterface().CompileBloc(oBlocToMove, ref sErrorText) == false)
                        {
                            bRet = false;
                            break;
                        }
                    }
                    sXmlBlocToMove = string.Format(@"{0}BlocToMove.XML", oExploreTiaPLC.GetTiaProjectDefinitions().sPathApplication);

                    if (oExploreTiaPLC.GetTiainterface().ExportBlocToXml(oBlocToMove, sXmlBlocToMove, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    oBlocToMove.Delete();

                    if (oExploreTiaPLC.GetTiainterface().ImportBlocFromXml(oThisBlocFolder.Blocks, sXmlBlocToMove, ref sErrorText) == false)
                    {
                        bRet = false;
                        break;
                    }
                    if (File.Exists(sXmlBlocToMove) == true) File.Delete(sXmlBlocToMove);

                    oBlocImport = FindAnBlocInSpecificFolder(false, oThisPLC, oThisBlocFolder, sBlocName);
                    if (oBlocImport == null)
                    {
                        sErrorText = string.Format("MoveBlocFromRootFolderToSpecificFolder() : Bloc {0} not found in target folder", sBlocName);
                        bRet = false;
                        break;
                    }
                    if (oBlocImport.IsConsistent == false)
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
                    sErrorText = string.Format("Exception in MoveBlocFromRootFolderToSpecificFolder() : {0}", e.Message);
                    bRet = false;
                    break;
                }
                break;
            }
            return bRet;
        }

        /// <summary>
        /// Searches for a block by name in a specific folder or in the root, depending on the flag.
        /// </summary>
        /// <param name="bInRoot">True to search in the root folder, false to search in the specific folder.</param>
        /// <param name="oThisPLC">The PLC software instance.</param>
        /// <param name="oThisBlocFolder">The folder to search, used when bInRoot is false.</param>
        /// <param name="sBlocName">The name of the block to search for.</param>
        /// <returns>The matching <see cref="PlcBlock"/>, or null if not found.</returns>
        PlcBlock FindAnBlocInSpecificFolder(
            bool bInRoot,
            PlcSoftware oThisPLC,
            PlcBlockUserGroup oThisBlocFolder,
            string sBlocName)
        {
            PlcBlock oBlocToFound = null;

            if (!bInRoot)
            {
                foreach (PlcBlock oBlock in oThisBlocFolder.Blocks)
                {
                    if (oBlock.Name.ToUpper() == sBlocName.ToUpper())
                    {
                        oBlocToFound = oBlock;
                        break;
                    }
                }
            }
            else
            {
                foreach (PlcBlock oBloc in oThisPLC.BlockGroup.Blocks)
                {
                    if (oBloc.Name.ToUpper() == sBlocName.ToUpper())
                    {
                        oBlocToFound = oBloc;
                        break;
                    }
                }
            }
            return oBlocToFound;
        }
    }
}

