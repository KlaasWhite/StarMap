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
            if (!File.Exists("./StarMapConfig.json"))
            {
                Console.WriteLine("Please fill the StarMapConfig.json and restart the program");
                File.WriteAllText("./StarMapConfig.json", JsonSerializer.Serialize(new LoaderConfig(), new JsonSerializerOptions { WriteIndented = true }));
                return false;
            }
        
            var jsonString = File.ReadAllText("./StarMapConfig.json");
            var config = JsonSerializer.Deserialize<LoaderConfig>(jsonString);
        
            if (config is null) return false;
        
            if (string.IsNullOrEmpty(config.GameLocation))
            {
                Console.WriteLine("The 'GameLocation' property in StarMapConfig.json is either empty or points to a non-existing file.");
                return false;
            }
        
            string path = config.GameLocation;
        
            if (Directory.Exists(path))
            {
                path = Path.Combine(path, "KSA.dll");
            }
        
            if (!File.Exists(path))
            {
                Console.WriteLine("Could not find KSA.dll. Make sure the folder or file path is correct:");
                Console.WriteLine(path);
                return false;
            }
        
            GameLocation = path;
            return true;
        }

        public string GameLocation { get; set; } = "";
        public string RepositoryLocation { get; set; } = "";
    }
}
