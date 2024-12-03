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

namespace InterfaceMain
{
    public partial class ReturnLogForms : Form
    {
        public ReturnLogForms()
        {
            InitializeComponent();
        }


        private void ReturnLogForms_Load(object sender, EventArgs e)
        {
            Color AgroM_Green = Color.FromArgb(175, 190, 36);
            LbTitle.BackColor = AgroM_Green;
            AgromIcon.BackColor = AgroM_Green;
            CloseIconButton.BackColor = AgroM_Green;
            LeftBorder.BackColor = AgroM_Green;
            RightBorder.BackColor = AgroM_Green;
            BottomBorder.BackColor = AgroM_Green;
        }

        #region TitleBar

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void LbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void AgromIcon_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void CloseIconButton_Click(object sender, EventArgs e)
        {
            ReturnLogForms.ActiveForm.Hide();  
        }
        #endregion

        public void UpdateLog(string message)
        {
            var now = DateTime.Now; // Actual date and time
            string currentDateTime = $"{now.Year}/{now.Month.ToString("D2")}/{now.Day.ToString("D2")} {now.Hour.ToString("D2")}:{now.Minute.ToString("D2")}:{now.Second.ToString("D2")} - ";
            string fullMessage;
            if (message == "-")
            {
                message = "---------------------------------------------------------------------------------------------------------------------";
                fullMessage = $"{message}\n";
            }
            else
            {
                fullMessage = $"{currentDateTime} {message}\n";
            }
            // Ajoute le message au RichTextBox
            TxtBLogs.AppendText(fullMessage);

            // Défile automatiquement vers le bas
            TxtBLogs.SelectionStart = TxtBLogs.Text.Length;
            TxtBLogs.ScrollToCaret();
        }
    }
}
