using System;
using System.IO;

namespace Common
{
    public static class TIAVersionManager
    {
        private static string _currentVersion;
        public static event EventHandler<string> VersionChanged;

        private static readonly string[] OpennessDlls = new string[]
        {
            "Siemens.Engineering.dll",
            "Siemens.Engineering.Hmi.dll"
        };

        public static string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                if (_currentVersion != value)
                {
                    _currentVersion = value;
                    VersionChanged?.Invoke(null, _currentVersion);
                }
            }
        }
        public static void SetVersion(string version)
        {
            CurrentVersion = version;
        }

        public static string GetOpennessDllPath(string assemblyName)
        {
            string tiaVersion = CurrentVersion;
            string dllPath = $"C:\\Program Files\\Siemens\\Automation\\Portal V{tiaVersion}\\PublicAPI\\V{tiaVersion}\\{assemblyName}";

            if (File.Exists(dllPath))
                return dllPath;

            return null;
        }
        public static bool AreAllDllsAvailable()
        {
            if (string.IsNullOrEmpty(CurrentVersion))
                return false;

            foreach (string dll in OpennessDlls)
            {
                if (GetOpennessDllPath(dll) == null)
                    return false;
            }

            return true;
        }
    }
}