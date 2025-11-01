using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMapLoader
{
    internal class LoaderConfig
    {
        public string GameFolder { get; set; } = "C:\\data\\programming\\game-modding\\KSA\\StarMap\\StarMapLoader\\bin\\Debug\\net9.0";
        public string GamePath => Path.Combine(GameFolder, "StarMap.exe");
        public string ModPath => Path.Combine(GameFolder, "mods");
    }
}
