using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AideAuDiagnostic.TiaExplorer;
using GlobalsOPCUA;
using ClosedXML.Excel;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace ReceptionDeProjet
{
    public partial class ReceptionDeProjet: UserControl
    {
        protected string sCdcFilePath = null;

        // Objet interface Tia Portal
        public ExploreTiaPLC oExploreTiaPLC;
        // Paramètres généraux pour l'application
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();
        //Liste des instructions pour le FC
        private readonly List<string> lsDataCollection = new List<string>();

        //Liste des devices dans le Cdc
        private readonly Dictionary<Tuple<int, int>, string> dDevicesCdc = new Dictionary<Tuple<int, int>, string>();
        //Liste des devices dans le Projet
        private readonly Dictionary<Tuple<int, int>, string> dDevicesProjet = new Dictionary<Tuple<int, int>, string>();

        // Dictionnaires pour stocker les paires (Nom -> IP)
        Dictionary<string, string> devicesCdc = new Dictionary<string, string>();
        Dictionary<string, string> devicesProjet = new Dictionary<string, string>();

        public ReceptionDeProjet()
        {
            InitializeComponent();

            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
        }

        private void BpCdcLoad_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "xlsx files (*.xlsx) | *.xlsx";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            sCdcFilePath += filePath;
            UpdateInfo($"Fichier {sCdcFilePath.Split('\\').Last()} chargé");
        }

        private void BpSelectProject_Click(object sender, EventArgs e)
        {
            string sProjectName = string.Empty;

            UpdateInfo("-");

            if (GetTiaProject(ref sProjectName) == true)
            {
                UpdateInfo(string.Format(@"Le projet cible : {0} est bien sélectionné", sProjectName));
                
            }
            else
            {
                UpdateInfo("Le projet cible n'est pas sélectionné");
               
            }
        }

        private bool GetTiaProject(ref string sProjectName)
        {
            bool bRet = true;
            string sError = string.Empty;

            sProjectName = string.Empty;

            // Test si le projet Tia Portal est déja sélectionné ?
            if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
            {
                // Sélection du projet Tia Portal
                if (oExploreTiaPLC.ChooseTiaProject(ref sError) == false)
                {
                    bRet = false;
                    sProjectName = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
                }
            }
            return bRet;
        }

        private void BpVerification_Click(object sender, EventArgs e)
        {
            int i = 1;
            var plcInfoList = oExploreTiaPLC.GetPlcDevicesInfo();

            UpdateInfo("-");
            UpdateInfo("Device trouvé : ");
            foreach (var plc in plcInfoList)
            {
                UpdateInfo($"Nom: {plc.Name}, IP: {plc.IPAddress}");
                dDevicesProjet.Add(new Tuple<int, int>(i, 1), plc.Name);
                dDevicesProjet.Add(new Tuple<int, int>(i, 2), plc.IPAddress);
                i++;
            }
            UpdateInfo("-");
            CompareProject();
        }

        private void CompareProject()
        {
            int iNbDevice = 0;
            var plcInfoList = oExploreTiaPLC.GetPlcDevicesInfo();
            ReadExcel(ref iNbDevice);

            // Reconstruction du dictionnaire depuis dDevicesCdc (Excel)
            for (int i = 2; i <= iNbDevice + 1; i++)  // Démarre à 2 car iNbDevice vient d'Excel (1ère ligne ignorée)
            {
                if (dDevicesCdc.TryGetValue(System.Tuple.Create(i, 1), out string deviceName) &&
                    dDevicesCdc.TryGetValue(System.Tuple.Create(i, 2), out string deviceIP))
                {
                    devicesCdc[deviceName] = deviceIP;
                }
            }

            // Reconstruction du dictionnaire depuis dDevicesProjet (projet)
            foreach (var plc in plcInfoList)
            {
                devicesProjet[plc.Name] = plc.IPAddress;
            }

            // Comparaison des devices par Nom
            foreach (var kvp in devicesCdc)
            {
                string deviceName = kvp.Key;
                string ipCdc = kvp.Value;

                if (devicesProjet.TryGetValue(deviceName, out string ipProjet))
                {
                    if (!ipCdc.Equals(ipProjet))
                    {
                        UpdateInfo($"⚠ Différence détectée pour {deviceName} : {ipCdc} (Cdc) ≠ {ipProjet} (Projet)");
                    }
                }
                else
                {
                    UpdateInfo($"❌ Le device {deviceName} est dans le fichier de référence mais absent du projet.");
                }
            }

            // Vérifier les devices présents dans le projet mais absents du fichier de référence
            foreach (var kvp in devicesProjet)
            {
                string deviceName = kvp.Key;
                if (!devicesCdc.ContainsKey(deviceName))
                {
                    UpdateInfo($"❌ Le device {deviceName} est dans le projet mais absent du fichier de référence.");
                }
            }
        }

        private void ReadExcel(ref int iNbDevice)
        {
            
            using (XLWorkbook wb = new XLWorkbook(sCdcFilePath))
            {
                // Récupère la première feuille
                var ws = wb.Worksheets.First();
                var range = ws.RangeUsed();

                // Parcours des lignes
                for (int i = 2; i < range.RowCount() + 1; i++)
                {
                    iNbDevice++;
                    // Parcours des colonnes
                    for (int j = 1; j < range.ColumnCount() + 1; j++)
                    {
                        dDevicesCdc.Add(new Tuple<int, int>(i, j), ws.Cell(i, j).Value.ToString());
                    }
                }
                UpdateInfo("Fichier lu avec succès");
            }
        }

        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now; // Actual date and time
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "----------------------------------------------------------------------------------------------------------------------\n";
            }
            else
            {
                sFullMessage = $"{sCurrentDateTime} {sMessage}\n";
            }
            // Ajoute le message au RichTextBox
            txBInformations.AppendText(sFullMessage);

            // Défile automatiquement vers le bas
            txBInformations.SelectionStart = txBInformations.Text.Length;
            txBInformations.ScrollToCaret();
        }

        private void BpPdfExport_Click(object sender, EventArgs e)
        {
            string filename = getFilePath();

            if (!string.IsNullOrEmpty(filename))
            {
                // Création du document PDF
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Comparaison des Devices et IPs";
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont titleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                XFont headerFont = new XFont("Verdana", 12, XFontStyle.Bold);
                XFont bodyFont = new XFont("Verdana", 10, XFontStyle.Regular);

                int y = 30; // Position verticale initiale

                // Titre
                gfx.DrawString("Comparaison des Devices et IPs", titleFont, XBrushes.Black, new XPoint(40, y));
                y += 30;

                // Largeur des colonnes
                int col1Width = 200;
                int col2Width = 150;
                int rowHeight = 20;

                // Dessiner tableau des devices du CDC
                gfx.DrawString("Devices du CDC", headerFont, XBrushes.Black, new XPoint(40, y));
                y += 20;

                gfx.DrawRectangle(XPens.Black, 40, y, col1Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, 40 + col1Width, y, col2Width, rowHeight);
                gfx.DrawString("Nom du Device", bodyFont, XBrushes.Black, new XPoint(50, y + 15));
                gfx.DrawString("Adresse IP", bodyFont, XBrushes.Black, new XPoint(50 + col1Width, y + 15));
                y += rowHeight;



                foreach (var kvp in devicesCdc)
                {
                    gfx.DrawRectangle(XPens.Black, 40, y, col1Width, rowHeight);
                    gfx.DrawRectangle(XPens.Black, 40 + col1Width, y, col2Width, rowHeight);
                    gfx.DrawString(kvp.Key, bodyFont, XBrushes.Black, new XPoint(50, y + 15));
                    gfx.DrawString(kvp.Value, bodyFont, XBrushes.Black, new XPoint(50 + col1Width, y + 15));
                    y += rowHeight;
                }

                y += 30; // Espacement entre les tableaux

                // Dessiner tableau des devices du projet
                gfx.DrawString("Devices du Projet", headerFont, XBrushes.Black, new XPoint(40, y));
                y += 20;

                gfx.DrawRectangle(XPens.Black, 40, y, col1Width, rowHeight);
                gfx.DrawRectangle(XPens.Black, 40 + col1Width, y, col2Width, rowHeight);
                gfx.DrawString("Nom du Device", bodyFont, XBrushes.Black, new XPoint(50, y + 15));
                gfx.DrawString("Adresse IP", bodyFont, XBrushes.Black, new XPoint(50 + col1Width, y + 15));
                y += rowHeight;

                foreach (var kvp in devicesProjet)
                {
                    gfx.DrawRectangle(XPens.Black, 40, y, col1Width, rowHeight);
                    gfx.DrawRectangle(XPens.Black, 40 + col1Width, y, col2Width, rowHeight);
                    gfx.DrawString(kvp.Key, bodyFont, XBrushes.Black, new XPoint(50, y + 15));
                    gfx.DrawString(kvp.Value, bodyFont, XBrushes.Black, new XPoint(50 + col1Width, y + 15));
                    y += rowHeight;
                }

                // Sauvegarde du PDF
                document.Save(filename);
                UpdateInfo("-");
                UpdateInfo("Le PDF a été exporté avec succès.");
            }
        }

        public string getFilePath()
        {
            // Création de la fenêtre de dialogue pour enregistrer le fichier
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // Configuration de la fenêtre de dialogue
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.Title = $"ExportPDF";
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = ".pdf";
            saveFileDialog1.Filter = "PDF files (*.pdf) |  *.pdf";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName;
            }
            return "";
        }
    }
}
