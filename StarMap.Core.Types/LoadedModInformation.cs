using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMap.Core.Types
{
    public class LoadedModInformation
    {
        public required Version ModVersion { get; init; }
        public required string Name { get; init; }
    }
}
