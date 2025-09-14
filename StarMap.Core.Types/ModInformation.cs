using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMap.Core.Types
{
    public class ModInformation
    {
        public required Version[] AvailableVersions { get; init; }
        public required string Name { get; init; }
    }
}
