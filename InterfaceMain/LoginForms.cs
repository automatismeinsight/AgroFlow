using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InterfaceMain;

namespace InterfaceLoginTest
{
    public partial class LoginForms : Form
    {

        public string Login { get; private set; }
        public bool ConnectionSuccessful { get; private set; }

        public LoginForms()
        {
            InitializeComponent();
        }

        // Dictionary to store login credentials
        readonly IDictionary<string, string> BaseID = new Dictionary<string, string>();

        // Method to add default login credentials
        public void AddBaseID()
        {
            BaseID.Add("admin", "admin");
            BaseID.Add("user", "user");
            BaseID.Add("default", "");
        }

        // Event handler for form load event
        private void LoginForms_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            AddBaseID();
        }

        // Event handler for mouse click on login input field
        private void LoginInput_MouseClick(object sender, MouseEventArgs e)
        {
            if (LoginInput.Text == "Identifiant") LoginInput.Text = "";
        }

        // Event handler for mouse click on password input field
        private void PasswordInput_MouseClick(object sender, MouseEventArgs e)
        {
            if (PasswordInput.Text == "Mot de passe")
            {
                PasswordInput.Text = "";
            }
        }

        // Event handler for text change in password input field
        private void PasswordInput_TextChanged(object sender, EventArgs e)
        {
            PasswordInput.UseSystemPasswordChar = true;
            if (LoginInput.Text != "Identifiant" && PasswordInput.Text != "Mot de passe")
            {
                BPConnexion.Enabled = true;
            }
            else
            {
                BPConnexion.Enabled = false;
            }
        }

        // Event handler for key down event in password input field
        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BPConnexion_Click(sender, e);
            }
        }

        // Event handler for login button click
        private void BPConnexion_Click(object sender, EventArgs e)
        {
            if (PasswordInput.Text == BaseID[LoginInput.Text])
            {
                Login = LoginInput.Text;
                ConnectionSuccessful = true;

                this.Close();
            }
            else
            {
                MessageBox.Show("Identifiant ou mot de passe incorrect.");
            }
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
