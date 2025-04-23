namespace GlobalsOPCUA
{
    /// <summary>
    /// Classe contenant les informations de configuration et de recherche dans le projet TIA Portal
    /// pour l'automate et le serveur OPC UA.
    /// </summary>
    public class PLC_ProjectDefinitions
    {
        #region Constantes

        private const string sFamilyBlocMarckDef = @"OPCUA:";
        private const string sCommentarBlocParameterMarckDef = @"OPCUA:";
        private const string sFamillyStrResearchNewBlocNameDef = @"$";
        private const string sCommentarTagVariableMarckDef = @"OPCUA:";

        private const string sRootFolderBlocOPCUAServerDef = @"Blocks";
        private const string sRootFolderTagsOPCUAServerDef = @"Tags";

        #endregion

        #region Variables

        /// <summary>
        /// Indique si l'interface graphique de TIA Portal doit être visible.
        /// </summary>
        public bool bWithUITiaPortal { get; set; }

        /// <summary>
        /// Nom du répertoire pour la partie Openness.
        /// </summary>
        private string m_sOpennessLibraryPath;
        /// <summary>
        /// Définit le chemin du répertoire Openness.
        /// </summary>
        public void SetOpennessLibraryPath(string sOpennessLibraryPath) { m_sOpennessLibraryPath = sOpennessLibraryPath; }

        /// <summary>
        /// Extension du nom du projet TIA Portal.
        /// </summary>
        public string sExtensionProjectName = @"ap16";

        /// <summary>
        /// Marque de repère des blocs OPC UA au niveau des blocs.
        /// </summary>
        public string sFamilyBlocMarck { get; set; } = sFamilyBlocMarckDef;

        /// <summary>
        /// Chaine définissant le nouveau nom de bloc.
        /// </summary>
        public string sFamillyStrResearchNewBlocName { get; set; } = sFamillyStrResearchNewBlocNameDef;

        /// <summary>
        /// Marque de repère des paramètres d'un bloc pour accès OPC UA.
        /// </summary>
        public string sCommentarBlocParameterMarck { get; set; } = sCommentarBlocParameterMarckDef;

        /// <summary>
        /// Marque de repère d'un tag pour accès OPC UA.
        /// </summary>
        public string sCommentarTagVariableMarck { get; set; } = sCommentarTagVariableMarckDef;

        /// <summary>
        /// Nom de l'utilisateur pour accès au projet TIA Portal.
        /// </summary>
        private string m_sUserName;
        public string GetUserName() { return m_sUserName; }
        public void SetUserName(string sUserName) { m_sUserName = sUserName; }

        /// <summary>
        /// Mot de passe utilisateur déchiffré (jamais stocké en clair).
        /// </summary>
        private string m_sUncryptPasswordUser;
        public string GetUncryptPasswordUser() { return m_sUncryptPasswordUser; }
        /// <summary>
        /// Déchiffre le mot de passe à partir du mot de passe chiffré.
        /// </summary>
        public void SetUncryptPasswordUser(string sCryptPasswordUser)
        {
            // Déchiffrement du mot de passe
            CryptString crypt = new CryptString(true);
            m_sUncryptPasswordUser = crypt.Decrypt(sCryptPasswordUser);
        }

        /// <summary>
        /// Nom du premier dossier pour la définition des blocs dans l'arborescence du serveur OPC UA.
        /// </summary>
        public string sRootFolderBlocOPCUAServer { get; set; } = sRootFolderBlocOPCUAServerDef;

        /// <summary>
        /// Nom du premier dossier pour la définition des tags dans l'arborescence du serveur OPC UA.
        /// </summary>
        public string sRootFolderTagsOPCUAServer { get; set; } = sRootFolderTagsOPCUAServerDef;

        /// <summary>
        /// Nom de la station S7-1500 H cible pour l'accès en OPC UA.
        /// </summary>
        public string sPLCS71500HTargetStationName { get; set; }

        /// <summary>
        /// Chemin complet de l'application.
        /// </summary>
        public string sPathApplication { get; set; }

        /// <summary>
        /// Nom du DB OPC UA de mapping.
        /// </summary>
        public string sDBNameMappingOPCUA { get; set; }

        /// <summary>
        /// Namespace pour le serveur OPC UA.
        /// </summary>
        public string sOPCUAServerNamespace { get; set; }

        /// <summary>
        /// Flag pour valider la génération des NodeId uniquement avec le nouveau nom dans la famille.
        /// </summary>
        public bool bNewNameBlockOnlyForTagNodeId { get; set; }

        #endregion

        /// <summary>
        /// Constructeur par défaut de la classe.
        /// </summary>
        public PLC_ProjectDefinitions()
        {
        }

    }
}