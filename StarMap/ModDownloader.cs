using StarMap.Core.Types;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMap
{
    internal class ModDownloader
    {
        private const string ModLocation = "C:\\data\\programming\\game-modding\\KSA\\Mods";

        public bool DownloadMod(string modName, Version modVersion, string location)
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
                Name = "TestMod1",
                AvailableVersions = [Version.Parse("1.0.0.0"), Version.Parse("2.0.0.0"), Version.Parse("3.0.0.0")]
            };
            mods["TestMod2"] = new ModInformation()
            {
                Name = "TestMod2",
                AvailableVersions = [Version.Parse("1.0.0.0"), Version.Parse("2.0.0.0"), Version.Parse("3.0.0.0")]
            };

            return mods;
        }
    }
}
