using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AideAuDiagnostic.TiaExplorer
{
    public partial class TiaPortalProjectSelection : Form
    {
        public enum TiaProjectSelectionType : int { NoSelectionType = 0, CurrentTiaProject = 1, NewTiaProject = 2 }

        #region Variables
        // Objet dictionnaire de la liste des projets Tia Portal en cours de traitement
        public Dictionary<string, HMATiaPortalProcess> dTiaProcessList = new Dictionary<string, HMATiaPortalProcess>();

        // Permet d'indiquer si un projet a été sélectionné
        public bool bOneProjectSelected = false;

        // Type de sélection du projet
        public TiaProjectSelectionType iTiaProjectSelectType = TiaProjectSelectionType.NoSelectionType;

        // Chemin complet du nouveau Tia Portal
        public string sNewTiaPortalSelectionPath = string.Empty;
        #endregion

        public TiaPortalProjectSelection()
        {
            InitializeComponent();
        }

        private void TiaPortalProjectSelection_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            // Test si des instances sont en cours de traitement ?
            if (dTiaProcessList.Count != 0)
            {
                // Remplissage de la Combobox de sélection
                foreach (var tiaportalprocess in dTiaProcessList)
                {
                    cBCurrentProject.Items.Add(tiaportalprocess.Value);
                }
                // On sélectionne le premier choix
                cBCurrentProject.SelectedIndex = 0;
                cBCurrentProject.Enabled = true;
                bPValidate.Enabled = true;
            }
            else
            {
                bPValidate.Enabled = false;
            }
        }

        /// <summary>
        /// Permet de sélectionner le projet en cours de traitement sélectionné
        /// </summary>
        /// <returns></returns>
        public HMATiaPortalProcess GetSelectCurrentProject()
        {
            HMATiaPortalProcess hmatiaportalprocess = null;

            hmatiaportalprocess = (HMATiaPortalProcess)(cBCurrentProject.Items[cBCurrentProject.SelectedIndex]);

            //YOU PROJET EN COURS

            return hmatiaportalprocess;
        }

        private void bPValidate_Click(object sender, EventArgs e)
        {
            // On a validé un projet actuellement ouvert
            bOneProjectSelected = true;
            iTiaProjectSelectType = TiaProjectSelectionType.CurrentTiaProject;
            Close();
        }

        #region TitleBar
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        // Event handler for mouse down event on title label
        private void LbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // Event handler for mouse down event on icon
        private void AgromIcon_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        // Event handler for close button click
        private void CloseIconButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}
