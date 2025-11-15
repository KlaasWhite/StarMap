using StarMap.Types.Proto.IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarMap.Types
{
    public class LoaderConfig
    {

        public bool TryLoadConfig()
        {
            if (!File.Exists("./StarMapConfig.json")) {
                Console.WriteLine("Please fill the StarMapConfig.json and restart the program");
                File.Create("./StarMapConfig.json").Dispose();
                File.WriteAllText("./StarMapConfig.json", JsonSerializer.Serialize(new LoaderConfig()));
                Console.ReadLine();
                return false;
            }

            var jsonString = File.ReadAllText("./StarMapConfig.json");
            var config = JsonSerializer.Deserialize<LoaderConfig>(jsonString);

            if (config is null) return false;

            if (string.IsNullOrEmpty(config.GameLocation) || !File.Exists(config.GameLocation))
            {
                Console.WriteLine("The 'GameLocation' property in StarMapConfig.json is either empty or points to a non-existing file.");
                Console.ReadLine();
                return false;
            }

            GameLocation = config.GameLocation;
            return true;
        }

        public string GameLocation { get; set; } = "";
        public string RepositoryLocation { get; set; } = "";
    }
}
