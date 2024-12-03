using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Runtime.CompilerServices;

using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Library;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Blocks.Interface;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;

namespace OpennessV16
{
    public class HMATIAOpenness_V16
    {

        private TiaPortal m_oTiaPortal;

        public Project m_oTiaProject;

        private UserGlobalLibrary m_oTiaGlobalLibrary;

        private bool m_bIsConnected = false;

        private string m_sTIAPortalProject = string.Empty;
        private bool m_bTiaProjectIsOpen = false;

        // Utilisateur dans le cas d'un projet protégé par UMAC
        public static string m_sUserName;
        public static string m_sPassword;

        private string m_sTIAGlobalLibrary = string.Empty;
        private bool m_bTiaGlobalLibraryIsOpen = false;


        public enum TargetDevice { TargetDeviceNotDefine = 0, TargetPLC = 1, TagHMI = 2 };

        /// <summary>
        /// Indique si la connexion avec TIA Portal est établie
        /// </summary>
        /// <returns></returns>
        public bool GetConnected() { return (m_bIsConnected); }

        /// <summary>
        /// Indique le nom du projet TIA Portal sélectionné
        /// </summary>
        /// <returns></returns>
        public string GetTIAPortalProject() { return (m_sTIAPortalProject); }

        /// <summary>
        /// Permet de retouner le projet TIA en cours de traitement
        /// </summary>
        /// <returns></returns>
        public Project GetCurrentTIAProject() { return (m_oTiaProject); }

        /// <summary>
        /// Permet de retourner la library du projet
        /// </summary>
        /// <returns></returns>
        public ProjectLibrary GetCurrentTIALibraryProject() { return (m_oTiaProject.ProjectLibrary); }

        /// <summary>
        /// Permet de fixer le nom du projet TIA Portal à exploiter
        /// </summary>
        /// <param name="sTiaPortalProject"></param>
        public void SetTIAPortalProject(string sTiaPortalProject) { m_sTIAPortalProject = sTiaPortalProject; }

        public bool GetTiaProjectIsOpen() { return m_bTiaProjectIsOpen; }


        /// <summary>
        /// Permet de fixer le nom de la library globale ouverte
        /// </summary>
        /// <param name="sTIAGlobalLibrary"></param>
        public void SetTIAGlobalLibrary(string sTIAGlobalLibrary) { m_sTIAGlobalLibrary = sTIAGlobalLibrary; }

        /// <summary>
        /// Permet de retourner la library globale ouverte
        /// </summary>
        /// <returns></returns>
        public UserGlobalLibrary GetTIAGlobalLibrary() { return m_oTiaGlobalLibrary; }

        private void Confirmation(object sender, ConfirmationEventArgs e)
        {
            e.IsHandled = true;
        }

        /// <summary>
        /// constructeur par defaut;
        /// </summary>
        public HMATIAOpenness_V16()
        {
        }

