using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AideAuDiagnostic.TiaExplorer;
using Common;
using GlobalsOPCUA;
using ReceptionDeProjet;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace GenerationMAQ
{
    public partial class GenerationMAQ : UserControl
    {
        /// <summary>
        /// Interface for exploring the TIA Portal project.
        /// </summary>
        public ExploreTiaPLC oExploreTiaPLC;

        /// <summary>
        /// Interface for comparing the TIA project and retrieving device information.
        /// </summary>
        public CompareTIA oCompareTiaPLC;

        public GenerateMAQ oGenerateMAQ;

        /// <summary>
        /// Global configuration and mapping settings for the PLC project.
        /// </summary>
        public static PLC_ProjectDefinitions oPLC_ProjectDefinitions = new PLC_ProjectDefinitions();

        /// <summary>
        /// Data storage dictionary for internal processing.
        /// </summary>
        private readonly Dictionary<Tuple<int, int>, object> dData = new Dictionary<Tuple<int, int>, object>();

        /// <summary>
        /// List of instructions to be generated for the FC (function code).
        /// </summary>
        private readonly List<string> lsDataCollection = new List<string>();

        /// <summary>
        /// Stores the loaded TIA Project information.
        /// </summary>
        Project oTiaProject = new Project();

        /// <summary>
        /// Path to the Export MAQ Excel file.
        /// </summary>
        protected string sExportMaqPath = null;

        public GenerationMAQ()
        {
            InitializeComponent();

            TIAAssemblyLoader.SetupControl(this);

            oExploreTiaPLC = new ExploreTiaPLC(oPLC_ProjectDefinitions, dData, lsDataCollection);
            oCompareTiaPLC = new CompareTIA();
            oGenerateMAQ = new GenerateMAQ();

            InitExcel();
        }

        /// <summary>
        /// Handles the "Load CDC" button click event.
        /// Allows the user to select a CDC Excel file and stores its path.
        /// </summary>
        private void InitExcel()
        {
            var filePath = @".\copy_of_ExportMAQ.xlsx";
            var sourcePath = @".\ExportMAQ.xlsx";

            try
            {
                File.Copy(sourcePath, filePath, overwrite: true);
                sExportMaqPath = filePath;
                UpdateInfo($"Fichier {filePath.Split('\\').Last()} chargé");
            }
            catch (IOException ex)
            {
                UpdateInfo($"Erreur lors de la copie : {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the "Select Project" button click event.
        /// Prompts the user to select a TIA Portal project and updates the info panel.
        /// </summary>
        private void BpSelectProject_Click(object sender, EventArgs e)
        {
            string sProjectName = string.Empty;

            if (GetTiaProject(ref sProjectName) == true)
            {
                UpdateInfo(string.Format($"Le projet cible : {sProjectName} est bien sélectionné"));
                UpdateInfo("-");
                BpGenration.Enabled = true;
            }
            else
            {
                UpdateInfo("Le projet cible n'est pas sélectionné");
            }
        }

        /// <summary>
        /// Selects a TIA Portal project via the <see cref="ExploreTiaPLC"/> interface.
        /// </summary>
        /// <param name="sProjectName">Returns the selected project name if successful.</param>
        /// <returns>True if a project is selected, otherwise false.</returns>
        private bool GetTiaProject(ref string sProjectName)
        {
            bool bRet = false;
            string sError = string.Empty;

            sProjectName = string.Empty;

            // Test if a TIA Portal project is already selected
            if (oExploreTiaPLC.GetTiaPortalProjectIsSelected() == false)
            {
                // Prompt user to select a TIA Portal project
                if (oExploreTiaPLC.ChooseTiaProject(ref sError) == true)
                {
                    bRet = true;
                    sProjectName = oExploreTiaPLC.oTiainterface.m_oTiaProject.Name;
                }
            }
            return bRet;
        }

        private void BpGenration_Click(object sender, EventArgs e)
        {
            UpdateInfo("Début du listage des tags...");
            txBInformations.Refresh();

            oTiaProject = oGenerateMAQ.GetMaqInTiaProject(oExploreTiaPLC.oTiainterface);

            UpdateInfo("-");
            // Display project information
            if (oTiaProject == null)
            {
                UpdateInfo("Erreur lors de la récupération des informations du projet");
                return;
            }
            UpdateInfo($"Projet : {oTiaProject.sName}");
            UpdateInfo($"Nombre d'automates : {oTiaProject.oAutomates.Count}");
            UpdateInfo("-");
            UpdateInfo("Device trouvé : ");
            foreach (Automate plc in oTiaProject.oAutomates)
            {
                UpdateInfo($"  - {plc.sName}");
                int tagCount = 0;
                if (ChkTagIn.Checked) tagCount += plc.oTagsIn.Count;
                if (ChkTagOut.Checked) tagCount += plc.oTagsOut.Count;
                if (ChkTagMem.Checked) tagCount += plc.oTagsMem.Count;
                UpdateInfo($"    Nombre de tags : {tagCount}");
                if(ChkTagIn.Checked) UpdateInfo($"    Nombre de tags IN : {plc.oTagsIn.Count}");
                if (ChkTagOut.Checked) UpdateInfo($"    Nombre de tags OUT : {plc.oTagsOut.Count}");
                if (ChkTagMem.Checked) UpdateInfo($"    Nombre de tags MEM : {plc.oTagsMem.Count}");

                if (ChkTagIn.Checked)
                {
                    UpdateInfo($"    Tags In :");
                    foreach (MyTag tag in plc.oTagsIn)
                    {
                        UpdateInfo($"      - {tag.sName}");
                    }
                }

                if (ChkTagOut.Checked)
                {
                    UpdateInfo($"    Tags Out :");
                    foreach (MyTag tag in plc.oTagsOut)
                    {
                        UpdateInfo($"      - {tag.sName}");
                    }
                }

                if (ChkTagMem.Checked)
                {
                    UpdateInfo($"    Tags Mem :");
                    foreach (MyTag tag in plc.oTagsMem)
                    {
                        UpdateInfo($"      - {tag.sName}");
                    }
                }
            }
            BpDownloadFile.Enabled = true;
        }

        /// <summary>
        /// Handles the "Download" button click event.
        /// Copy the result Excel file to a user-defined location.
        /// </summary>
        private void BpDownloadFile_Click(object sender, EventArgs e)
        {
            
            ExportTagToExcel();

            var now = DateTime.Now;
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Enregistrer le fichier sous";
                saveFileDialog.Filter = "Fichiers Excel Macro (*.xlsx)|*.xlsx|Tous les fichiers (*.*)|*.*";
                saveFileDialog.FileName = "ExportMAQ_" + oTiaProject.sName + "_" + $"{now.Day}{now.Month:D2}{now.Year:D2}";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string destinationPath = saveFileDialog.FileName;
                    string sourcePath = sExportMaqPath;

                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        UpdateInfo("Fichier enregistré avec succès.");
                    }
                    catch (Exception ex)
                    {
                        UpdateInfo($"Erreur lors de l'enregistrement : {ex.Message}");
                    }
                }
            }
        }

        private void ExportTagToExcel()
        {
            UpdateInfo("Exportation des données vers Excel...");
            try
            {
                using (XLWorkbook wb = new XLWorkbook(sExportMaqPath))
                {
                    var worksheetModel = wb.Worksheet("MODELE");

                    foreach (Automate plc in oTiaProject.oAutomates)
                    {
                        string newSheetName = plc.sName;

                        if(wb.Worksheets.Contains(newSheetName))
                        {
                            wb.Worksheet(newSheetName).Delete();
                        }

                        var newSheet = wb.Worksheets.Add(newSheetName);
                        var worksheet = wb.Worksheet(newSheetName);

                        int startRow = 2;
                        int currentRow = 0;

                        if (ChkTagIn.Checked)
                        {
                            worksheetModel.Range("A1:C1").CopyTo(newSheet.Cell("A1"));
                            currentRow = startRow;

                            foreach (MyTag tag in plc.oTagsIn)
                            {
                                worksheet.Cell("A" + currentRow).Value = tag.sAddress;
                                worksheet.Cell("B" + currentRow).Value = tag.sName;
                                worksheet.Cell("C" + currentRow).Value = tag.sType;

                                currentRow++;
                            }

                            if (ChkTagOut.Checked)
                            {
                                worksheetModel.Range("D1:F1").CopyTo(newSheet.Cell("D1"));
                                currentRow = startRow;

                                currentRow = startRow;
                                foreach (MyTag tag in plc.oTagsOut)
                                {
                                    worksheet.Cell("D" + currentRow).Value = tag.sAddress;
                                    worksheet.Cell("E" + currentRow).Value = tag.sName;
                                    worksheet.Cell("F" + currentRow).Value = tag.sType;
                                    currentRow++;
                                }

                                if (ChkTagMem.Checked)
                                {
                                    worksheetModel.Range("G1:I1").CopyTo(newSheet.Cell("G1"));
                                    currentRow = startRow;

                                    foreach (MyTag tag in plc.oTagsMem)
                                    {
                                        worksheet.Cell("G" + currentRow).Value = tag.sAddress;
                                        worksheet.Cell("H" + currentRow).Value = tag.sName;
                                        worksheet.Cell("I" + currentRow).Value = tag.sType;

                                        currentRow++;
                                    }
                                }

                            }
                            else if (ChkTagMem.Checked)
                            {
                                worksheetModel.Range("G1:I1").CopyTo(newSheet.Cell("D1"));
                                currentRow = startRow;
                                foreach (MyTag tag in plc.oTagsMem)
                                {
                                    worksheet.Cell("D" + currentRow).Value = tag.sAddress;
                                    worksheet.Cell("E" + currentRow).Value = tag.sName;
                                    worksheet.Cell("F" + currentRow).Value = tag.sType;
                                    currentRow++;
                                }
                            }
                        }
                        else if (ChkTagOut.Checked)
                        {
                            worksheetModel.Range("D1:F1").CopyTo(newSheet.Cell("A1"));
                            currentRow = startRow;
                            foreach (MyTag tag in plc.oTagsOut)
                            {
                                worksheet.Cell("A" + currentRow).Value = tag.sAddress;
                                worksheet.Cell("B" + currentRow).Value = tag.sName;
                                worksheet.Cell("C" + currentRow).Value = tag.sType;
                                currentRow++;
                            }
                            if (ChkTagMem.Checked)
                            {
                                worksheetModel.Range("G1:I1").CopyTo(newSheet.Cell("D1"));
                                currentRow = startRow;
                                foreach (MyTag tag in plc.oTagsMem)
                                {
                                    worksheet.Cell("D" + currentRow).Value = tag.sAddress;
                                    worksheet.Cell("E" + currentRow).Value = tag.sName;
                                    worksheet.Cell("F" + currentRow).Value = tag.sType;
                                    currentRow++;
                                }
                            }
                        }
                        else if (ChkTagMem.Checked)
                        {
                            worksheetModel.Range("G1:I1").CopyTo(newSheet.Cell("A1"));
                            currentRow = startRow;
                            foreach (MyTag tag in plc.oTagsMem)
                            {
                                worksheet.Cell("A" + currentRow).Value = tag.sAddress;
                                worksheet.Cell("B" + currentRow).Value = tag.sName;
                                worksheet.Cell("C" + currentRow).Value = tag.sType;
                                currentRow++;
                            }
                        }

                        int lastColumn = worksheet.LastColumnUsed().ColumnNumber();

                        for (int i = 1; i <= lastColumn; i++)
                        {
                            worksheet.Column(i).AdjustToContents();
                        }
                    }
                    worksheetModel.Hide();
                    wb.Worksheet(2).SetTabActive();
                    wb.Save();
                }
            }
            catch(Exception ex)
            {
                UpdateInfo($"Erreur lors de l'exportation : {ex.Message}");
            }
            finally
            {
                UpdateInfo("Exportation terminée.");
            }
        }

        /// <summary>
        /// Adds an informational message to the information RichTextBox and scrolls to the latest message.
        /// </summary>
        /// <param name="sMessage">The message to display. Use "-" for a section separator.</param>
        public void UpdateInfo(string sMessage)
        {
            var now = DateTime.Now;
            string sCurrentDateTime = $"{now.Year}/{now.Month:D2}/{now.Day:D2} {now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} - ";
            string sFullMessage;

            if (sMessage == "-")
            {
                sFullMessage = "----------------------------------------------------------------------------------------------------------------------------------------------\n";
            }
            else
            {
                sFullMessage = $"{sCurrentDateTime} {sMessage}\n";
            }
            // Add the message to the RichTextBox
            txBInformations.AppendText(sFullMessage);

            // Auto-scroll to the bottom
            txBInformations.SelectionStart = txBInformations.Text.Length;
            txBInformations.ScrollToCaret();
        }
    }
}
