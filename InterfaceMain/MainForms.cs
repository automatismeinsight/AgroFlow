using InterfaceLoginTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace InterfaceMain
{
    /// <summary>
    /// Represents the main Windows Form of the application.
    /// Manages UI components, events, and user/admin session states.
    /// </summary>
    public partial class MainForms : Form
    {
        /// <summary>
        /// Indicates whether an admin user is currently connected.
        /// </summary>
        public bool adminConnected = false;

        /// <summary>
        /// Provides access to the logging form for recording actions and events.
        /// </summary>
        public readonly ReturnLogForms returnLogForms = new ReturnLogForms();

        /// <summary>
        /// List of available functions for a standard user.
        /// </summary>
        List<string> userFunctions = new List<string> { "Aide au Diagnostic", "Reception de Projet", "Generation MAQ" };

        /// <summary>
        /// List of available functions for an admin user.
        /// </summary>
        List<string> adminFunctions = new List<string> { "AdminItem1", "AdminItem2" };

        /// <summary>
        /// Initializes a new instance of the MainForms class.
        /// </summary>
        public MainForms()
        {
            InitializeComponent();
            Updater();
            RemoveDll();
        }

        public void Updater()
        {
            WebClient webClient = new WebClient();
            var client = new WebClient();

            if (!webClient.DownloadString("https://raw.githubusercontent.com/SamBzd/AgroFlow/refs/heads/master/Update.txt").Contains("1.3.0"))
            {
                if(File.Exists(@".\Help.zip"))
                {
                    File.Delete(@".\Help.zip");
                }

                if(Directory.Exists(@".\Help"))
                {
                    Directory.Delete(@".\Help", true);
                }

                if (MessageBox.Show("Une nouvelle version est disponible. Voulez-vous la télécharger ?", "Mise à jour", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        if(File.Exists(@".\AgroFlowSetup.msi"))
                        {
                            File.Delete(@".\AgroFlowSetup.msi");
                        }
                        client.DownloadFile("https://github.com/SamBzd/AgroFlow/raw/refs/heads/master/AgroFlowSetup.zip", @"AgroFlowSetup.zip");
                        string zipPath = @".\AgroFlowSetup.zip";
                        string extractPath = @".\";
                        ZipFile.ExtractToDirectory(zipPath, extractPath);

                        Process process = new Process();
                        process.StartInfo.FileName = "msiexec";
                        process.StartInfo.Arguments = String.Format("/i AgroFlowSetup.msi");

                        this.Close();
                        process.Start();
                    }
                    catch
                    {
                        MessageBox.Show("Erreur lors du téléchargement de la mise à jour.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void RemoveDll()
        {
            var dll1 = @".\Siemens.Engineering.dll";
            var dll2 = @".\Siemens.Engineering.Hmi.dll";

            if (File.Exists(dll1))
            {
                File.Delete(dll1);
            }
            if (File.Exists(dll2))
            {
                File.Delete(dll2);
            }
        }

        /// <summary>
        /// Loads and applies the color scheme to UI elements.
        /// </summary>
        private void LoadColor()
        {
            Color AgroM_Green = Color.FromArgb(175, 190, 36);
            Color AgroM_Red = Color.FromArgb(65, 238, 49, 36);

            LbLogin.BackColor = AgroM_Red;
            LbTitle.BackColor = AgroM_Green;
            AgromIcon.BackColor = AgroM_Green;
            CloseIconButton.BackColor = AgroM_Green;
            MaximizeIconButton.BackColor = AgroM_Green;
            MinimizeIconButton.BackColor = AgroM_Green;
            CloseFullIconButton.BackColor = AgroM_Green;
            LeftBorder.BackColor = AgroM_Green;
            RightBorder.BackColor = AgroM_Green;
            BottomBorder.BackColor = AgroM_Green;
            LoginIconButton.BackColor = AgroM_Green;
            LogoutIconButton.BackColor = AgroM_Green;
            InfoPictureButton.BackColor = AgroM_Green;
        }

        /// <summary>
        /// Handles the form load event.
        /// Initializes references, loads TIA versions, and restores the previous session state.
        /// </summary>
        private void MainForms_Load(object sender, EventArgs e)
        {
            LoadColor();

            List<string> TIAversions = new List<string>();
            LoadReferences.InitializeApp(TIAversions);
            foreach (string version in TIAversions)
            {
                CbTIAVersion.Items.Add($"TIA V{version}");
                returnLogForms.UpdateLog($"TIA version V{version} founds");
            }
            UpdateCbFunctions();

            string lastVersion = null;
            try
            {
                lastVersion = Properties.Settings.Default.LastSelectedTIAVersion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading last selected TIA version: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(lastVersion))
            {
                foreach (var item in CbTIAVersion.Items)
                {
                    if (item.ToString().Contains(lastVersion))
                    {
                        CbTIAVersion.SelectedItem = item;
                        TIAVersionManager.IsFirstSelection = false;
                        break;
                    }
                }
                TIAVersionManager.SetVersion(lastVersion);
                returnLogForms.UpdateLog($"Version TIA chargée : TIA V{lastVersion}");
            }
        }

        /// <summary>
        /// Handles the form closed event.
        /// Exits the application.
        /// </summary>
        private void MainForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Handles the function selection change event.
        /// Loads and displays the selected function's user control.
        /// </summary>
        private void CbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            GbFunctions.Controls.Clear();
            UserControl selectedControl = null;
            string selectedFunction = LoadReferences.FormatString(CbFunction.Text);

            switch (selectedFunction)
            {
                case "AideAuDiagnostic":
                    selectedControl = new AideAuDiagnostic.AideAuDiagnostic();
                    break;
                case "ReceptionDeProjet":
                    selectedControl = new ReceptionDeProjet.ReceptionDeProjet();
                    break;
                case "GenerationMaq":
                    selectedControl = new GenerationMAQ.GenerationMAQ();
                    break;
                default:
                    GbFunctions.Text = "Fonction";
                    GbFunctions.Controls.Clear();
                    CbFunction.Text = "Function choice error";
                    MessageBox.Show("Invalid index selected.");
                    break;
            }

            if (selectedControl != null)
            {
                GbFunctions.Text = CbFunction.Text;
                selectedControl.Dock = DockStyle.Fill;
                GbFunctions.Controls.Add(selectedControl);
            }

            returnLogForms.UpdateLog($"Function TIA changed as {CbFunction.Text}");
        }

        /// <summary>
        /// Handles the TIA version selection change event.
        /// Updates the application state and restarts if a different version is selected.
        /// </summary>
        private void CbTIAVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            CbFunction.Enabled = true;

            string selectedVersion = LoadReferences.ExtractNumber(CbTIAVersion.SelectedItem.ToString());

            if (TIAVersionManager.IsFirstSelection)
            {
                TIAVersionManager.SetVersion(selectedVersion);
                TIAVersionManager.IsFirstSelection = false;

                Properties.Settings.Default.LastSelectedTIAVersion = selectedVersion;
                Properties.Settings.Default.Save();

                returnLogForms.UpdateLog($"-");
                returnLogForms.UpdateLog($"Version TIA définie à TIA V{selectedVersion}");
                return;
            }

            if (selectedVersion != TIAVersionManager.CurrentVersion)
            {
                Properties.Settings.Default.LastSelectedTIAVersion = selectedVersion;
                Properties.Settings.Default.Save();

                Application.Restart();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Handles the TIA version combo box click event.
        /// Resets the function group box and disables the function selection.
        /// </summary>
        private void CbTIAVersion_Click(object sender, EventArgs e)
        {
            GbFunctions.Text = "Fonction";
            GbFunctions.Controls.Clear();

            CbFunction.Text = "Sélectionner une fonction";
            CbFunction.Enabled = false;
        }

        #region MainMenu

        /// <summary>
        /// Handles the undevelop menu button click event.
        /// Shrinks the menu and adjusts the layout.
        /// </summary>
        private void UndeveloppIconButton_Click(object sender, EventArgs e)
        {
            ShowMenu();
            GbMenu.Height = 40;
            GbFunctions.Height = this.Height - 200 + 60;
            GbFunctions.Location = new Point(25, 115);
        }

        /// <summary>
        /// Handles the develop menu button click event.
        /// Expands the menu and adjusts the layout.
        /// </summary>
        private void DeveloppIconButton_Click(object sender, EventArgs e)
        {
            ShowMenu();
            GbMenu.Height = 100;
            GbFunctions.Height = this.Height - 200;
            GbFunctions.Location = new Point(25, 175);
        }

        /// <summary>
        /// Toggles the visibility of menu components and admin controls.
        /// </summary>
        private void ShowMenu()
        {
            CbFunction.Visible = !CbFunction.Visible;
            CbTIAVersion.Visible = !CbTIAVersion.Visible;
            UndeveloppIconButton.Visible = !UndeveloppIconButton.Visible;
            DeveloppIconButton.Visible = !DeveloppIconButton.Visible;

            if (adminConnected)
            {
                LbLogin.Visible = !LbLogin.Visible;
                TerminalIconButton.Visible = !TerminalIconButton.Visible;
            }
        }

        /// <summary>
        /// Handles the terminal icon button click event.
        /// Displays the logging form.
        /// </summary>
        private void IconTerminalButton_Click(object sender, EventArgs e)
        {
            returnLogForms.Show();
        }
        #endregion

        #region TitleBar

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        /// <summary>
        /// Handles the mouse down event on the title label for window dragging.
        /// </summary>
        private void LbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        /// <summary>
        /// Handles the mouse down event on the icon for window dragging.
        /// </summary>
        private void AgromIcon_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        /// <summary>
        /// Handles the close button click event.
        /// Closes the application immediately.
        /// </summary>
        private void CloseIconButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Handles the maximize button click event.
        /// Maximizes the window and updates UI elements.
        /// </summary>
        private void MaximizeIconButton_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
            }
            RescaleForms();
            MaximizeIconButton.Enabled = false;
            MaximizeIconButton.Visible = false;

            CloseFullIconButton.Enabled = true;
            CloseFullIconButton.Visible = true;
        }

        /// <summary>
        /// Handles the restore button click event.
        /// Restores the window to its normal state and updates UI elements.
        /// </summary>
        private void CloseFullIconButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            RescaleForms();
            CloseFullIconButton.Enabled = false;
            CloseFullIconButton.Visible = false;
            MaximizeIconButton.Enabled = true;
            MaximizeIconButton.Visible = true;
        }

        /// <summary>
        /// Handles the minimize button click event.
        /// Minimizes the window.
        /// </summary>
        private void MinimizeIconButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Rescales and repositions the form's controls according to the current window size.
        /// </summary>
        private void RescaleForms()
        {
            CloseIconButton.Location = new Point(this.Width - 50, 0);
            MaximizeIconButton.Location = new Point(this.Width - 100, 0);
            MinimizeIconButton.Location = new Point(this.Width - 150, 0);
            CloseFullIconButton.Location = new Point(this.Width - 100, 0);
            LogoutIconButton.Location = new Point(this.Width - 200, 0);
            LoginIconButton.Location = new Point(this.Width - 200, 0);

            GbFunctions.Width = this.Width - 50;
            GbFunctions.Height = this.Height - 200;

            LeftBorder.Height = this.Height;
            RightBorder.Height = this.Height;
            RightBorder.Location = new Point(this.Width - 5, 0);
            BottomBorder.Width = this.Width;
            BottomBorder.Location = new Point(0, this.Height - 10);
        }

        /// <summary>
        /// Handles the login icon button click event.
        /// Opens the login form and updates the UI based on authentication result.
        /// </summary>
        private void LoginIconButton_Click(object sender, EventArgs e)
        {
            using (LoginForms loginForms = new LoginForms())
            {
                loginForms.ShowDialog();
                if (loginForms.ConnectionSuccessful)
                {
                    string login = loginForms.Login;
                    returnLogForms.UpdateLog($"-");
                    returnLogForms.UpdateLog($"Connected as {login}");
                    if (login == "admin")
                    {
                        AdminConnected();
                    }

                    LoginIconButton.Visible = false;
                    LoginIconButton.Enabled = false;

                    LogoutIconButton.Visible = true;
                    LogoutIconButton.Enabled = true;
                }
                else
                {
                    returnLogForms.UpdateLog($"-");
                    returnLogForms.UpdateLog("Connection failed");
                }
            }
        }

        /// <summary>
        /// Updates the UI and available functions for an admin connection.
        /// </summary>
        private void AdminConnected()
        {
            adminConnected = true;
            UpdateCbFunctions();
            LbLogin.Text = $"Connecté en tant qu'admin";
            LbLogin.Visible = true;

            TerminalIconButton.Visible = true;
            TerminalIconButton.Enabled = true;
        }

        /// <summary>
        /// Handles the logout icon button click event.
        /// Logs out the user and resets UI states.
        /// </summary>
        private void LogoutIconButton_Click(object sender, EventArgs e)
        {
            LogoutIconButton.Visible = false;
            LogoutIconButton.Enabled = false;

            LoginIconButton.Visible = true;
            LoginIconButton.Enabled = true;

            LbLogin.Visible = false;

            TerminalIconButton.Visible = false;
            TerminalIconButton.Enabled = false;

            adminConnected = false;

            returnLogForms.UpdateLog("Disconnected");

            UpdateCbFunctions();
        }

        /// <summary>
        /// Updates the items in the function combo box based on the user/admin status.
        /// </summary>
        private void UpdateCbFunctions()
        {
            CbFunction.Items.Clear();

            foreach (var item in userFunctions)
            {
                CbFunction.Items.Add(item);
            }

            if (adminConnected)
            {
                foreach (var item in adminFunctions)
                {
                    CbFunction.Items.Add(item);
                }
            }
        }




        private void InfoPictureButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@".\Help"))
            {
                var client = new WebClient();

                if (MessageBox.Show("Voulez-vous télécharger la documentation?", "Documentatio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        
                        client.DownloadFile("https://raw.githubusercontent.com/SamBzd/AgroFlow/6d4dec02a5ad27bb83d288bf76f1c28ae34ffbd4/Help.zip", @"Help.zip");
                        string zipPath = @".\Help.zip";
                        string extractPath = @".\";
                        ZipFile.ExtractToDirectory(zipPath, extractPath);
                    }
                    catch
                    {
                        MessageBox.Show("Erreur lors du téléchargement de la documentation.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            string path = @".\Help\index.html";
            string fullPath = Path.GetFullPath(path);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'ouvrir le fichier d'aide : " + ex.Message);
            }
        }
        #endregion
    }
}