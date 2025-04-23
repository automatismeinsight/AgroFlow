using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Library;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Types;

namespace OpennessV16
{
    /// <summary>
    /// Provides a wrapper for interacting with Siemens TIA Portal using Openness V16,
    /// including project and library management, connection handling, and user authentication.
    /// </summary>
    public class HMATIAOpenness_V16
    {
        private TiaPortal m_oTiaPortal;
        /// <summary>
        /// The current TIA project instance.
        /// </summary>
        public Project m_oTiaProject;
        private bool m_bIsConnected = false;
        private string m_sTIAPortalProject = string.Empty;
        private bool m_bTiaProjectIsOpen = false;
        /// <summary>
        /// User name for UMAC-protected projects.
        /// </summary>
        public static string m_sUserName;
        /// <summary>
        /// Password for UMAC-protected projects.
        /// </summary>
        public static string m_sPassword;

        /// <summary>
        /// Gets the name of the selected TIA Portal project.
        /// </summary>
        /// <returns>The project file name.</returns>
        public string GetTIAPortalProject() { return (m_sTIAPortalProject); }

        /// <summary>
        /// Sets the name of the TIA Portal project to use.
        /// </summary>
        /// <param name="sTiaPortalProject">The project file path.</param>
        public void SetTIAPortalProject(string sTiaPortalProject) { m_sTIAPortalProject = sTiaPortalProject; }

        private void Confirmation(object sender, ConfirmationEventArgs e)
        {
            e.IsHandled = true;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HMATIAOpenness_V16()
        {
        }

        /// <summary>
        /// Constructor with option to launch TIA Portal UI.
        /// </summary>
        /// <param name="bOpenTIAPortal">Indicates if the TIA Portal editor should be opened.</param>
        /// <param name="sError">Outputs error text if connection fails.</param>
        public HMATIAOpenness_V16(bool bOpenTIAPortal, ref string sError)
        {
            sError = string.Empty;
            m_bIsConnected = true;
            try
            {
                switch (bOpenTIAPortal)
                {
                    case true:
                        m_oTiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);
                        break;
                    case false:
                        m_oTiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
                        m_oTiaPortal.Confirmation += Confirmation;
                        break;
                }
            }
            catch (Exception e)
            {
                m_bIsConnected = false;
                sError = e.Message;
            }
        }

        /// <summary>
        /// Attaches to an existing TIA Portal process and sets the current project.
        /// </summary>
        /// <param name="tiaprocess">The running TIA Portal process to attach to.</param>
        /// <param name="sError">Outputs any error encountered during attachment.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool AttachTiaPortalInstance(TiaPortalProcess tiaprocess, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            m_bIsConnected = true;
            try
            {
                m_oTiaPortal = tiaprocess.Attach();
                m_sTIAPortalProject = tiaprocess.ProjectPath.FullName;
            }
            catch (Exception e)
            {
                m_bIsConnected = false;
                sError = e.Message;
            }
            return bRet;
        }

        /// <summary>
        /// Opens the currently configured TIA Portal project.
        /// </summary>
        /// <param name="sUserName">The user name for UMAC-protected projects.</param>
        /// <param name="sPassword">The password for UMAC-protected projects.</param>
        /// <param name="sErrorText">Outputs any error if project opening fails.</param>
        /// <param name="bCriticalError">Outputs true if a critical error occurs.</param>
        /// <returns>True if the project was opened successfully, otherwise false.</returns>
        public bool OpenTIAProject(string sUserName, string sPassword, ref string sErrorText, ref bool bCriticalError)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            bCriticalError = false;

