using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMap.Core.Types
{
    public interface IModRepository
    {
        List<LoadedModInformation> LoadedModInformation { get; }

        string[] GetPossibleMods();

        ModInformation GetModInformation(string modId);

        void SetModUpdates(IEnumerable<(string modName, Version? before, Version after)> updates);
    }
}
