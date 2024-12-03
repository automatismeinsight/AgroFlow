using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Automation.Peers;

namespace InterfaceMain
{
    internal class LoadReferences
    {
        public static void InitializeApp(List<string> versions)
        {
            LoadVersions(versions);
            // Charger les assemblies via la bibliothèque partagée
            SharedLibrary.AssemblyManager.LoadAssemblies(versions.ToArray());
        }

        private static void LoadVersions(List<string> versions)
        {
            string rootPath = @"C:\Program Files\Siemens\Automation\";
            Regex versionRegex = new Regex(@"^Portal\s*V(\d+)$", RegexOptions.IgnoreCase);

            foreach (string directory in Directory.GetDirectories(rootPath))
            {
                string folderName = Path.GetFileName(directory);
                Match match = versionRegex.Match(folderName);
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int version))
                    {
                        versions.Add(version.ToString());
                    }
                }
            }
        }

        public static string ExtractNumber(string input)
        {
            Match match = Regex.Match(input, @"\d+");
            return match.Success ? match.Value : string.Empty;
        }

        public static int LoadReference(string path)
        {
            if ((!File.Exists($"{path}Siemens.Engineering.dll")) || (!File.Exists($"{path}Siemens.Engineering.Hmi.dll")))
            {
                Console.WriteLine("DLL not found.");
                return 1;
            }

            try // Try to load the required references
            {
                // Load the required references
                Assembly.LoadFrom(Path.Combine(path, "Siemens.Engineering.dll"));
                Assembly.LoadFrom(Path.Combine(path, "Siemens.Engineering.Hmi.dll"));

                return 0;
            }
            catch (Exception e) // Catch any exceptions
            {
                Console.WriteLine(e.Message); // Write the exception message to the console
                return 1;
            }
        }

        public static string FormatString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // On divise la chaîne en mots basés sur les espaces
            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // On met la première lettre de chaque mot en majuscule et on les concatène
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string result = string.Concat(Array.ConvertAll(words, word => textInfo.ToTitleCase(word.ToLower())));

            return result;
        }
    }
}