            while (true)
            {
                m_sUserName = sUserName;
                m_sPassword = sPassword;

                if ((m_bIsConnected == false) || (m_oTiaPortal == null))
                {
                    sErrorText = "Pas de connexion avec une instance de TIA Portal";
                    bRet = false;
                    break;
                }
                if (File.Exists(m_sTIAPortalProject) == false)
                {
                    sErrorText = string.Format("Projet TIA Portal {0} incorrect", m_sTIAPortalProject);
                    bRet = false;
                    break;
                }
                try
                {
                    FileInfo fileinfo = new FileInfo(m_sTIAPortalProject);

                    if (sUserName.Length == 0)
                    {
                        m_oTiaProject = m_oTiaPortal.Projects.Open(fileinfo);
                    }
                    else
                    {
                        m_oTiaProject = m_oTiaPortal.Projects.Open(fileinfo, MyUmacDelegate);
                    }
                    m_bTiaProjectIsOpen = true;
                }
                catch (Siemens.Engineering.EngineeringException e)
                {
                    sErrorText = string.Format("Problème ouverture projet '{0}'", e.Message);
                    m_bTiaProjectIsOpen = false;
                    bRet = false;
                    break;
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Problème critique ouverture projet '{0}'", e.Message);
                    bRet = false;
                    m_bTiaProjectIsOpen = false;
                    m_oTiaPortal = null;
                    m_bIsConnected = false;
                    bCriticalError = true;
                    break;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Delegate for secure UMAC connection to TIA Portal.
        /// Sets user name and password for project authentication.
        /// </summary>
        /// <param name="umacCredentials">UMAC credentials structure for the TIA project.</param>
        private static void MyUmacDelegate(UmacCredentials umacCredentials)
        {
            SecureString password = new SecureString();
            foreach (char c in m_sPassword)
            {
                password.AppendChar(c);
            }
            umacCredentials.Type = UmacUserType.Project;
            umacCredentials.Name = m_sUserName;
            umacCredentials.SetPassword(password);
        }

        /// <summary>
        /// Opens the currently attached TIA Portal project instance.
        /// </summary>
        /// <param name="sErrorText">Outputs any error if the project instance cannot be opened.</param>
        /// <param name="bCriticalError">Outputs true if a critical error occurs.</param>
        /// <returns>True if the project was opened successfully, otherwise false.</returns>
        public bool OpenCurrentTIAProjectFromInstance(ref string sErrorText, ref bool bCriticalError)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            bCriticalError = false;

            while (true)
            {
                if ((m_bIsConnected == false) || (m_oTiaPortal == null))
                {
                    sErrorText = "Pas de connexion avec une instance de TIA Portal";
                    bRet = false;
                    break;
                }
                if (File.Exists(m_sTIAPortalProject) == false)
                {
                    sErrorText = string.Format("Projet TIA Portal {0} incorrect", m_sTIAPortalProject);
                    bRet = false;
                    break;
                }
                try
                {
                    foreach (Project projet in m_oTiaPortal.Projects)
                    {
                        m_oTiaProject = projet;
                    }
                    m_bTiaProjectIsOpen = true;
                }
                catch (Siemens.Engineering.EngineeringException e)
                {
                    sErrorText = string.Format("Problème ouverture projet '{0}'", e.Message);
                    m_bTiaProjectIsOpen = false;
                    bRet = false;
                    break;
                }
                catch (Exception e)
                {
                    sErrorText = string.Format("Problème critique ouverture projet '{0}'", e.Message);
                    bRet = false;
                    m_bTiaProjectIsOpen = false;
                    m_oTiaPortal = null;
                    m_bIsConnected = false;
                    bCriticalError = true;
                    break;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Enumerates all devices in the current TIA Portal project, filtering by supported PLC or ET200SP types.
        /// </summary>
        /// <param name="ListDevice">Reference to a list that will be filled with found devices.</param>
        /// <param name="sError">Outputs any error encountered during enumeration.</param>
        /// <returns>True if devices were enumerated successfully, otherwise false.</returns>
        public bool EnumerationDevice(ref List<Device> ListDevice, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            DeviceComposition deviceaggregation = m_oTiaProject.Devices;

            try
            {
                while (true)
                {
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sError = "Pas de projet TIA ouvert";
                        bRet = false;
                        break;
                    }
                    foreach (Device device in deviceaggregation)
                    {
                        if (device.TypeIdentifier == "System:Device.S71500" || device.TypeIdentifier == "System:Device.S71200" || device.TypeIdentifier == "System:Device.ET200SP")
                        {
                            ListDevice.Add(device);
                        }
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                sError = string.Format("Exception dans énumération device '{0}'", e.Message);
                bRet = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Enumerates all device items for a given device in TIA Portal.
        /// </summary>
        /// <param name="device">The device whose items to enumerate.</param>
        /// <param name="ListDeviceItem">Reference to a list that will be filled with device items.</param>
        /// <param name="sError">Outputs any error encountered during enumeration.</param>
        /// <returns>True if device items were enumerated successfully, otherwise false.</returns>
        public bool EnumerationDeviceItems(Device device, ref List<DeviceItem> ListDeviceItem, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            DeviceItemComposition deviceItemAggregation = device.DeviceItems;

            try
            {
                while (true)
                {
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sError = "Pas de projet TIA ouvert";
                        bRet = false;
                        break;
                    }
                    foreach (DeviceItem deviceitem in deviceItemAggregation)
                    {
                        ListDeviceItem.Add(deviceitem as DeviceItem);
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                sError = string.Format("Exception dans énumération device items '{0}'", e.Message);
                bRet = false;
            }
            return (bRet);
        }
        /// <summary>
        /// Lists all program blocks in the root program directory of the PLC.
        /// </summary>
        /// <param name="controllertarget">PLC software controller target.</param>
        /// <param name="oListUserBlock">Reference to the list of user program blocks to populate.</param>
        /// <param name="sError">Outputs any error message encountered during enumeration.</param>
        /// <returns>True if enumeration succeeded, otherwise false.</returns>
        public bool EnumerateBlockUserProgramForPlc(PlcSoftware controllertarget, ref List<PlcBlock> oListUserBlock, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            oListUserBlock.Clear();

            try
            {
                foreach (PlcBlock UserBlock in controllertarget.BlockGroup.Blocks)
                {
                    oListUserBlock.Add(UserBlock);
                }
            }
            catch (Exception e)
            {
                sError = string.Format("Exception dans énumération du répertoire '{0}' '{1}'", controllertarget.BlockGroup.Name, e.Message);
                bRet = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Lists all program blocks in a specific user folder.
        /// </summary>
        /// <param name="plcblocfolder">Source user folder to enumerate.</param>
        /// <param name="oListUserBlock">Reference to the list of user program blocks to populate.</param>
        /// <param name="sError">Outputs any error message encountered during enumeration.</param>
        /// <returns>True if enumeration succeeded, otherwise false.</returns>
        public bool EnumerateBlockUserProgramForThisFolder(PlcBlockUserGroup plcblocfolder, ref List<PlcBlock> oListUserBlock, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            oListUserBlock.Clear();

            try
            {
                foreach (PlcBlock UserBlock in plcblocfolder.Blocks)
                {
                    oListUserBlock.Add(UserBlock);
                }
            }
            catch (Exception e)
            {
                sError = string.Format("Exception dans énumération du répertoire '{0}' '{1}'", plcblocfolder.Name, e.Message);
                bRet = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Exports a PLC block to an XML file.
        /// </summary>
        /// <param name="bloc">The block to export.</param>
        /// <param name="sXMLFileExport">The XML file path for export.</param>
        /// <param name="sError">Outputs any error message encountered during export.</param>
        /// <returns>True if export succeeded, otherwise false.</returns>
        public bool ExportBlocToXml(PlcBlock bloc, string sXMLFileExport, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    FileInfo fileinfo = new FileInfo(sXMLFileExport);
                    bloc.Export(fileinfo, ExportOptions.WithDefaults);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction ExportBlocToXml() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }
        /// <summary>
        /// Recursively enumerates all subfolders in a UDT folder to find a UDT by name.
        /// </summary>
        /// <param name="userdatatypefolder">Composition of UDT user groups to scan.</param>
        /// <param name="sUTDName">The UDT name to search for (upper case expected).</param>
        /// <returns>The matching <see cref="PlcType"/> if found, otherwise null.</returns>
        private PlcType EnumSubFolderInFolderOfDatatype(PlcTypeUserGroupComposition userdatatypefolder, string sUTDName)
        {
            PlcType oUdt = null;

            foreach (PlcTypeUserGroup _userdatatypefolder in userdatatypefolder)
            {
                oUdt = EnumerateUserDataTypeInFolder(_userdatatypefolder, sUTDName);
                if (oUdt != null) break;
                EnumSubFolderInFolderOfDatatype(_userdatatypefolder.Groups, sUTDName);
                if (oUdt != null) break;
            }

            return oUdt;
        }

        /// <summary>
        /// Enumerates user datatypes in a UDT folder and returns the matching UDT by name if found.
        /// </summary>
        /// <param name="userdatatypefolder">The UDT user group to scan.</param>
        /// <param name="sUTDName">The UDT name to search for (upper case expected).</param>
        /// <returns>The matching <see cref="PlcType"/> if found, otherwise null.</returns>
        private PlcType EnumerateUserDataTypeInFolder(PlcTypeUserGroup userdatatypefolder, string sUTDName)
        {
            PlcType oUdt = null;

            foreach (PlcType userdatatype in userdatatypefolder.Types)
            {
                if (userdatatype.Name.ToUpper() == sUTDName)
                {
                    oUdt = userdatatype;
                    break;
                }
            }
            return oUdt;
        }

        /// <summary>
        /// Imports a PLC block into the program folder from an XML file.
        /// </summary>
        /// <param name="blocs">The block composition object representing the destination in the project.</param>
        /// <param name="sXMLFileImport">The XML file path of the block to import.</param>
        /// <param name="sError">Outputs any error message encountered during the import.</param>
        /// <returns>True if the import succeeded, otherwise false.</returns>
        public bool ImportBlocFromXml(PlcBlockComposition blocs, string sXMLFileImport, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                if (File.Exists(sXMLFileImport) == false)
                {
                    sError = string.Format("Fichier importation XML inexistant '{0}'", sXMLFileImport);
                    bRet = false;
                }
                try
                {
                    FileInfo fileinfo = new FileInfo(sXMLFileImport);
                    blocs.Import(fileinfo, ImportOptions.Override);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction ImportBlocFromXml() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Compiles the specified PLC block.
        /// </summary>
        /// <param name="bloc">The block to compile.</param>
        /// <param name="sError">Outputs any error message encountered during compilation.</param>
        /// <returns>True if compilation succeeded, otherwise false.</returns>
        public bool CompileBloc(PlcBlock bloc, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    ICompilable compileService = bloc.GetService<ICompilable>();
                    CompilerResult result = compileService.Compile();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction CompileBloc() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Generates blocks in the TIA Portal project from a given external source.
        /// </summary>
        /// <param name="source">The external source file to use for block generation.</param>
        /// <param name="sError">Outputs any error message encountered during generation.</param>
        /// <returns>True if blocks were generated successfully, otherwise false.</returns>
        public bool GenerateBlocFromSourceFile(PlcExternalSource source, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    source.GenerateBlocksFromSource();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction GenerateBlocFromSourceFile() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Inserts a source into the TIA Portal project from an external file, creating a new external source.
        /// </summary>
        /// <param name="externalsourcesystemFolder">The external source system group in which to insert the source.</param>
        /// <param name="sSourceName">The name to assign to the source.</param>
        /// <param name="sFilePathSource">The path to the external source file.</param>
        /// <param name="SourceImport">Outputs the created <see cref="PlcExternalSource"/> object.</param>
        /// <param name="sError">Outputs any error message encountered during import.</param>
        /// <returns>True if the source was imported successfully, otherwise false.</returns>
        public bool ImportSourceFileToSourceFolder(
            PlcExternalSourceSystemGroup externalsourcesystemFolder,
            string sSourceName,
            string sFilePathSource,
            ref PlcExternalSource SourceImport,
            ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    SourceImport = externalsourcesystemFolder.ExternalSources.CreateFromFile(sSourceName, sFilePathSource);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction ImportSourceFileToSourceFolder() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }
    }
    /// <summary>
    /// Fournit des méthodes utilitaires pour interroger les instances TIA Portal actuellement actives sur le système.
    /// </summary>
    public static class HMATIAOpennessCurrentInstance
    {
        /// <summary>
        /// Retourne la liste de tous les processus TIA Portal actuellement actifs sur la machine.
        /// </summary>
        /// <returns>Liste des instances <see cref="TiaPortalProcess"/> trouvées.</returns>
        public static List<TiaPortalProcess> GetCurrentTiaPortalInstance()
        {
            List<TiaPortalProcess> list = new List<TiaPortalProcess>();

            foreach (TiaPortalProcess tiaportalprocess in TiaPortal.GetProcesses())
            {
                list.Add(tiaportalprocess);
            }

            return list;
        }
    }
}