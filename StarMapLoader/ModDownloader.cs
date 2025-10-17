using StarMap.Types.Proto.IPC;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMapLoader
{
    internal class ModDownloader
    {
        private const string ModLocation = "C:\\data\\programming\\game-modding\\KSA\\Mods";

        public bool DownloadMod(string modName, string modVersion, string location)
        {
            try
            {
                var sourceFile = Path.Combine(ModLocation, $"{modName}-{modVersion}.zip");
                var destinationFile = Path.Combine(location, $"{modName}-{modVersion}.zip");

                if (!File.Exists(sourceFile)) return false;

                File.Copy(sourceFile, destinationFile, true);

                ZipFile.ExtractToDirectory(destinationFile, location);
                File.Delete(destinationFile);

                return true;
            }
            catch { return false; }
        }

        public Dictionary<string, ModInformation> GetModsFromStore()
        {
            var mods = new Dictionary<string, ModInformation>();

            mods["TestMod1"] = new ModInformation()
            {
                Name = "TestMod1"
            };
            mods["TestMod1"].AvailableVersions.AddRange(["1.0.0.0", "2.0.0.0", "3.0.0.0"]);

            mods["TestMod2"] = new ModInformation()
            {
                Name = "TestMod2"
            };
            mods["TestMod2"].AvailableVersions.AddRange(["1.0.0.0", "2.0.0.0", "3.0.0.0"]);

            return mods;
        }
    }
}
