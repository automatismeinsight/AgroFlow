using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Common
{
    public static class TIAVersionManager
    {
        private static string _currentVersion;
        public static event EventHandler<string> VersionChanged;
        // Liste des DLL requises pour Openness
        public static readonly string[] OpennessDlls = new string[]
        {
        "Siemens.Engineering.dll",
        "Siemens.Engineering.Hmi.dll"
        };

        public static bool IsFirstSelection { get; set; } = true;

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

        // Méthode de commodité pour définir la version
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

        // Cette méthode vérifie si toutes les DLL requises sont disponibles
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

    public static class TIAAssemblyLoader
    {
        // Gestionnaire d'événements pour résoudre les assemblies
        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string shortAssemblyName = new AssemblyName(args.Name).Name;

            if (shortAssemblyName.StartsWith("Siemens.Engineering"))
            {
                if (!shortAssemblyName.EndsWith(".dll"))
                    shortAssemblyName += ".dll";

                string dllPath = TIAVersionManager.GetOpennessDllPath(shortAssemblyName);

                if (!string.IsNullOrEmpty(dllPath))
                {
                    Console.WriteLine($"Chargement de {shortAssemblyName} depuis {dllPath}");
                    return Assembly.LoadFrom(dllPath);
                }
                else
                {
                    Console.WriteLine($"Échec de chargement: {shortAssemblyName} introuvable");
                }
            }
            return null;
        }

        // Chargement forcé des assemblies principales
        public static void ForceLoadAssemblies()
        {
            try
            {
                foreach (string dllName in new[] { "Siemens.Engineering.dll", "Siemens.Engineering.Hmi.dll" })
                {
                    string dllPath = TIAVersionManager.GetOpennessDllPath(dllName);
                    if (!string.IsNullOrEmpty(dllPath))
                    {
                        Console.WriteLine($"Chargement forcé de {dllName} depuis {dllPath}");
                        Assembly assembly = Assembly.LoadFrom(dllPath);
                        Console.WriteLine($"DLL chargée avec succès: {assembly.FullName}");
                    }
                    else
                    {
                        Console.WriteLine($"ERREUR: Impossible de trouver {dllName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors du chargement des DLLs: {ex.Message}");
            }
        }

        // Méthode pour configurer un contrôle avec les gestionnaires d'événements
        public static void SetupControl(Control control)
        {
            // Enregistrer les gestionnaires d'événements
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            TIAVersionManager.VersionChanged += (s, newVersion) => UnloadControl(control, newVersion);

            // Forcer le chargement des assemblies
            ForceLoadAssemblies();

            // S'assurer que les gestionnaires sont désenregistrés lors de la suppression
            control.Disposed += (s, e) =>
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                TIAVersionManager.VersionChanged -= (sender, version) => UnloadControl(control, version);
            };
        }

        // Méthode pour décharger proprement un contrôle
        private static void UnloadControl(Control control, string newVersion)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => UnloadControlImpl(control)));
            }
            else
            {
                UnloadControlImpl(control);
            }
        }

        private static void UnloadControlImpl(Control control)
        {
            if (control.Parent != null)
            {
                control.Parent.Controls.Remove(control);
            }
            control.Dispose();
        }
    }
}