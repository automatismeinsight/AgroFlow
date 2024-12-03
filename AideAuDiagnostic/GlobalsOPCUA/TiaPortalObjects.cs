using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalsOPCUA
{
    public class TiaPortalVariable
    {
        //Nom de la variable
        public string sVariableName { get; set; }
        //Type de la variable
        public string sVariableType { get; set; }
        //Type de la variable bute
        public string sRawType { get; set; }
        //Bloc parent si il existe
        public TiaPortalBloc oParentBloc { get; set; }
        //Id de la variable
        public int iVariableId { get; set; }
        //Noeud folder parent
        public TreeNode oParentNode { get; set; }
        //Definition du commentaire sur la variable (Description dans le fichier OPC UA)
        public string sComment { get; set; }
        //Definition de l'adresse de variable de mapping
        public string sMappingAddress { get; set; }
        //Definition de la variable ReadOnly
        public bool bIsReadOnly { get; set; }
        //Definition du chemin de variable courant
        public string sCurrentPath { get; set; }


        //Constructeur de la classe
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
        //Constructeur de la classe évolué
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
    public class TiaPortalBloc
    {
        //Nom du bloc
        public string sBlocName { get; set; }
        //Liste des variables du bloc
        public List<TiaPortalVariable> loListVariables { get; set; }
        //Node folder reference
        public TreeNode oParentNode { get; set; }
        //Id du folder
        public int iFolderId { get; set; }
        //Nouveau nom dans le cas d'une régénération
        public string sNewBlocName { get; set; }

        //Constructeur de la classe
        public TiaPortalBloc(string sBlocName, TreeNode oParentNode, int iFolderId, string sNewBlocName)
        {
            this.sBlocName = sBlocName;
            this.oParentNode = oParentNode;
            this.loListVariables = new List<TiaPortalVariable>();
            this.iFolderId = iFolderId;
            this.sNewBlocName = sNewBlocName;
        }
    }
    public class TiaPortalFolder
    {
        //Nom du Folder
        public string sFolderName { get; set; }
        //Chemin complet du Folder
        public int iFolderId { get; set; }

        //Constructeur de la classe
        public TiaPortalFolder(string sFolderName, int iFolderId)
        {
            this.sFolderName = sFolderName;
            this.iFolderId = iFolderId;
        }
    }
    public class TiaProjectForCPU
    {
        #region CONSTANTES
        // Valeur du premier identifiant pour les folders
        private const int iStartFolderId = 1200;
        #endregion
        #region VARIABLES
        // Dernier identifiant disponible pour affectation dans les folders et variables
        private int iLastFolderVariableId;
        //Nom du projet TIA Portal cible pour l'automate passerelle
        public string sProjectName { get; set; }
        // Nom de la station cible pour l'automate passerelle
        public string sStationName { get; set; }
        // Nom de la racine pour les blocs
        public string sRootBlocsName { get; set; }
        // Nom de la racine pour les tags
        public string sRootTagsName { get; set; }
        // Nom du DB pour le mapping OPC UA
        public string sDBNameMappingOPCUA { get; set; }
        // Noed arborescence pour les folders et les blocs
        private TreeNode oRootNodefolderBlocks;
        public TreeNode GetRootNodefolderBlocks()
        {
            return oRootNodefolderBlocks;
        }
        // Noed arborescence pour les folders et les tags
        private TreeNode oRootNodefolderTags;
        public TreeNode GetRootNodefolderTags()
        {
            return oRootNodefolderTags;
        }
        // Noed arborescence pour les folders et les variables systeme
        private TreeNode oRootNodefolderSystemVariables;
        public TreeNode GetRootNodefolderSystemVariables()
        {
            return oRootNodefolderSystemVariables;
        }
        // Repertoire par defaut pour le système
        private string sRootVariableSystemeName = @"System";
        // Liste des blocs
        public List<TiaPortalBloc> loListBlocs { get; set; } = new List<TiaPortalBloc>();
        // Liste des variables sous le folder tags
        public List<TiaPortalVariable> loListVariablesTags { get; set; } = new List<TiaPortalVariable>();
        // Liste des variables sous le folder des variables systeme
        public List<TiaPortalVariable> loListVariablesSystem { get; set; } = new List<TiaPortalVariable>();
        //Nom du controlleur automate dans le projet le source
        public string sControllerPLCName { get; set; }
        // Répertoire par défaut pour le system H
        public string sRootSystemName = @"System";
        // Répertoire par défaut pour les variables system H
        public string sRootVariableSystemName = @"Variables_PLC_H";
        #endregion

        // Constructeur de la classe par défaut
        public TiaProjectForCPU()
        {
            iLastFolderVariableId = iStartFolderId;
        }

        // Constructeur de la classe
        public TiaProjectForCPU(string sProjectName, string sStationName, string sRootBlocsName, string sRootTagsName, string sDBNameMappingOPCUA) : this()
        {
            this.sProjectName = sProjectName;
            this.sStationName = sStationName;
            this.sRootBlocsName = sRootBlocsName;
            this.sRootTagsName = sRootTagsName;
            this.sDBNameMappingOPCUA = sDBNameMappingOPCUA;

            // Ajout d'un folder root par defaut s'il l'on en défini un
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

        // Calcul et retournl'identifiant du prochain folder ou variable
        public int GetNextFolderVariableId()
        {
            return iLastFolderVariableId++;
        }
    }
}
