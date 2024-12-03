using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public static class AssemblyManager
    {
        private static List<Assembly> _loadedAssemblies = new List<Assembly>();

        public static void LoadAssemblies(string[] versions)
        {
            foreach (string version in versions)
            {
                try
                {
                    string assemblyPath = $@"C:\Program Files\Siemens\Automation\Portal V{version}\PublicAPI\";
                    string[] dllFiles = Directory.GetFiles(assemblyPath, "*.dll");

                    foreach (string dllPath in dllFiles)
                    {
                        try
                        {
                            Assembly assembly = Assembly.LoadFile(dllPath);
                            _loadedAssemblies.Add(assembly);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Erreur de chargement DLL {dllPath}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur version V{version}: {ex.Message}");
                }
            }
        }

        public static Assembly[] GetLoadedAssemblies()
        {
            return _loadedAssemblies.ToArray();
        }

        public static Type GetType(string typeName)
        {
            foreach (Assembly assembly in _loadedAssemblies)
            {
                Type type = assembly.GetType(typeName, false);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