        /// <summary>
        /// Constructeur par défaut de la classe
        /// </summary>
        /// <param name="bOpenTIAPortal"> Indique si l'éditeur du TIA Portal doit être ouvert</param>
        /// <param name="sError"> Texte de l'erreur si exception lors de la connexion </param>
        public HMATIAOpenness_V16(bool bOpenTIAPortal, ref string sError)
        {
            sError = string.Empty;
            m_bIsConnected = true;
            try
            {
                // Instanciation de la classe de connexion sur TIA Portal
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
        /// Attachement à un projet Tia portal en cours de traitement
        /// </summary>
        /// <param name="tiaprocess"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool AttachTiaPortalInstance(TiaPortalProcess tiaprocess, ref string sError)
        {
            bool bRet = true;

            sError = string.Empty;
            m_bIsConnected = true;
            try
            {
                // Affectation du projet Tia Portal déja ouvert
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
        /// Permet de se deconnecter de Tia Portal
        /// </summary>
        public void DisconnectFromTiaPortal()
        {
            try
            {
                if (m_bIsConnected == true) m_oTiaPortal.Dispose();
            }
            catch { };
        }

        /// <summary>
        /// Permet d'ouvrir le projet TIA Portal configuré
        /// </summary>
        /// <param name="sUserName"> Utilisateur </param>
        /// <param name="sPassword"> Mot de passe </param>
        /// <param name="sErrorText"> Texte d'erreur si problème d'ouverture </param>
        /// <param name="bCriticalError"> Indique si une erreur critique est apparue </param>
        /// <returns></returns>
        public bool OpenTIAProject(string sUserName, string sPassword, ref string sErrorText, ref bool bCriticalError)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            bCriticalError = false;

            while (true)
            {
                m_sUserName = sUserName;
                m_sPassword = sPassword;

                // Test si la connexion avec une instance de TIA est établie ?
                if ((m_bIsConnected == false) || (m_oTiaPortal == null))
                {
                    sErrorText = "Pas de connexion avec une instance de TIA Portal";
                    bRet = false;
                    break;
                }
                // Test si le nom du projet est cohérent ?
                if (File.Exists(m_sTIAPortalProject) == false)
                {
                    sErrorText = string.Format("Projet TIA Portal {0} incorrect", m_sTIAPortalProject);
                    bRet = false;
                    break;
                }
                // Tentative d'ouverture du projet TIA Portal
                try
                {
                    FileInfo fileinfo = new FileInfo(m_sTIAPortalProject);

                    // Test si une protection UMAC est spécifiée ?
                    if (sUserName.Length == 0)
                    {
                        // Connexion sur projet non sécurisé par UMAC
                        m_oTiaProject = m_oTiaPortal.Projects.Open(fileinfo);
                    }
                    else
                    {
                        // Connexion sur projet sécurisé par UMAC
                        m_oTiaProject = m_oTiaPortal.Projects.Open(fileinfo, MyUmacDelegate);
                    }
                    m_bTiaProjectIsOpen = true;
                }
                // Test si exception Siemens interne
                catch (Siemens.Engineering.EngineeringException e)
                {
                    // Exception Siemens à traiter
                    sErrorText = string.Format("Problème ouverture projet '{0}'", e.Message);
                    m_bTiaProjectIsOpen = false;
                    bRet = false;
                    break;
                }
                catch (Exception e)
                {
                    // Exception critique avec probablement crash de TIA Portal
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
        /// Délégué pour la connexion sécurisée
        /// </summary>
        /// <param name="umacCredentials"></param>
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
        /// Permet de se connecter au projet TIA Portal en cours de traitement 
        /// sur l'instance choisie
        /// </summary>
        /// <param name="sErrorText"></param>
        /// <param name="bCriticalError"></param>
        /// <returns></returns>
        public bool OpenCurrentTIAProjectFromInstance(ref string sErrorText, ref bool bCriticalError)
        {
            bool bRet = true;
            sErrorText = string.Empty;
            bCriticalError = false;

            while (true)
            {

                // Test si la connexion avec une instance de TIA est établie ?
                if ((m_bIsConnected == false) || (m_oTiaPortal == null))
                {
                    sErrorText = "Pas de connexion avec une instance de TIA Portal";
                    bRet = false;
                    break;
                }
                // Test si le nom du projet est cohérent ?
                if (File.Exists(m_sTIAPortalProject) == false)
                {
                    sErrorText = string.Format("Projet TIA Portal {0} incorrect", m_sTIAPortalProject);
                    bRet = false;
                    break;
                }
                // Tentative d'affectation du projet TIA Portal
                try
                {
                    // On récupère le projet en cours
                    foreach (Project projet in m_oTiaPortal.Projects)
                    {
                        m_oTiaProject = projet;
                    }
                    m_bTiaProjectIsOpen = true;
                }
                // Test si exception Siemens interne
                catch (Siemens.Engineering.EngineeringException e)
                {
                    // Exception Siemens à traiter
                    sErrorText = string.Format("Problème ouverture projet '{0}'", e.Message);
                    m_bTiaProjectIsOpen = false;
                    bRet = false;
                    break;
                }
                catch (Exception e)
                {
                    // Exception critique avec probablement crash de TIA Portal
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
        /// Permet de fermer le projet TIA Portal en cours de traitement
        /// </summary>
        /// <param name="sErrorText"></param>
        /// <returns></returns>
        public bool CloseTIAProject(ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            try
            {
                while (true)
                {
                    // Test si un projet est ouvert ?
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sErrorText = "Projet TIA Portal pas ouvert";
                        bRet = false;
                        break;
                    }
                    // Test si l'instance du projet et de TIA sont corrects
                    if ((m_oTiaPortal != null) && (m_oTiaProject != null))
                    {
                        m_oTiaProject.Close();
                        m_oTiaProject = null;
                    }
                    else
                    {
                        sErrorText = "Instances de TIA ou du projet incorrectes";
                        bRet = false;
                    }

                    break;
                }
            }
            finally
            {
                m_bTiaProjectIsOpen = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de sauvegarder les modifications du projet TIA en cours
        /// </summary>
        /// <param name="sErrorText"></param>
        /// <returns></returns>
        public bool SaveTIAProject(ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            try
            {
                while (true)
                {
                    // Test si un projet est ouvert ?
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sErrorText = "Projet TIA Portal pas ouvert";
                        bRet = false;
                        break;
                    }
                    // Test si l'instance du projet et de TIA sont corrects
                    if ((m_oTiaPortal != null) && (m_oTiaProject != null))
                    {
                        m_oTiaProject.Save();
                    }
                    else
                    {
                        sErrorText = "Instances de TIA ou du projet incorrectes";
                        bRet = false;
                    }

                    break;
                }
            }
            catch (Exception e)
            {
                sErrorText = string.Format("Exception dans SaveTIAProject '{0}'", e.Message);
                bRet = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de sauvegarder les modifications du projet TIA en cours
        /// dans un nouveau projet
        /// </summary>
        /// <param name="sNewTiaPortalProject"> Nouveau nom du projet TIA Portal Path\NomProjet sans extension </param>
        /// <param name="sErrorText"></param>
        /// <returns></returns>
        public bool SaveTIAProjectAs(string sNewTiaPortalProjectPath, ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            try
            {
                while (true)
                {
                    // Test si un projet est ouvert ?
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sErrorText = "Projet TIA Portal pas ouvert";
                        bRet = false;
                        break;
                    }
                    // Test si l'instance du projet et de TIA sont corrects
                    if ((m_oTiaPortal != null) && (m_oTiaProject != null))
                    {
                        DirectoryInfo newdirectory = new DirectoryInfo(sNewTiaPortalProjectPath);
                        m_oTiaProject.SaveAs(newdirectory);
                    }
                    else
                    {
                        sErrorText = "Instances de TIA ou du projet incorrectes";
                        bRet = false;
                    }

                    break;
                }
            }
            catch (Exception e)
            {
                sErrorText = string.Format("Exception dans SaveTIAProjectAs '{0}'", e.Message);
                bRet = false;
            }
            return (bRet);
        }


        /// <summary>
        /// Permet d'ouvrir la library globale spécifiée
        /// </summary>
        /// <param name="sErrorText"></param>
        /// <returns></returns>
        public bool OpenGlobalLibrary(ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            while (true)
            {
                // Test si la connexion avec une instance de TIA est établie ?
                if ((m_bIsConnected == false) || (m_oTiaPortal == null))
                {
                    sErrorText = "Pas de connexion avec une instance de TIA Portal";
                    bRet = false;
                    break;
                }
                // Test si le nom de la library globale est cohérent ?
                if (File.Exists(m_sTIAGlobalLibrary) == false)
                {
                    sErrorText = string.Format("Library globale TIA Portal {0} incorrect", m_sTIAGlobalLibrary);
                    bRet = false;
                    break;
                }
                // Tentative d'ouverture de la library globale
                try
                {
                    FileInfo fileinfo = new FileInfo(m_sTIAGlobalLibrary);
                    // Ouverture de la library globale en mode readonly
                    m_oTiaGlobalLibrary = m_oTiaPortal.GlobalLibraries.Open(fileinfo, OpenMode.ReadOnly);
                    m_bTiaGlobalLibraryIsOpen = true;
                }
                // Test si exception Siemens interne
                catch (Siemens.Engineering.EngineeringException e)
                {
                    // Exception Siemens à traiter
                    sErrorText = string.Format("Problème ouverture library globale '{0}'", e.Message);
                    m_bTiaGlobalLibraryIsOpen = false;
                    bRet = false;
                    break;
                }
                catch (Exception e)
                {
                    // Exception critique avec probablement crash de TIA Portal
                    sErrorText = string.Format("Problème critique ouverture library globale '{0}'", e.Message);
                    bRet = false;
                    m_bTiaGlobalLibraryIsOpen = false;
                    m_oTiaGlobalLibrary = null;
                    break;
                }
                break;
            }
            return (bRet);
        }


        /// <summary>
        /// Permet de fermer la lirary globaleTIA Portal en cours de traitement
        /// </summary>
        /// <param name="sErrorText"></param>
        /// <returns></returns>
        public bool CloseGlobalLibrary(ref string sErrorText)
        {
            bool bRet = true;
            sErrorText = string.Empty;

            try
            {
                while (true)
                {
                    // Test si une library globale est ouverte ?
                    if (m_bTiaGlobalLibraryIsOpen == false)
                    {
                        sErrorText = "Library globale TIA Portal pas ouverte";
                        bRet = false;
                        break;
                    }
                    // Test si l'instance de la library globale et de TIA sont corrects
                    if ((m_oTiaGlobalLibrary != null) && (m_oTiaProject != null))
                    {
                        m_oTiaGlobalLibrary.Close();
                        m_oTiaGlobalLibrary = null;
                    }
                    else
                    {
                        sErrorText = "Instances de TIA ou de la library globale incorrectes";
                        bRet = false;
                    }

                    break;
                }
            }
            finally
            {
                m_bTiaGlobalLibraryIsOpen = false;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet d'énumérer la liste des devices dans un projet TIA Portal
        /// </summary>
        /// <param name="ListDevice"> Liste des devices présents dans le projet </param>
        /// <param name="sError"> Texte d'erreur </param>
        /// <returns></returns>
        public bool EnumerationDevice(ref List<Device> ListDevice, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            DeviceComposition deviceaggregation = m_oTiaProject.Devices;

            try
            {
                while (true)
                {
                    // Test si un projet est ouvert ?
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sError = "Pas de projet TIA ouvert";
                        bRet = false;
                        break;
                    }
                    // Enumération de tous les devices contenus dans le projet
                    foreach (Device device in deviceaggregation)
                    {

                        // FILTRE DU TYPE DE STATION
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
        /// Permet de lister la liste des items dans un device
        /// </summary>
        /// <param name="device"> Device a énumérer pour les items </param>
        /// <param name="ListDevice"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool EnumerationDeviceItems(Device device, ref List<DeviceItem> ListDeviceItem, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            DeviceItemComposition deviceItemAggregation = device.DeviceItems;

            try
            {
                while (true)
                {
                    // Test si un projet est ouvert ?
                    if (m_bTiaProjectIsOpen == false)
                    {
                        sError = "Pas de projet TIA ouvert";
                        bRet = false;
                        break;
                    }
                    // Boucle de balayage des items pour ce device
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
        /// Permet de retourner le type de Target pour le device
        /// </summary>
        /// <param name="deviceItem"> ItemDevice a tester </param>
        /// <param name="iItemTypeDeviceForPlcTarget"> Type de l'item dans le cas d'un target de type PLC </param>
        /// <returns></returns>
// Ophelie        public TargetDevice GetTarget(DeviceItem deviceItem, ref DeviceItemType iItemTypeDeviceForPlcTarget)
        public TargetDevice GetTarget(DeviceItem deviceItem)
        {
            TargetDevice iTargetDevice = TargetDevice.TargetDeviceNotDefine;

            while (true)
            {
                // Test si c'est une target de type automate ?
                SoftwareContainer softwarecontainer = deviceItem.GetService<SoftwareContainer>() as SoftwareContainer;
                PlcSoftware controllerTarget = softwarecontainer.Software as PlcSoftware;
                // Test si la target n'est pas nul ?
                if (controllerTarget != null)
                {
                    iTargetDevice = TargetDevice.TargetPLC;
                    //                    iItemTypeDeviceForPlcTarget = deviceItem.DeviceItemType;
                    break;
                }
                // Test si c'est une target de type HMI ?

                HmiTarget hmitarget = softwarecontainer.Software as HmiTarget;
                if (hmitarget != null)
                {
                    iTargetDevice = TargetDevice.TagHMI;
                    break;
                }
                else
                {
                    iTargetDevice = TargetDevice.TargetPLC;
                    //                    iItemTypeDeviceForPlcTarget = deviceItem.DeviceItemType;
                }


                break;
            }
            return (iTargetDevice);
        }

        /// <summary>
        /// Permet de retourner le type du device item dans le cas d'une target de type PLC
        /// </summary>
        /// <param name="iItemTypeDeviceForPlcTarget"></param>
        /// <returns></returns>
/*
        public string ConvertTypeDeviceForPlcTargetToString(DeviceItemType iItemTypeDeviceForPlcTarget)
        {
            int iBit = 0;
            string sDeviceItem = string.Empty;
            string sType = string.Empty;
            int iMask = 1;

            for (iBit = 0; iBit < 24; iBit++)
            {
                iMask = 1;
                iMask = iMask << iBit;
                iMask = (int)(iItemTypeDeviceForPlcTarget) & iMask;

                switch ((DeviceItemType)iMask)
                {
                    case DeviceItemType.None: sType = string.Empty; break;
                    case DeviceItemType.Rack: sType = "Rack"; break;
                    case DeviceItemType.Module: sType = "Module"; break;
                    case DeviceItemType.Submodule: sType = "Submodule"; break;
                    case DeviceItemType.CPU: sType = "CPU"; break;
                    case DeviceItemType.FM: sType = "FM"; break;
                    case DeviceItemType.CP: sType = "CP"; break;
                    case DeviceItemType.PS: sType = "PS"; break;
                    case DeviceItemType.IM: sType = "IM"; break;
                    case DeviceItemType.AddressContainer: sType = "AddressContainer"; break;
                    case DeviceItemType.DPMasterIOController: sType = "DPMasterIOController"; break;
                    case DeviceItemType.Headmodule: sType = "Headmodule"; break;
                    case DeviceItemType.VirtualSlave: sType = "VirtualSlave"; break;
                    case DeviceItemType.Interface: sType = "Interface"; break;
                    case DeviceItemType.Port: sType = "Port"; break;
                    case DeviceItemType.IOSystemConnector: sType = "IOSystemConnector"; break;
                    case DeviceItemType.VirtualObject: sType = "VirtualObject"; break;
                    case DeviceItemType.SubSubmodule: sType = "SubSubmodule"; break;
                    case DeviceItemType.MastersystemCarrier: sType = "MastersystemCarrier"; break;
                    default: sType = string.Empty; break;
                }
                if ((sDeviceItem.Length != 0) && (sType.Length != 0))
                {
                    sDeviceItem = sDeviceItem + "|" + sType;
                }
                else if ((sDeviceItem.Length == 0) && (sType.Length != 0))
                {
                    sDeviceItem = sType;
                }
            }
            return (sDeviceItem);
        }
*/
        /// <summary>
        /// Permet de lister tous les blocs programmes du répertoire root programme
        /// </summary>
        /// <param name="controllertarget"> Controller Plc </param>        
        /// <param name="oListUserBlock"> Liste des programmes utilisateur pour le répertoire </param>
        /// <param name="sError"></param>
        /// <returns></returns>
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
        /// Permet de lister tous les blocs programmes d'un user folder
        /// </summary>
        /// <param name="plcblocfolder"> Répertoire source </param>
        /// <param name="oListUserBlock"> Liste des programmes utilisateur pour le répertoire </param>
        /// <param name="sError"></param>
        /// <returns></returns>
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
        /// Permet de retourner le répertoire contenant l'ensemble des blocs systèmes + utilisateurs
        /// </summary>
        /// <param name="deviceItem"></param>
        /// <returns></returns>
        public PlcBlockSystemGroup GetProgramblockSystemFolderForThisControlTarget(DeviceItem deviceItem)
        {
            try
            {
                // Test si c'est une target de type automate ?
                SoftwareContainer softwarecontainer = deviceItem.GetService<SoftwareContainer>() as SoftwareContainer;
                PlcSoftware controllertarget = softwarecontainer.Software as PlcSoftware;

                if (controllertarget != null)
                {
                    return (controllertarget.BlockGroup);
                }
            }
            catch
            {
                return null;
            }
            return null;
        }


        /// <summary>
        /// Exportation d'un bloc au format XML
        /// </summary>
        /// <param name="bloc"> Bloc a exporter </param>
        /// <param name="sXMLFileExport"> Nom du fichier XML pour l'exportation </param>
        /// <param name="sError"> Texte erreur </param>
        /// <returns></returns>
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
                    //                    bloc.Export(sXMLFileExport, ExportOptions.None);
                    //                    bloc.Export(sXMLFileExport, ExportOptions.WithReadOnly);
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
        /// Permet de récupérer l'objet UDT par son nom
        /// </summary>
        /// <param name="sUTDName"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public PlcType GetUDTByName(PlcSoftware controllertarget, string sUTDName, ref string sError)
        {
            PlcType oUdt = null;
            sError = string.Empty;
            PlcTypeSystemGroup controllerdatatypesystemfolder;

            while (true)
            {
                try
                {
                    controllerdatatypesystemfolder = controllertarget.TypeGroup;
                    oUdt = TrytoFoundUDTByName(controllerdatatypesystemfolder, sUTDName);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction GetUDTByName() '{0}'", e.Message);
                    oUdt = null;
                }
                break;
            }

            return oUdt;
        }

        /// <summary>
        /// Permet de rechercher une UDT en fonction de son dans le répertoire des UDT
        /// </summary>
        /// <param name="controllerdatatypesystemfolder"></param>
        /// <param name="sUTDName"></param>
        /// <returns></returns>
        private PlcType TrytoFoundUDTByName(PlcTypeSystemGroup controllerdatatypesystemfolder, string sUTDName)
        {
            PlcType oUdt = null;
            bool bFound = false;
            string sUTDNameUpper = sUTDName.ToUpper();

            while (true)
            {
                // Boucle de balayage des blocs type s'ils existent sous la racine
                foreach (PlcType userdatatype in controllerdatatypesystemfolder.Types)
                {
                    // Test si l'on se trouve sur la bonne UDT ?
                    if (userdatatype.Name.ToUpper() == sUTDNameUpper)
                    {
                        bFound = true;
                        oUdt = userdatatype;
                        break;
                    }
                }

                if (bFound == true) break;

                // Boucle de balayage de tous les folders sous la racine
                foreach (PlcTypeUserGroup userdatatypefolder in controllerdatatypesystemfolder.Groups)
                {
                    // Boucle de balayage des blocs type s'ils existent
                    oUdt = EnumerateUserDataTypeInFolder(userdatatypefolder, sUTDNameUpper);
                    if (oUdt != null) break;
                    // Boucle de balayage de tous les répertoires de type userdatatype
                    oUdt = EnumSubFolderInFolderOfDatatype(userdatatypefolder.Groups, sUTDNameUpper);
                    if (oUdt != null) break;
                }

                break;
            }

            return oUdt;
        }

        /// <summary>
        /// Permet de balayer les subfolder dans un folder UDT
        /// </summary>
        /// <param name="userdatatypefolder"></param>
        /// <param name="sUTDName"></param>
        /// <returns></returns>
        private PlcType EnumSubFolderInFolderOfDatatype(PlcTypeUserGroupComposition userdatatypefolder, string sUTDName)
        {
            PlcType oUdt = null;

            foreach (PlcTypeUserGroup _userdatatypefolder in userdatatypefolder)
            {
                // Enumération de tous les userbloctype présents dans le répertoire
                oUdt = EnumerateUserDataTypeInFolder(_userdatatypefolder, sUTDName);
                if (oUdt != null) break;
                EnumSubFolderInFolderOfDatatype(_userdatatypefolder.Groups, sUTDName);
                if (oUdt != null) break;
            }

            return oUdt;
        }

        /// <summary>
        /// Permet d'énumérer les datatypes dans un répertoire de datatype et de tester si notre UDT
        /// est définie dans ce répertoire
        /// </summary>
        /// <param name="userdatatypefolder"></param>
        /// <param name="treenodesource"></param>
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
        /// Permet d'exporter un UDT au format XML
        /// </summary>
        /// <param name="userdatatype"></param>
        /// <param name="sXMLFileExport"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool ExportUDTToXml(PlcType userdatatype, string sXMLFileExport, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    FileInfo fileinfo = new FileInfo(sXMLFileExport);
                    userdatatype.Export(fileinfo, ExportOptions.WithDefaults);
                    //                    bloc.Export(sXMLFileExport, ExportOptions.None);
                    //                    bloc.Export(sXMLFileExport, ExportOptions.WithReadOnly);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction ExportUDTToXml() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }

            return (bRet);
        }

        /// <summary>
        /// Permet d'importer un bloc dans le répertoire programme associé
        /// </summary>
        /// <param name="blocs"> Zone Blocks du projet </param>
        /// <param name="sXMLFileImport"> Nom du fichier XML du bloc a importer </param>
        /// <param name="sError"> Texte d'erreur </param>
        /// <returns></returns>
        public bool ImportBlocFromXml(PlcBlockComposition blocs, string sXMLFileImport, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                // Test si le fichier existe ?
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
        /// Permet de compiler un bloc 
        /// </summary>
        /// <param name="bloc"> Bloc a compiler </param>
        /// <param name="sError"> Texte erreur </param>
        /// <returns></returns>
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
        /// Permet de supprimer un bloc 
        /// </summary>
        /// <param name="blocs"> Répertoire ou se trouve le bloc </param>
        /// <param name="bloc"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool DeleteBloc(PlcBlockComposition blocs, PlcBlock bloc, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    bloc.Delete();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction DeleteBloc() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de générer des blocs à partir d'une source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sError"></param>
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
        /// Permet d'insérer une source à partir d'un fichier externe
        /// </summary>
        /// <param name="externalsourcesystemFolder"></param>
        /// <param name="sSourceName"></param>
        /// <param name="sFilePathSource"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool ImportSourceFileToSourceFolder(PlcExternalSourceSystemGroup externalsourcesystemFolder, string sSourceName,
                                                   string sFilePathSource, ref PlcExternalSource SourceImport, ref string sError)
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


        /// <summary>
        /// Permet de supprimer une source dans le projet
        /// </summary>
        /// <param name="externalsourcesystemFolder"></param>
        /// <param name="sSourceName"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool DeleteExternalSource(PlcExternalSourceSystemGroup externalsourcesystemFolder, PlcExternalSource source, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    source.Delete();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction DeleteExternalSource() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet d'ouvrir le bloc dans l'éditeur de TIA Portal
        /// </summary>
        /// <param name="bloc"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool showBlockInEditor(PlcBlock bloc, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    bloc.ShowInEditor();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction showBlockInEditor() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de créer un folder dans l'arborescence
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="sFoldername"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool CreateFolder(PlcBlockUserGroupComposition folders, string sFoldername, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;

            while (true)
            {
                try
                {
                    folders.Create(sFoldername);
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction CreateFolder() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de supprimer un folder dans l'arborescence
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="foldername"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public bool DeleteFolder(PlcBlockUserGroupComposition folders, string foldername, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            PlcBlockUserGroup folder;

            while (true)
            {
                try
                {
                    // Recherche du folder dans la liste
                    folder = folders.Find(foldername);
                    folder.Delete();
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction DeleteFolder() '{0}'", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }

        /// <summary>
        /// Permet de récupérer les informations sur un device
        /// </summary>
        /// <param name="deviceItem"></param>
        /// <param name="sDeviceInformation"></param>
        /// <returns></returns>
        public bool GetInformationOnDeviceItem(DeviceItem deviceItem, ref string sDeviceInformation, ref string sError)
        {
            bool bRet = true;
            sError = string.Empty;
            sDeviceInformation = string.Empty;

            while (true)
            {
                try
                {
                    sDeviceInformation = sDeviceInformation + string.Format("Nom : {0}\r", deviceItem.Name);

                    string sAddressInformation = "";
                    AddressComposition addresscomposition = deviceItem.Addresses;
                    int cc = addresscomposition.Count;
                    foreach (Address adresse in addresscomposition)
                    {
                        AddressIoType iTypeAdresse = adresse.IoType;
                        switch (iTypeAdresse)
                        {
                            case AddressIoType.Input: sAddressInformation = sAddressInformation + "Adresse de type entrée\r"; break;
                            case AddressIoType.Output: sAddressInformation = sAddressInformation + "Adresse de type sortie\r"; break;
                        }
                        sDeviceInformation = sDeviceInformation + string.Format("Adresse = {0} Longueur = {1}\r", adresse.StartAddress, adresse.Length);

                        //                        IList<EngineeringAttributeInfo> attributeInfos = ((IEngineeringObject)deviceItem).GetAttributeInfos();
                        IList<EngineeringAttributeInfo> attributeInfos = adresse.GetAttributeInfos();

                        foreach (EngineeringAttributeInfo attributeInfo in attributeInfos)
                        {
                            sDeviceInformation = sDeviceInformation + string.Format("Attribute: {0} - AccessMode {1} valeur = {2}\r",
                                                                                     attributeInfo.Name,
                                                                                     attributeInfo.AccessMode,
                                                                                     adresse.GetAttribute(attributeInfo.Name).ToString());
                            switch (attributeInfo.AccessMode)
                            {
                                case EngineeringAttributeAccessMode.Read:
                                    sDeviceInformation = sDeviceInformation + string.Format("Attribute: {0} - Read Access valeur = {1}\r",
                                                                                            attributeInfo.Name,
                                                                                            adresse.GetAttribute(attributeInfo.Name).ToString());
                                    break;
                                case EngineeringAttributeAccessMode.Write:
                                    sDeviceInformation = sDeviceInformation + string.Format("Attribute: {0} - Write Access valeur = {1}\r",
                                                                                            attributeInfo.Name,
                                                                                            adresse.GetAttribute(attributeInfo.Name).ToString());
                                    break;
                                case EngineeringAttributeAccessMode.Read | EngineeringAttributeAccessMode.Write:
                                    sDeviceInformation = sDeviceInformation + string.Format("Attribute: {0} - Read and Write Access valeur = {1}\r",
                                                                                            attributeInfo.Name,
                                                                                            adresse.GetAttribute(attributeInfo.Name).ToString());
                                    break;
                            }
                        }
                    }

                    sDeviceInformation = sDeviceInformation + string.Format("Position : {0}\r", deviceItem.PositionNumber);
                    sDeviceInformation = sDeviceInformation + "******************** Liste des channels******************** \r";
                    sDeviceInformation = sDeviceInformation + "\r";

                    ChannelComposition channelcomposition = deviceItem.Channels;
                    foreach (Channel channel in channelcomposition)
                    {
                        string sChanneltype = string.Empty;

                        switch (channel.IoType)
                        {
                            case ChannelIoType.None:
                                sChanneltype = "Pas de type";
                                break;
                            case ChannelIoType.Input:
                                sChanneltype = "Type 'Entree'";
                                break;
                            case ChannelIoType.Output:
                                sChanneltype = "Type 'Sortie'";
                                break;
                            case ChannelIoType.Complex:
                                sChanneltype = "Type 'Complexe'";
                                break;
                        }

                        sDeviceInformation = sDeviceInformation + string.Format("Channel {0} number {1}\r", sChanneltype, channel.Number);
                        sDeviceInformation = sDeviceInformation + "Liste des attributs pour ce channel\r";
                        sDeviceInformation = sDeviceInformation + "\r";

                        // Boucle de traitement de tous les paramètres
                        foreach (EngineeringAttributeInfo attributinfo in channel.GetAttributeInfos())
                        {
                            string sAccessMode = string.Empty;
                            switch (attributinfo.AccessMode)
                            {
                                case EngineeringAttributeAccessMode.None:
                                    sAccessMode = "Acces none";
                                    break;
                                case EngineeringAttributeAccessMode.Read:
                                    sAccessMode = "Acces Read";
                                    break;
                                case EngineeringAttributeAccessMode.Write:
                                    sAccessMode = "Acces Write";
                                    break;
                                case EngineeringAttributeAccessMode.ReadWrite:
                                    sAccessMode = "Acces ReadWrite";
                                    break;
                            }
                            sDeviceInformation = sDeviceInformation + string.Format("attributinfo.Name = {0} ({1}) Valeur = {2}\r",
                                                                                     attributinfo.Name,
                                                                                     sAccessMode,
                                                                                     channel.GetAttribute(attributinfo.Name).ToString());
                        }

                    }

                    sDeviceInformation = sDeviceInformation + "******************** Liste des hwidentifier ******************** \r";
                    sDeviceInformation = sDeviceInformation + "\r";

                    HwIdentifierComposition hwidentifer = deviceItem.HwIdentifiers;
                    foreach (HwIdentifier hw in hwidentifer)
                    {
                        sDeviceInformation = sDeviceInformation + string.Format("hwidentifer = {0} \r", hw.Identifier);
                        sDeviceInformation = sDeviceInformation + "Liste des attributs pour ce hwidentifer\r";
                        sDeviceInformation = sDeviceInformation + "\r";

                        foreach (EngineeringAttributeInfo attributinfo in hw.GetAttributeInfos())
                        {
                            string sAccessMode = string.Empty;
                            switch (attributinfo.AccessMode)
                            {
                                case EngineeringAttributeAccessMode.None:
                                    sAccessMode = "Acces none";
                                    break;
                                case EngineeringAttributeAccessMode.Read:
                                    sAccessMode = "Acces Read";
                                    break;
                                case EngineeringAttributeAccessMode.Write:
                                    sAccessMode = "Acces Write";
                                    break;
                                case EngineeringAttributeAccessMode.ReadWrite:
                                    sAccessMode = "Acces ReadWrite";
                                    break;
                            }
                            sDeviceInformation = sDeviceInformation + string.Format("attributinfo.Name = {0} ({1}) Valeur = {2}\r",
                                                                                     attributinfo.Name,
                                                                                     sAccessMode,
                                                                                     hw.GetAttribute(attributinfo.Name).ToString());

                        }
                    }
                }
                catch (Exception e)
                {
                    sError = string.Format("Exception dans fonction GetInformationOnDeviceItem() '{0}'\r", e.Message);
                    bRet = false;
                }
                break;
            }
            return (bRet);
        }
    }
    public static class HMATIAOpennessCurrentInstance
    {
        /// <summary>
        /// Permet de retourner la liste des projets Tia portal en cours d'édition
        /// Attention : La méthode est statique pour pouvoir récupérer le bon projet
        /// </summary>
        /// <returns></returns>
        /// 

        
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