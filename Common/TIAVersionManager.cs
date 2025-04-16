using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TIAVersionManager
    {
        private static string _currentVersion;

        // Événement pour notifier les changements de version
        public static event EventHandler<string> VersionChanged;

        public static string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                if (_currentVersion != value)
                {
                    _currentVersion = value;
                    // Déclencher l'événement de changement de version
                    VersionChanged?.Invoke(null, _currentVersion);
                }
            }
        }

        // Méthode de commodité pour définir la version
        public static void SetVersion(string version)
        {
            CurrentVersion = version;
        }
    }
}
