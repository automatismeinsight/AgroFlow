using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GlobalsOPCUA
{
    /// <summary>
    /// Classe utilitaire pour la gestion des fichiers INI (lecture uniquement).
    /// </summary>
    internal class IniFile
    {
        /// <summary>
        /// Chemin complet du fichier INI utilisé.
        /// </summary>
        private string Path;

        /// <summary>
        /// Nom de l'exécutable courant (utilisé comme section/fichier par défaut).
        /// </summary>
        private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(
            string Section,
            string Key,
            string Default,
            StringBuilder RetVal,
            int Size,
            string FilePath);

        /// <summary>
        /// Initialise une instance de <see cref="IniFile"/>.
        /// Si aucun chemin n'est donné, prend le nom de l'exe + ".ini".
        /// </summary>
        /// <param name="IniPath">Chemin du fichier INI (optionnel).</param>
        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        /// <summary>
        /// Lit la valeur d'une clé dans une section donnée du fichier INI.
        /// </summary>
        /// <param name="Key">Nom de la clé à lire.</param>
        /// <param name="Section">Nom de la section (optionnel, défaut = nom de l'exe).</param>
        /// <returns>Valeur lue, ou chaîne vide si non trouvée.</returns>
        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }
    }
}