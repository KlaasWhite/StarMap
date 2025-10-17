using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;

namespace DummyProgram
{
    public class ModLibrary
    {
        private List<Mod> _mods = [];

        public ModLibrary() { }
        public void LoadAll()
        {
            FetchMods();
        }

        public void FetchMods()
        {
            string modsPath = Path.GetFullPath("./mods");
            if (!Directory.Exists(modsPath))
            {
                return;
            }

            foreach (var folder in Directory.GetDirectories(modsPath))
            {
                string folderName = Path.GetFileName(folder);
                string tomlPath = Path.Combine(folder, folderName + ".toml");

                if (!File.Exists(tomlPath))
                    continue;

                try
                {
                    string tomlContent = File.ReadAllText(tomlPath);
                    TomlTable tomlTable = Toml.ToModel(tomlContent);

                    if (tomlTable.ContainsKey("mod_type") && tomlTable["mod_type"]?.ToString() == "StarMap")
                    {
                        string dllPath = Path.Combine(folder, folderName + ".dll");

                        if (!File.Exists(dllPath))
                            continue;

                        AssemblyName assemblyName;
                        try
                        {
                            assemblyName = AssemblyName.GetAssemblyName(dllPath);
                        }
                        catch
                        {
                            continue;
                        }

                        var mod = new Mod
                        {
                            DirectoryPath = folder,
                            Assembly = assemblyName
                        };

                        mod.PrepareSystems();
                        _mods.Add(mod);

                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
