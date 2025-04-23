using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace InterfaceMain
{
    /// <summary>
    /// Provides utility methods for loading and formatting references, such as TIA versions.
    /// </summary>
    internal class LoadReferences
    {
        /// <summary>
        /// Initializes the application by loading available TIA versions into the provided list.
        /// </summary>
        /// <param name="versions">The list to populate with detected TIA version numbers.</param>
        public static void InitializeApp(List<string> versions)
        {
            LoadVersions(versions);
        }

        /// <summary>
        /// Scans the Siemens Automation root directory for installed TIA Portal versions and adds them to the specified list.
        /// </summary>
        /// <param name="versions">The list to populate with detected version numbers.</param>
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

        /// <summary>
        /// Extracts the first numeric sequence found in the input string.
        /// </summary>
        /// <param name="input">The string to search for a number.</param>
        /// <returns>The first number found as a string, or an empty string if no number is found.</returns>
        public static string ExtractNumber(string input)
        {
            Match match = Regex.Match(input, @"\d+");
            return match.Success ? match.Value : string.Empty;
        }

        /// <summary>
        /// Formats the input string by capitalizing the first letter of each word and concatenating them together.
        /// </summary>
        /// <param name="input">The input string to format.</param>
        /// <returns>The formatted string, or the original string if it is null or whitespace.</returns>
        public static string FormatString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string result = string.Concat(Array.ConvertAll(words, word => textInfo.ToTitleCase(word.ToLower())));

            return result;
        }
    }
}