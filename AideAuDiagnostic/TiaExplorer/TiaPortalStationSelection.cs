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
    /// <summary>
    /// Form for selecting a TIA Portal station from the available stations in the current project.
    /// </summary>
    public partial class TiaPortalStationSelection : Form
    {
        /// <summary>
        /// Dictionary containing the list of TIA Portal stations in the current project.
        /// </summary>
        public Dictionary<string, HMATiaPortalDevice> dDictionnaryTiaStationList = new Dictionary<string, HMATiaPortalDevice>();

        /// <summary>
        /// Indicates whether a station has been selected.
        /// </summary>
        public bool bOneStationSelected = false;

        /// <summary>
        /// Initializes a new instance of the TiaPortalStationSelection form.
        /// </summary>
        public TiaPortalStationSelection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the form load event. Initializes UI and populates the station selection list.
        /// </summary>
        private void TiaProjectStationSelection_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            // Load and populate the list of available stations if any exist
            if (dDictionnaryTiaStationList.Count != 0)
            {
                foreach (var device in dDictionnaryTiaStationList)
                {
                    cBCurrentStation.Items.Add(device.Value);
                }
                cBCurrentStation.SelectedIndex = 0;
                cBCurrentStation.Enabled = true;
                bPValidate.Enabled = true;
            }
            else
            {
                bPValidate.Enabled = false;
            }
        }

        /// <summary>
        /// Gets the currently selected TIA Portal station device.
        /// </summary>
        /// <returns>The selected HMATiaPortalDevice object, or null if none is selected.</returns>
        public HMATiaPortalDevice GetSelectCurrentStation()
        {
            HMATiaPortalDevice oDevice;

            oDevice = (HMATiaPortalDevice)(cBCurrentStation.Items[cBCurrentStation.SelectedIndex]);

            return oDevice;
        }

        /// <summary>
        /// Handles the validation button click event. Confirms the current station selection.
        /// </summary>
        private void bPValidate_Click(object sender, EventArgs e)
        {
            bOneStationSelected = true;
            Close();
        }

        #region TitleBar

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        /// <summary>
        /// Handles mouse down event on the title label for window dragging.
        /// </summary>
        private void LbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        /// <summary>
        /// Handles mouse down event on the icon for window dragging.
        /// </summary>
        private void AgromIcon_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        /// <summary>
        /// Handles the close button click event. Exits the application.
        /// </summary>
        private void CloseIconButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}