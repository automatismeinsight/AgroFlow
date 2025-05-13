using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpennessV16;
using ReceptionDeProjet;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Tags;

namespace GenerationMAQ
{
    public class GenerateMAQ
    {
        /// <summary>
        /// Interface for comparing the TIA project and retrieving device information.
        /// </summary>
        public CompareTIA oCompareTiaPLC;
        public GenerateMAQ() 
        {
            oCompareTiaPLC = new CompareTIA();
        }

        public ReceptionDeProjet.MyProject GetMaqInTiaProject(HMATIAOpenness_V16 tiaInterface)
        {
            var resultProject = new ReceptionDeProjet.MyProject
            {
                sName = tiaInterface.m_oTiaProject.Name
            };
            try
            {
                foreach (var device in tiaInterface.m_oTiaProject.Devices)
                {
                    if (device.DeviceItems.Count <= 1)
                    {
                        Console.WriteLine("Empty device detected");
                        continue;
                    }

                    var mainModule = oCompareTiaPLC.FindMainModule(device);
                    if (mainModule == null)
                    {
                        Console.WriteLine("Main module not found");
                        continue;
                    }
                    try
                    {
                        var automate = new MyAutomate
                        {
                            sName = device.Name
                        };
                        GetAllTags(mainModule, automate);
                        resultProject.AddAutomate(automate);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing device {device.Name}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing TIA project: {ex.Message}");
            }

            return resultProject;
        }

        public void GetAllTags(DeviceItem mainModule,MyAutomate automate)
        {
            var softwareContainer = mainModule.GetService<SoftwareContainer>();
            var plcSoftware = softwareContainer?.Software as PlcSoftware ?? throw new Exception("Unable to retrieve PlcSoftware for the specified CPU.");
            

            foreach (PlcTagTable oTagTable in plcSoftware.TagTableGroup.TagTables)
            {
                foreach (PlcTag oTag in oTagTable.Tags)
                {
                    string sTagComment = string.Empty;
                    if (oTag.Name != null)
                    {
                        
                        foreach (MultilingualTextItem oTagCommentText in oTag.Comment.Items)
                        {
                            if (oTagCommentText.Text.ToString() != null && oTagCommentText.Text.Count() > 1)
                            {
                                sTagComment = oTagCommentText.Text.ToString();
                                Console.WriteLine($"Tag comment: {sTagComment}"); // Debugging line
                            }

                        }
                        if (oTag.LogicalAddress.StartsWith("%I"))
                        {
                            var tag = new MyTag
                            {
                                sName = oTag.Name,
                                sAddress = oTag.LogicalAddress,
                                sType = oTag.DataTypeName.ToString(),
                                sComment = sTagComment
                            };
                            automate.AddTagIn(tag);
                        }
                        else if (oTag.LogicalAddress.StartsWith("%Q"))
                        {
                            var tag = new MyTag
                            {
                                sName = oTag.Name,
                                sAddress = oTag.LogicalAddress,
                                sType = oTag.DataTypeName.ToString(),
                                sComment = sTagComment
                            };
                            automate.AddTagOut(tag);
                        }
                        else if (oTag.LogicalAddress.StartsWith("%M"))
                        {
                            var tag = new MyTag
                            {
                                sName = oTag.Name,
                                sAddress = oTag.LogicalAddress,
                                sType = oTag.DataTypeName.ToString(),
                                sComment = sTagComment
                            };
                            automate.AddTagMem(tag);
                        }
                        else
                        {
                            Console.WriteLine($"Unknown tag type for {oTag.Name}: {oTag.LogicalAddress}");
                        }
                    }
                }
            }
        }

    }
}
