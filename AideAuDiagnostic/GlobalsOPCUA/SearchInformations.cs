using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GlobalsOPCUA
{
    /// <summary>
    /// Classe contenant les informations de recherche dans le 
    /// projet Tia Portal de l'automate 
    /// </summary>
    public class PLC_ProjectDefinitions
    {
        #region constantes

        private const string sFamilyBlocMarckDef = @"OPCUA:";
        private const string sCommentarBlocParameterMarckDef = @"OPCUA:";
        private const string sFamillyStrResearchNewBlocNameDef = @"$";
        private const string sCommentarTagVariableMarckDef = @"OPCUA:";

        private const string sRootFolderBlocOPCUAServerDef = @"Blocks";
        private const string sRootFolderTagsOPCUAServerDef = @"Tags";

        #endregion

        #region Variables

        // indique soi l'interface de Tia Portal est visible
        public bool bWithUITiaPortal { get; set; }

        // Nom du répertoire pour la partie openness
        private string m_sOpennessLibraryPath;
        public string GetOpennessLibraryPath() { return m_sOpennessLibraryPath; }
        public void SetOpennessLibraryPath(string sOpennessLibraryPath) { m_sOpennessLibraryPath = sOpennessLibraryPath; }

        public string sExtensionProjectName = @"ap16";  // Extension project name Tia Portal

        // Marque de repere des blocs OPC UA au niveau des blocs
        public string sFamilyBlocMarck { get; set; } = sFamilyBlocMarckDef;

        // String parameter define new bloc name
        public string sFamillyStrResearchNewBlocName { get; set; } = sFamillyStrResearchNewBlocNameDef;

        // Marque de repere des paramètres d'un bloc pour accès OPC UA
        public string sCommentarBlocParameterMarck { get; set; } = sCommentarBlocParameterMarckDef;

        // Marque de repère d'un tag pour accès OPC UA
        public string sCommentarTagVariableMarck { get; set; } = sCommentarTagVariableMarckDef;

        // Nom de l'utilisateur pour accès au projet TIA portal
        private string m_sUserName;
        public string GetUserName() { return m_sUserName; }
        public void SetUserName(string sUserName) { m_sUserName = sUserName; }

        // Mot de passe de l'utilisateur déchiffré
        private string m_sUncryptPasswordUser;
        public string GetUncryptPasswordUser() { return m_sUncryptPasswordUser; }
        public void SetUncryptPasswordUser(string sCryptPasswordUser)
        {
            // Déchiffrement du mot de passe            
            CryptString crypt = new CryptString(true);
            m_sUncryptPasswordUser = crypt.Decrypt(sCryptPasswordUser);
        }

        // Nom du premier folder pour la définition des blocs dans l'arcborescence du serveur OPC UA
        public string sRootFolderBlocOPCUAServer { get; set; } = sRootFolderBlocOPCUAServerDef;

        // Nom du premier folder pour la définition des tags dans l'arcborescence du serveur OPC UA
        public string sRootFolderTagsOPCUAServer { get; set; } = sRootFolderTagsOPCUAServerDef;

        // Nom de la station S5-1500 H cible pour l'accès en OPC UA
        public string sPLCS71500HTargetStationName { get; set; }

        // Nom de la station PLC S7-1500 gateway numéro 1
        public string sPLCS71500GatewayStationName_01 { get; set; }

        // Nom de la station PLC S7-1500 gateway numéro 2
        public string sPLCS71500GatewayStationName_02 { get; set; }

        // Chemin complet de l'application
        public string sPathApplication { get; set; }

        // Nom du DB OPC UA de mapping
        public string sDBNameMappingOPCUA { get; set; }

        // Namespace pour le serveur OPC UA
        public string sOPCUAServerNamespace { get; set; }

        // Flag de validation de la génération des nodeid avec le nouveau dans la famille
        public bool bNewNameBlockOnlyForTagNodeId { get; set; }

        #endregion

        /// <summary>
        /// Constructeur par defaut de la classe
        /// </summary>
        public PLC_ProjectDefinitions()
        {
        }

        /// <summary>
        /// Constructeur évolué de la classe
        /// <paramref name="sOpennessLibraryPath"> Chemin du répertoire pour la partie openness </paramref>/>
        /// <paramref name="sProjectUserName"> UserName pour accès au projet Tia </paramref>/>
        /// <paramref name="sCryptPasswordUser"> Mot de passe chiffré pour l'utilisateur </paramref>/>
        /// </summary>
        public PLC_ProjectDefinitions(string sOpennessLibraryPath, string sUserName, string sCryptPasswordUser)
        {
            m_sOpennessLibraryPath = sOpennessLibraryPath;
            m_sUserName = sUserName;
            // Déchiffrement du mot de passe            
            CryptString crypt = new CryptString(true);
            m_sUncryptPasswordUser = crypt.Decrypt(sCryptPasswordUser);
        }

    }
}
