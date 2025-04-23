using System.Collections.Generic;
using System.Windows.Forms;

namespace GlobalsOPCUA
{
    /// <summary>
    /// Représente une variable du TIA Portal, avec ses propriétés, parent et informations de mapping OPC UA.
    /// </summary>
    public class TiaPortalVariable
    {
        /// <summary>Nom de la variable.</summary>
        public string sVariableName { get; set; }
        /// <summary>Type de la variable.</summary>
        public string sVariableType { get; set; }
        /// <summary>Type brut de la variable.</summary>
        public string sRawType { get; set; }
        /// <summary>Bloc parent si il existe.</summary>
        public TiaPortalBloc oParentBloc { get; set; }
        /// <summary>Identifiant de la variable.</summary>
        public int iVariableId { get; set; }
        /// <summary>Noeud parent dans l'arborescence.</summary>
        public TreeNode oParentNode { get; set; }
        /// <summary>Description/commentaire OPC UA.</summary>
        public string sComment { get; set; }
        /// <summary>Adresse de mapping OPC UA.</summary>
        public string sMappingAddress { get; set; }
        /// <summary>Indique si la variable est en lecture seule.</summary>
        public bool bIsReadOnly { get; set; }
        /// <summary>Chemin courant de la variable dans l'arborescence.</summary>
        public string sCurrentPath { get; set; }

        /// <summary>
        /// Constructeur de base de la classe <see cref="TiaPortalVariable"/>.
        /// </summary>
        public TiaPortalVariable(string sVariableName, string sVariableType, string sRawType,
                                 TiaPortalBloc oParentBloc, int iVariableId, TreeNode oParentNode,
                                 string sComment, string sCurrentPath)
        {
            this.sVariableName = sVariableName;
            this.sVariableType = sVariableType;
            this.sRawType = sRawType;
            this.oParentBloc = oParentBloc;
            this.iVariableId = iVariableId;
            this.oParentNode = oParentNode;
            this.sComment = sComment;
            this.sMappingAddress = string.Empty;
            this.bIsReadOnly = false;
            this.sCurrentPath = sCurrentPath;
        }

        /// <summary>
        /// Constructeur évolué de la classe <see cref="TiaPortalVariable"/>.
        /// </summary>
        public TiaPortalVariable(string sName, TiaPortalBloc oBlocParent, string sType, string sRawType, int iId, TreeNode oNodeParent, string sCommentar,
                                 string sMappingVariable, bool bReadOnly, string sCurrentPathVariable)
        {
            this.sVariableName = sName;
            this.sVariableType = sType;
            this.oParentBloc = oBlocParent;
            this.sRawType = sRawType;
            this.iVariableId = iId;
            this.oParentNode = oNodeParent;
            this.sComment = sCommentar;
            this.sMappingAddress = sMappingVariable;
            this.bIsReadOnly = bReadOnly;
            this.sCurrentPath = sCurrentPathVariable;
        }
    }

    /// <summary>
    /// Représente un bloc de programme TIA Portal, avec ses variables et propriétés associées.
    /// </summary>
    public class TiaPortalBloc
    {
        /// <summary>Nom du bloc.</summary>
        public string sBlocName { get; set; }
        /// <summary>Liste des variables du bloc.</summary>
        public List<TiaPortalVariable> loListVariables { get; set; }
        /// <summary>Noeud parent dans l'arborescence.</summary>
        public TreeNode oParentNode { get; set; }
        /// <summary>Identifiant du dossier parent.</summary>
        public int iFolderId { get; set; }
        /// <summary>Nouveau nom dans le cas d'une régénération.</summary>
        public string sNewBlocName { get; set; }

        /// <summary>
        /// Constructeur de la classe <see cref="TiaPortalBloc"/>.
        /// </summary>
        public TiaPortalBloc(string sBlocName, TreeNode oParentNode, int iFolderId, string sNewBlocName)
        {
            this.sBlocName = sBlocName;
            this.oParentNode = oParentNode;
            this.loListVariables = new List<TiaPortalVariable>();
            this.iFolderId = iFolderId;
            this.sNewBlocName = sNewBlocName;
        }
    }

    /// <summary>
    /// Représente un dossier (folder) dans l'arborescence TIA Portal.
    /// </summary>
    public class TiaPortalFolder
    {
        /// <summary>Nom du dossier.</summary>
        public string sFolderName { get; set; }
        /// <summary>Identifiant du dossier.</summary>
        public int iFolderId { get; set; }

