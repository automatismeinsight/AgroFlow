using Siemens.Engineering.HW;
using Siemens.Engineering;
using Siemens.Engineering.HW.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Siemens.Engineering.Online;
using Siemens.Engineering.Connection;

namespace CreationDeProjet
{
    internal class TIAGenerator
    {
        public bool ProjectGenerator(ref string sError, ref TiaPortal oTiaPortal, ref Project oTiaProject, string sProjecName, string sProjectPath)
        {
            try
            {
                //Vérifiez si TIA Portal est déjà en cours d'exécution
                oTiaPortal = new TiaPortal(TiaPortalMode.WithUserInterface);

                DirectoryInfo directoryInfo = new DirectoryInfo(sProjectPath);

                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                //Créez le projet
                oTiaProject = oTiaPortal.Projects.Create(directoryInfo, sProjecName);
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return false;
            }

            return true;
        }

        public bool DeviceGenerator(ref string sError, ref Project oTiaProject)
        {
            try
            {
                if (oTiaProject == null)
                {
                    sError = "Aucun projet ouvert. Veuillez créer ou ouvrir un projet d'abord.";
                    return false;
                }
                DeviceComposition devices = oTiaProject.Devices;
                Device deviceWithItem = devices.CreateWithItem(
                    "OrderNumber:6ES7 511-1AL03-0AB0/V3.1", // Numéro de commande pour un S7-1510
                    "PLC_1",                                // Nom de l'élément d'appareil
                    "MyPLC"                                 // Nom de l'appareil
                );

                // Vérifier si l'appareil a été créé
                if (deviceWithItem == null)
                {
                    sError = "Échec de la création de l'appareil.";
                    return false;
                }

                SetNodeAddressAndMask(deviceWithItem, "192.168.07.99", "255.255.0.0");

            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return false;
            }
            return true;
        }

        public static void SetNodeAddressAndMask(Device device, string address, string subnetMask)
        {
            //set PLC IP address and subnet mask
            Console.WriteLine("Changing " + device.Name + " IP address to " + address);
            if (device != null)
            {
                NetworkInterface itf = null;
                int idx1 = 0, idx2 = 0;
                bool foundIt = false;

                for (idx1 = 0; idx1 < device.DeviceItems.Count; idx1++)
                {
                    DeviceItem di1 = device.DeviceItems.ElementAt(idx1);
                    for (idx2 = 0; idx2 < di1.DeviceItems.Count; idx2++)
                    {
                        DeviceItem di2 = di1.DeviceItems.ElementAt(idx2);
                        itf = ((IEngineeringServiceProvider)di2).GetService<NetworkInterface>();
                        if (itf != null)
                        {
                            foundIt = true;
                            break;
                        }
                    }
                    if (foundIt)
                    {
                        Console.WriteLine("Found network interface at " + idx1 + ", " + idx2);
                        break;
                    }

                }
                if (foundIt)
                {
                    //change the IP address
                    Node node = itf.Nodes.ElementAt(0);
                    if (node != null)
                    {
                        IEngineeringObject nodeObject = (IEngineeringObject)node;
                        nodeObject.SetAttribute("Address", address);
                        
                    }
                    else
                        Console.WriteLine("Unable to locate node");
                }
                else
                    Console.WriteLine("Unable to locate profinet interface");

            }
            else
                Console.WriteLine("Device is null");


        }
    }
}
