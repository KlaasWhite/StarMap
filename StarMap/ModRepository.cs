using DummyProgram;
using StarMap.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tomlyn.Model;
using Tomlyn;
using System.IO;

namespace StarMap
{
    internal class ModRepository : IModRepository
    {
        public List<LoadedModInformation> LoadedModInformation { get; private set; } = [];
        public bool HasChanges { get; private set; }

        private readonly string _modsPath;
        private readonly ModDownloader _downloader = new();

        private (string modName, Version? before, Version after)[] _changes = [];
        

        public ModRepository(string modsPath)
        {
            _modsPath = modsPath;

            if (!Directory.Exists(modsPath)) 
            { 
                Directory.CreateDirectory(modsPath);
            }

            var filePath = Path.Combine(modsPath, "starmap.json");
            if (!File.Exists(filePath)) 
            { 
                File.Create(filePath).Dispose();
                File.WriteAllText(filePath, JsonSerializer.Serialize(new List<LoadedModInformation>
                {
                    new LoadedModInformation()
                    {
                        Name = "TestMod1",
                        ModVersion = Version.Parse("2.0.0.0")
                    }
                }));
            }

            string jsonString = File.ReadAllText(Path.Combine(modsPath, "starmap.json"));

            LoadedModInformation = JsonSerializer.Deserialize<List<LoadedModInformation>>(jsonString) ?? [];
        }

        public string[] GetPossibleMods()
        {
            return [.. _downloader.GetModsFromStore().Keys];
        }

        public ModInformation GetModInformation(string modName)
        {

            return _downloader.GetModsFromStore()[modName];
        }

        public void SetModUpdates(IEnumerable<(string modName, Version? before, Version after)> updates)
        {
            _changes = updates.ToArray();
            HasChanges = true;
        }

        public void ApplyModUpdates()
        {
            HasChanges = false;

            foreach (var mod in _changes) 
            {
                try
                {
                    var directoryPath = Path.Combine(_modsPath, mod.modName);

                    if (Directory.Exists(directoryPath))
                    {
                        Directory.Delete(directoryPath, true);
                    }

                    Directory.CreateDirectory(directoryPath);

                    if (!_downloader.DownloadMod(mod.modName, mod.after, directoryPath) && mod.before is not null)
                    {
                        Directory.Delete(directoryPath, true);
                        Directory.CreateDirectory(directoryPath);
                        _downloader.DownloadMod(mod.modName, mod.before, directoryPath);
                    }
                    else
                    {
                        var newModInfo = new LoadedModInformation()
                        {
                            ModVersion = mod.after,
                            Name = mod.modName
                        };

                        var index = LoadedModInformation.FindIndex(modInfo => modInfo.Name == mod.modName);
                        
                        if (index >= 0)
                        {
                            LoadedModInformation[index] = newModInfo;
                        }
                        else
                        {
                            LoadedModInformation.Add(newModInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to apply update for mod: {mod.modName} to version {mod.after}: {ex}");
                }
            }

            File.WriteAllText(Path.Combine(_modsPath, "starmap.json"), JsonSerializer.Serialize(LoadedModInformation));
        }
    }
}