        /// <summary>
        /// Constructeur de la classe <see cref="TiaPortalFolder"/>.
        /// </summary>
        public TiaPortalFolder(string sFolderName, int iFolderId)
        {
            this.sFolderName = sFolderName;
            this.iFolderId = iFolderId;
        }
    }

    /// <summary>
    /// Modèle de projet TIA Portal pour l'automate cible, avec gestion de l'arborescence blocs/tags/variables système.
    /// </summary>
    public class TiaProjectForCPU
    {
        #region CONSTANTES
        private const int iStartFolderId = 1200;
        #endregion

        #region VARIABLES
        private int iLastFolderVariableId;
        /// <summary>Nom du projet TIA Portal cible pour l'automate passerelle.</summary>
        public string sProjectName { get; set; }
        /// <summary>Nom de la station cible pour l'automate passerelle.</summary>
        public string sStationName { get; set; }
        /// <summary>Nom de la racine pour les blocs.</summary>
        public string sRootBlocsName { get; set; }
        /// <summary>Nom de la racine pour les tags.</summary>
        public string sRootTagsName { get; set; }
        /// <summary>Nom du DB pour le mapping OPC UA.</summary>
        public string sDBNameMappingOPCUA { get; set; }
        private TreeNode oRootNodefolderBlocks;
        /// <summary>Récupère le noeud racine des blocs.</summary>
        public TreeNode GetRootNodefolderBlocks() => oRootNodefolderBlocks;
        private TreeNode oRootNodefolderTags;
        /// <summary>Récupère le noeud racine des tags.</summary>
        public TreeNode GetRootNodefolderTags() => oRootNodefolderTags;
        private TreeNode oRootNodefolderSystemVariables;
        /// <summary>Récupère le noeud racine des variables système.</summary>
        public TreeNode GetRootNodefolderSystemVariables() => oRootNodefolderSystemVariables;
        private string sRootVariableSystemeName = @"System";
        /// <summary>Liste des blocs du projet.</summary>
        public List<TiaPortalBloc> loListBlocs { get; set; } = new List<TiaPortalBloc>();
        /// <summary>Liste des variables sous le folder tags.</summary>
        public List<TiaPortalVariable> loListVariablesTags { get; set; } = new List<TiaPortalVariable>();
        /// <summary>Liste des variables système.</summary>
        public List<TiaPortalVariable> loListVariablesSystem { get; set; } = new List<TiaPortalVariable>();
        /// <summary>Nom du contrôleur automate dans le projet source.</summary>
        public string sControllerPLCName { get; set; }
        /// <summary>Répertoire par défaut pour le système H.</summary>
        public string sRootSystemName = @"System";
        /// <summary>Répertoire par défaut pour les variables système H.</summary>
        public string sRootVariableSystemName = @"Variables_PLC_H";
        #endregion

        /// <summary>
        /// Constructeur par défaut de la classe <see cref="TiaProjectForCPU"/>.
        /// </summary>
        public TiaProjectForCPU()
        {
            iLastFolderVariableId = iStartFolderId;
        }

        /// <summary>
        /// Constructeur avec initialisation de toutes les propriétés principales.
        /// </summary>
        public TiaProjectForCPU(string sProjectName, string sStationName, string sRootBlocsName, string sRootTagsName, string sDBNameMappingOPCUA)
            : this()
        {
            this.sProjectName = sProjectName;
            this.sStationName = sStationName;
            this.sRootBlocsName = sRootBlocsName;
            this.sRootTagsName = sRootTagsName;
            this.sDBNameMappingOPCUA = sDBNameMappingOPCUA;

            // Ajoute un folder root par défaut si défini
            if (this.sRootBlocsName.Length != 0)
            {
                oRootNodefolderBlocks = new TreeNode(this.sRootBlocsName);
                oRootNodefolderBlocks.Tag = new TiaPortalFolder(this.sRootBlocsName, GetNextFolderVariableId());
            }
            if (this.sRootTagsName.Length != 0)
            {
                oRootNodefolderTags = new TreeNode(sRootTagsName);
                oRootNodefolderTags.Tag = new TiaPortalFolder(this.sRootTagsName, GetNextFolderVariableId());
            }
            oRootNodefolderSystemVariables = new TreeNode(sRootVariableSystemeName);
            oRootNodefolderSystemVariables.Tag = new TiaPortalFolder(sRootVariableSystemeName, GetNextFolderVariableId());
        }

        /// <summary>
        /// Retourne et incrémente l'identifiant du prochain folder ou variable.
        /// </summary>
        public int GetNextFolderVariableId()
        {
            return iLastFolderVariableId++;
        }
    }
}