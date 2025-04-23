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
    /// Form for selecting a TIA Portal project from available running processes or by specifying a new project path.
    /// </summary>
    public partial class TiaPortalProjectSelection : Form
    {
        /// <summary>
        /// Enum representing the type of TIA project selection.
        /// </summary>
        public enum TiaProjectSelectionType : int
        {
            NoSelectionType = 0,
            CurrentTiaProject = 1,
            NewTiaProject = 2
        }

        #region Variables

        /// <summary>
        /// Dictionary containing running TIA Portal project processes.
        /// </summary>
        public Dictionary<string, HMATiaPortalProcess> dTiaProcessList = new Dictionary<string, HMATiaPortalProcess>();

        /// <summary>
        /// Indicates whether a project has been selected.
        /// </summary>
        public bool bOneProjectSelected = false;

        /// <summary>
        /// Indicates the type of project selection made.
        /// </summary>
        public TiaProjectSelectionType iTiaProjectSelectType = TiaProjectSelectionType.NoSelectionType;

        /// <summary>
        /// Full path to the newly selected TIA Portal project.
        /// </summary>
        public string sNewTiaPortalSelectionPath = string.Empty;

        #endregion

        /// <summary>
        /// Initializes a new instance of the TiaPortalProjectSelection form.
        /// </summary>
        public TiaPortalProjectSelection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles form load event. Initializes UI and populates the project selection list.
        /// </summary>
        private void TiaPortalProjectSelection_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            // Check if any TIA Portal project instances are currently running
            if (dTiaProcessList.Count != 0)
            {
                // Populate the ComboBox with running projects
                foreach (var tiaportalprocess in dTiaProcessList)
                {
                    cBCurrentProject.Items.Add(tiaportalprocess.Value);
                }
                // Select the first project by default
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
        /// Gets the currently selected running TIA Portal project process.
        /// </summary>
        /// <returns>The selected HMATiaPortalProcess object, or null if none is selected.</returns>
        public HMATiaPortalProcess GetSelectCurrentProject()
        {
            HMATiaPortalProcess hmatiaportalprocess = null;

            hmatiaportalprocess = (HMATiaPortalProcess)(cBCurrentProject.Items[cBCurrentProject.SelectedIndex]);

            return hmatiaportalprocess;
        }

        /// <summary>
        /// Handles the validation button click event. Confirms the current project selection.
        /// </summary>
        private void bPValidate_Click(object sender, EventArgs e)
        {
            bOneProjectSelected = true;
            iTiaProjectSelectType = TiaProjectSelectionType.CurrentTiaProject;
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