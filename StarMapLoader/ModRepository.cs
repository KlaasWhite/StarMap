using System.Text.Json;
using StarMap.Types.Proto.IPC;

namespace StarMapLoader
{
    internal class ModRepository
    {
        public List<ManagedModInformation> LoadedModInformation { get; private set; } = [];
        public bool HasChanges { get; private set; }

        private readonly string _modsPath;
        private readonly ModDownloader _downloader = new();

        private ManagedModUpdate[] _changes = [];


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
                File.WriteAllText(filePath, JsonSerializer.Serialize(new List<ManagedModInformation>
                {
                    new ManagedModInformation()
                    {
                        Name = "TestMod1",
                        Version = "2.0.0.0"
                    }
                }));
            }

            string jsonString = File.ReadAllText(Path.Combine(modsPath, "starmap.json"));

            LoadedModInformation = JsonSerializer.Deserialize<List<ManagedModInformation>>(jsonString) ?? [];
        }

        public string[] GetPossibleMods()
        {
            return [.. _downloader.GetModsFromStore().Keys];
        }

        public ModInformation GetModInformation(string modName)
        {

            return _downloader.GetModsFromStore()[modName];
        }

        public void SetModUpdates(ManagedModUpdate[] updates)
        {
            _changes = updates;
            HasChanges = true;
        }

        public void ApplyModUpdates()
        {
            HasChanges = false;

            foreach (var mod in _changes)
            {
                try
                {
                    var directoryPath = Path.Combine(_modsPath, mod.Name);

                    if (Directory.Exists(directoryPath))
                    {
                        Directory.Delete(directoryPath, true);
                    }

                    Directory.CreateDirectory(directoryPath);

                    if (!_downloader.DownloadMod(mod.Name, mod.AfterVersion, directoryPath) && !string.IsNullOrEmpty(mod.BeforeVersion))
                    {
                        Directory.Delete(directoryPath, true);
                        Directory.CreateDirectory(directoryPath);
                        _downloader.DownloadMod(mod.Name, mod.BeforeVersion, directoryPath);
                    }
                    else
                    {
                        var newModInfo = new ManagedModInformation()
                        {
                            Version = mod.AfterVersion,
                            Name = mod.Name
                        };

                        var index = LoadedModInformation.FindIndex(modInfo => modInfo.Name == mod.Name);

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
                    Console.WriteLine($"Unable to apply update for mod: {mod.Name} to version {mod.AfterVersion}: {ex}");
                }
            }

            File.WriteAllText(Path.Combine(_modsPath, "starmap.json"), JsonSerializer.Serialize(LoadedModInformation));
        }
    }
}
