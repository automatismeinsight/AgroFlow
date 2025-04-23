using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Common
{
    /// <summary>
    /// Provides utilities for managing the current TIA Portal version and validating Openness DLLs.
    /// </summary>
    public static class TIAVersionManager
    {
        /// <summary>
        /// Holds the current selected TIA Portal version.
        /// </summary>
        private static string _currentVersion;

        /// <summary>
        /// Event triggered when the TIA version changes.
        /// The new version string is provided as event data.
        /// </summary>
        public static event EventHandler<string> VersionChanged;

        /// <summary>
        /// List of required Openness DLLs for TIA.
        /// </summary>
        public static readonly string[] OpennessDlls = new string[]
        {
            "Siemens.Engineering.dll",
            "Siemens.Engineering.Hmi.dll"
        };

        /// <summary>
        /// Indicates if it is the first version selection in the current session.
        /// </summary>
        public static bool IsFirstSelection { get; set; } = true;

        /// <summary>
        /// Gets or sets the current TIA Portal version.
        /// Triggers the VersionChanged event if the value changes.
        /// </summary>
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

        /// <summary>
        /// Sets the current TIA Portal version.
        /// </summary>
        /// <param name="version">The version string to set as current.</param>
        public static void SetVersion(string version)
        {
            CurrentVersion = version;
        }

        /// <summary>
        /// Gets the full path for a requested Openness DLL for the current TIA version.
        /// </summary>
        /// <param name="assemblyName">The DLL file name to look for.</param>
        /// <returns>The full DLL path if available, otherwise null.</returns>
        public static string GetOpennessDllPath(string assemblyName)
        {
            string tiaVersion = CurrentVersion;
            string dllPath = $"C:\\Program Files\\Siemens\\Automation\\Portal V{tiaVersion}\\PublicAPI\\V{tiaVersion}\\{assemblyName}";

            if (File.Exists(dllPath))
                return dllPath;

            return null;
        }

        /// <summary>
        /// Checks if all required Openness DLLs are available for the current TIA version.
        /// </summary>
        /// <returns>True if all DLLs are available, otherwise false.</returns>
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

    /// <summary>
    /// Provides methods for dynamic assembly loading and control management related to TIA Openness.
    /// </summary>
    public static class TIAAssemblyLoader
    {
        /// <summary>
        /// Event handler for resolving TIA Openness assemblies dynamically.
        /// </summary>
        /// <param name="sender">The event sender (not used).</param>
        /// <param name="args">Assembly resolve event arguments.</param>
        /// <returns>The loaded assembly if found, otherwise null.</returns>
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
                    Console.WriteLine($"Loading {shortAssemblyName} from {dllPath}");
                    return Assembly.LoadFrom(dllPath);
                }
                else
                {
                    Console.WriteLine($"Failed to load: {shortAssemblyName} not found");
                }
            }
            return null;
        }

        /// <summary>
        /// Forces the loading of all main Openness assemblies for the current TIA version.
        /// Logs success or error information to the console.
        /// </summary>
        public static void ForceLoadAssemblies()
        {
            try
            {
                foreach (string dllName in new[] { "Siemens.Engineering.dll", "Siemens.Engineering.Hmi.dll" })
                {
                    string dllPath = TIAVersionManager.GetOpennessDllPath(dllName);
                    if (!string.IsNullOrEmpty(dllPath))
                    {
                        Console.WriteLine($"Forcing load of {dllName} from {dllPath}");
                        Assembly assembly = Assembly.LoadFrom(dllPath);
                        Console.WriteLine($"DLL loaded successfully: {assembly.FullName}");
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Cannot find {dllName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during DLL loading: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures a control to use dynamic assembly resolving and handles unloading on version change.
        /// </summary>
        /// <param name="control">The control to configure.</param>
        public static void SetupControl(Control control)
        {
            // Register event handlers
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            TIAVersionManager.VersionChanged += (s, newVersion) => UnloadControl(control, newVersion);

            // Force load assemblies
            ForceLoadAssemblies();

            // Unregister event handlers when the control is disposed
            control.Disposed += (s, e) =>
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                TIAVersionManager.VersionChanged -= (sender, version) => UnloadControl(control, version);
            };
        }

        /// <summary>
        /// Unloads and disposes a control when the TIA version changes.
        /// </summary>
        /// <param name="control">The control to unload.</param>
        /// <param name="newVersion">The new TIA version string.</param>
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

        /// <summary>
        /// Helper method for removing and disposing a control.
        /// </summary>
        /// <param name="control">The control to remove and dispose.</param>
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