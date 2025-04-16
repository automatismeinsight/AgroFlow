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
