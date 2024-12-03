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
    public partial class TiaPortalStationSelection : Form
    {
        // Objet dictionnaire de la liste des stations Tia Portal dans le projet en cours de traitement
        public Dictionary<string, HMATiaPortalDevice> dDictionnaryTiaStationList = new Dictionary<string, HMATiaPortalDevice>();

        // Permet d'indiquer si un projet a été sélectionné
        public bool bOneStationSelected = false;

        public TiaPortalStationSelection()
        {
            InitializeComponent();
        }

        private void TiaProjectStationSelection_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            // Chargement de la liste des stations
            // Test si des instances sont en cours de traitement ?
            if (dDictionnaryTiaStationList.Count != 0)
            {
                // Remplissage de la Combobox de sélection
                foreach (var device in dDictionnaryTiaStationList)
                {
                    cBCurrentStation.Items.Add(device.Value);
                }
                // On sélectionne le premier choix
                cBCurrentStation.SelectedIndex = 0;
                cBCurrentStation.Enabled = true;
                bPValidate.Enabled = true;
            }
            else
            {
                bPValidate.Enabled = false;
            }
        }
        public HMATiaPortalDevice GetSelectCurrentStation()
        {
            HMATiaPortalDevice oDevice;

            oDevice = (HMATiaPortalDevice)(cBCurrentStation.Items[cBCurrentStation.SelectedIndex]);

            return oDevice;
        }

        private void bPValidate_Click(object sender, EventArgs e)
        {
            //Validation Station Sélectionné
            bOneStationSelected = true;
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
