using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreationDeProjet.GenerateObject
{
    public partial class AddStation : Form
    {
        //0 - Station Name
        //1 - Element Name
        //2 - Device Type
        //3 - @IP
        //4 - Mask
        private List<string> lsStation = Enumerable.Repeat("", 5).ToList();
        private List<List<string>> lsStationList  = new List<List<string>>();
        private int iStationIndex = 0;
        private List<string> lsDeviceType = new List<string> {"Type 1", "Type2", "Type3" };
        public AddStation()
        {
            InitializeComponent();
        }

        private void AddStation_Load(object sender, EventArgs e)
        {
            LbTitle.BackColor = Color.FromArgb(175, 190, 36);
            AgromIcon.BackColor = Color.FromArgb(175, 190, 36);
            CloseIconButton.BackColor = Color.FromArgb(175, 190, 36);
            LeftBorder.BackColor = Color.FromArgb(175, 190, 36);
            RightBorder.BackColor = Color.FromArgb(175, 190, 36);
            BottomBorder.BackColor = Color.FromArgb(175, 190, 36);

            foreach (var item in lsDeviceType)
            {
                CbDeviceType.Items.Add(item);
            }

            ClearInput();
        }

        private void CloseIconButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NewDevice()
        {
            ClearError();
            if (!setInput())
            {
                
                iStationIndex++;
                lsStationList.Add(lsStation);
                PrintStation();
                for (int i = 0; i < 5; i++)
                {
                    lsStation.Add("");
                }
                ClearInput();
            }
        }

        private void ClearError()
        {
            LbErrorElement.Visible = false;
            LbErrorName.Visible =false;
            LbErrorType.Visible =false;
            LbErrorIP.Visible =false;   
            LbErrorMask.Visible =false; 
        }

        private bool setInput()
        {
            string sIP = TxtIP1.Text + "." + TxtIP2.Text + "." + TxtIP3.Text + "." + TxtIP4.Text;
            string sMask = TxtMask1.Text + "." + TxtMask2.Text + "." + TxtMask3.Text + "." + TxtMask4.Text;
            bool bRet = false;

            if (TxtNameStation.Text.Contains(" "))
            {
                LbErrorName.Text = "Le nom ne peux pas contenir d'espace";
                LbErrorName.Visible = true;
                bRet = true;
            }
            else if (string.IsNullOrWhiteSpace(TxtNameStation.Text))
            {
                LbErrorName.Text = "Le nom ne peux pas être vide";
                LbErrorName.Visible = true;
                bRet = true;
            }

            if (TxtElementName.Text.Contains(" "))
            {
                LbErrorElement.Text = "Le nom ne peux pas contenir d'espace";
                LbErrorElement.Visible = true;
                bRet = true;
            }
            else if (string.IsNullOrWhiteSpace(TxtElementName.Text))
            {
                LbErrorElement.Text = "Le nom ne peux pas être vide";
                LbErrorElement.Visible = true;
                bRet = true;
            }

            foreach(string s in sIP.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    LbErrorIP.Text = "Un champ de l'adresse IP ne peut pas être vide";
                    LbErrorIP.Visible = true;
                    bRet = true;
                }
            }

            foreach (string s in sMask.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    LbErrorMask.Text = "Un champ du Mask ne peut pas être vide";
                    LbErrorMask.Visible = true;
                    bRet = true;
                }
            }

            lsStation[0] = TxtNameStation.Text;
            lsStation[1] = TxtElementName.Text;
            lsStation[2] = CbDeviceType.Text;
            lsStation[3] = sIP;
            lsStation[4] = sMask;
            

            return bRet;
        }
        
        private void ClearInput()
        {
            TxtNameStation.Clear();
            TxtElementName.Clear();
            CbDeviceType.Text = "Sélectionner un type";
            TxtIP1.Clear();
            TxtIP2.Clear();
            TxtIP3.Clear();
            TxtIP4.Clear();
            TxtMask1.Clear();
            TxtMask2.Clear();
            TxtMask3.Clear();
            TxtMask4.Clear();
        }

        private void PrintStation()
        {
            RtxtStationList.Clear();
            RtxtStationList.Text = "Nombre de device  = " + iStationIndex + "\n";
            RtxtStationList.Text = RtxtStationList.Text + "";
            RtxtStationList.Text = RtxtStationList.Text + "──────────────────────────────────────────────────────" + "\n";
            RtxtStationList.Text = RtxtStationList.Text + "" + "\n";
            foreach (List<string> lsStation in lsStationList)
            {
                RtxtStationList.Text = RtxtStationList.Text + "┌───────────┐" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    ┌─────┐    │ Nom : " + lsStation[0] + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    │  0  0  │    │" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    └─────┘    │ Nom Element : " + lsStation[1] + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│        <  >        │" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    ┌─────┐    │ Type : " + lsStation[2] + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    │  0  0  │    │" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "│    └─────┘    │ @IP : " + lsStation[3] + ConvertToCIDR(lsStation[4]) + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "└───────────┘" + "\n";

                RtxtStationList.Text = RtxtStationList.Text + "" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "──────────────────────────────────────────────────────" + "\n";
                RtxtStationList.Text = RtxtStationList.Text + "" + "\n";
            }
        }

        private string ConvertToCIDR(string mask)
        {
            return "/" + mask
                .Split('.')                         
                .Select(byte.Parse)                 
                .Sum(b => CountBits(b));           
        }

        private int CountBits(int number)
        {
            int count = 0;
            while (number > 0)
            {
                count += number & 1; 
                number >>= 1;        
            }
            return count;
        }

        #region IP/Mask
        //IP Configuration
        private void TxtIP1_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtIP1, TxtIP2);
        }

        private void TxtIP2_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtIP2, TxtIP3);
        }

        private void TxtIP3_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtIP3, TxtIP4);
        }
        private void TxtIP4_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtIP4, null);
        }

        //Mask Configuration
        private void TxtMask1_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtMask1, TxtMask2);
        }

        private void TxtMask2_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtMask2, TxtMask3);
        }

        private void TxtMask3_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtMask3, TxtMask4);
        }

        private void TxtMask4_TextChanged(object sender, EventArgs e)
        {
            HandleInputIP(TxtMask4, null);
        }

        //Adress Formatting
        private void HandleInputIP(TextBox currentTextBox, TextBox nextTextBox)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(currentTextBox.Text, @"^\d*$"))
            {
                MessageBox.Show("Veuillez n'entrer que des chiffres.", "Erreur de saisie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentTextBox.Text = string.Concat(currentTextBox.Text.Where(char.IsDigit));
                currentTextBox.SelectionStart = currentTextBox.Text.Length;
                return;
            }

            if (currentTextBox.Text.Length == 3 && nextTextBox != null) nextTextBox.Focus();
            
        }
        #endregion

        public List<string> getStaionName()
        {
            return lsStation;
        }

        private void BpValidate_Click(object sender, EventArgs e)
        {
            NewDevice();
            this.Close();
        }

        private void BpAddStation_Click(object sender, EventArgs e)
        {
            NewDevice();
        }
    }
}
