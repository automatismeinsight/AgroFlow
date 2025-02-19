using InterfaceLoginTest;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace InterfaceMain
{
    public partial class MainForms : Form
    {
        public bool adminConnected = false;
        public readonly ReturnLogForms returnLogForms = new ReturnLogForms(); // Create an instance of the ReturnLogForms class
        
        List<string> userFunctions = new List<string> { "Aide au Diagnostic", "Reception de Projet" }; // List of functions for the user
        List<string> adminFunctions = new List<string> { "AdminItem1", "AdminItem2" }; // List of functions for the admin

        public MainForms()
        {
            InitializeComponent();
        }

        // Method to load and set the colors for various UI elements
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
        }

        // Event handler for form load event

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
        }

        // Event handler for form closed event
        private void MainForms_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        // ...

        private void CbFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            GbFunctions.Controls.Clear();
            System.Windows.Forms.UserControl selectedControl = null;
            string selectedFunction = LoadReferences.FormatString(CbFunction.Text);

            switch (selectedFunction)
            {
                case "AideAuDiagnostic":
                    selectedControl = new AideAuDiagnostic.AideAuDiagnostic();
                    break;
                case "ReceptionDeProjet":
                    selectedControl = new ReceptionDeProjet.ReceptionDeProjet();
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

            // Write the selected function to the log
            returnLogForms.UpdateLog($"Function TIA changed as {CbFunction.Text}");
        }

        // Event handler for TIA version selection change
        private void CbTIAVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            CbFunction.Enabled = true;

            string selectedVersion = LoadReferences.ExtractNumber(CbTIAVersion.SelectedItem.ToString());

            // Load the appropriate DLL files based on the selected version
            // exemple path :  C:\Program Files\Siemens\Automation\Portal V16\PublicAPI\Siemens.Engineering.dll


            string referencePath = $"C:\\Program Files\\Siemens\\Automation\\Portal V{selectedVersion}\\PublicAPI\\V{selectedVersion}\\";


            returnLogForms.UpdateLog($"-");
            returnLogForms.UpdateLog($"Try to change TIA version as TIA V{selectedVersion}");
            // Dynamically load the assemblies (DLLs)
            if (LoadReferences.LoadReference(referencePath) == 0) returnLogForms.UpdateLog("References loaded successfully.");
            else returnLogForms.UpdateLog("Error loading references.");
        }

        // Event handler for TIA version click event
        private void CbTIAVersion_Click(object sender, EventArgs e)
        {
            // Clear the function group box
            GbFunctions.Text = "Fonction";
            GbFunctions.Controls.Clear();

            // Disable the function selection combo box
            CbFunction.Text = "Sélectionner une fonction";
            CbFunction.Enabled = false;
        }

        #region MainMenu
        // Event handler for undevelop button click
        private void UndeveloppIconButton_Click(object sender, EventArgs e)
        {
            ShowMenu();
            GbMenu.Height = 40;
            GbFunctions.Height = this.Height - 200 + 60;
            GbFunctions.Location = new Point(25, 115);
        }

        // Event handler for develop button click
        private void DeveloppIconButton_Click(object sender, EventArgs e)
        {
            ShowMenu();
            GbMenu.Height = 100;
            GbFunctions.Height = this.Height - 200;
            GbFunctions.Location = new Point(25, 175);
        }

        // Method to show or hide the menu
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

        // Event handler for terminal button click
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

        // Event handler for maximize button click
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

        // Event handler for restore button click
        private void CloseFullIconButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            RescaleForms();
            CloseFullIconButton.Enabled = false;
            CloseFullIconButton.Visible = false;
            MaximizeIconButton.Enabled = true;
            MaximizeIconButton.Visible = true;
        }

        // Event handler for minimize button click
        private void MinimizeIconButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // Method to rescale and reposition form elements
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

        private void AdminConnected()
        {
            adminConnected = true;
            UpdateCbFunctions();
            LbLogin.Text = $"Connecté en tant qu'admin";
            LbLogin.Visible = true;

            TerminalIconButton.Visible = true;
            TerminalIconButton.Enabled = true;
        }

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

        private void UpdateCbFunctions()
        {
            CbFunction.Items.Clear();

            // Add the user functions to the combo box
            foreach (var item in userFunctions)
            {
                CbFunction.Items.Add(item);
            }

            // Add the admin functions to the combo box
            if (adminConnected)
            {
                foreach (var item in adminFunctions)
                {
                    CbFunction.Items.Add(item);
                }
            }
        }
        // TEST POUR GITHUB 
        #endregion
    }
}