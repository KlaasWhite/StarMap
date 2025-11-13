/*using DummyProgram;
using DummyProgram.Screens;
using StarMap.Types.Mods;
using StarMap.Types.Proto.IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StarMap.Core
{
    internal sealed class ModManagerScreen : IScreen
    {
        private readonly ModManager _modManager;

        private List<IPCMod> _managedMods = [];
        private List<IPCMod> _unmanagedMods = [];

        private List<IPCUpdateModInformation> _changes = [];

        private (IPCMod mod, bool unmanaged, bool fromStore)? _currentMod;

        enum ManagerState
        {
            MAIN,
            MOD_INFO,
            MOD_STORE,
        }

        private ManagerState _managerState = ManagerState.MAIN;

        private List<Func<IScreen>> _actions = [];

        public ModManagerScreen(ModManager? modManager)
        {
            ArgumentNullException.ThrowIfNull(modManager);

            _modManager = modManager;

            RetrieveModInfo();
        }

        private void RetrieveModInfo()
        {
            _managedMods = [.. _modManager.GetManagedMods().Mods];
            _unmanagedMods = _modManager.GetLoadedMods().Where(loadedMod => !_managedMods.Any(managedMod => managedMod.Name == loadedMod.Name)).Select((loadedMod) => new IPCMod() { Name = loadedMod.Name}).ToList();

        }

        public string ScreenName => "Mod manager";

        public IScreen HandleInput(int input)
        {
            if (input >= _actions.Count) return new MainScreen();
            return _actions[input]();
        }

        public void Render()
        {
            _actions = [];
            switch (_managerState)
            {
                case ManagerState.MAIN:
                    {
                        RenderMain();
                        break;
                    }
                case ManagerState.MOD_INFO:
                    {
                        RenderModInfo();
                        break;
                    }
                case ManagerState.MOD_STORE:
                    {
                        RenderModStore();
                        break;
                    }
            }
        }
    
        private IScreen GoToMainMenu()
        {
            _managerState = ManagerState.MAIN;
            RetrieveModInfo();
            _changes = [];
            return new MainScreen();
        }

        private ModManagerScreen GoToModManager()
        {
            _managerState = ManagerState.MAIN;
            _currentMod = null;

            return this;
        }

        private ModManagerScreen GoToStore()
        {
            _managerState = ManagerState.MOD_STORE;
            return this;
        }

        private ModManagerScreen GoToSpecificMod(IPCMod mod, bool unmanaged, bool fromStore)
        {
            _managerState = ManagerState.MOD_INFO;
            _currentMod = (mod, unmanaged, fromStore);

            return this;
        }
    
        private void RenderMain()
        {
            var index = 0;

            Console.WriteLine("Mod manager");
            Console.WriteLine("Managed mods: ");

            foreach (var mod in _managedMods)
            {
                var localMod = mod;
                Console.WriteLine($"{index++}: {localMod.Name}:{localMod.Version}");
                _actions.Add(() => GoToSpecificMod(localMod, false, false));
            }

            Console.WriteLine("Unmanaged mods: ");
            foreach (var mod in _unmanagedMods)
            {
                var localMod = mod;
                Console.WriteLine($"{index++}: {localMod.Name}:{localMod.Version}");
                _actions.Add(() => GoToSpecificMod(mod, true, false));
            }

            Console.WriteLine($"");

            if (_changes.Count > 0)
            {
                Console.WriteLine($"Current changes");
                foreach (var modUpdate in _changes)
                {
                    if (modUpdate.BeforeVersion is IPCModVersion beforeVersion && !string.IsNullOrEmpty(beforeVersion.Version))
                        Console.WriteLine($"{modUpdate.Mod.Name}: {beforeVersion.Version} => {modUpdate.AfterVersion.Version}");
                    else
                        Console.WriteLine($"{modUpdate.Mod.Name}: {modUpdate.AfterVersion.Version}");

                }
                Console.WriteLine($"");
            }

            Console.WriteLine($"{index++}: Get other mods");
            _actions.Add(GoToStore);
            if ( _changes.Count > 0)
            {
                Console.WriteLine($"{index++}: Apply");
                _actions.Add(ApplyMods);
                Console.WriteLine($"{index++}: Revert");
                _actions.Add(GoToMainMenu);
            }
            else
            {
                Console.WriteLine($"{index++}: Return");
                _actions.Add(GoToMainMenu);
            }
            
        }
    
        private void RenderModInfo()
        {
            if (_currentMod is null) return;
            var modInformation = _modManager.GetModInformationAsync(_currentMod.Value.mod.Id).GetAwaiter().GetResult();
            if (modInformation is null)
            {
                Console.WriteLine($"Unable to retrieve information for mod: {_currentMod.Value.mod.Name}");
                Console.WriteLine($"Return");
                _actions.Add(GoToModManager);
                return;
            }

            var mod = _currentMod.Value.mod;
            var unmanaged = _currentMod.Value.unmanaged;

            Console.WriteLine($"Alter version for mod: {mod.Name}");
            if (unmanaged)
                Console.WriteLine($"Will remove the unmanaged mod and make it managed");

            var index = 0;

            foreach (var possibleVersion in modInformation.Versions)
            {
                if (!unmanaged && possibleVersion.Equals(mod.Version))
                {
                    Console.WriteLine($"* {mod.Version}");
                    continue;
                }

                var localMod = mod;
                var localversion = possibleVersion;
                var localUnmanaged = unmanaged;

                Console.WriteLine($"{index++}: {possibleVersion.Version}");
                _actions.Add(() => SetModVersion(localMod, localversion, localUnmanaged));
            }

            Console.WriteLine($"{index}: Return");
            _actions.Add(GoToModManager);
        }

        private void RenderModStore()
        {
            var availableMods = _modManager.GetAvailableModsAsync().GetAwaiter().GetResult();

            var index = 0;

            foreach (var availableMod in availableMods)
            {
                var localMod = availableMod;

                Console.WriteLine($"{index++}: {availableMod.Name} by {availableMod.Author}");
                _actions.Add(() => GoToSpecificMod(localMod, false, true));
            }

            Console.WriteLine($"{index}: Return");
            _actions.Add(GoToModManager);
        }

        private ModManagerScreen SetModVersion(IPCMod mod, IPCModVersion version, bool unmanaged)
        {
            _managerState = ManagerState.MAIN;
            IPCModVersion? previousVersion = null;

            if (unmanaged)
            {
                var removedMod = _unmanagedMods.FirstOrDefault((unmanagedMod) => string.Equals(unmanagedMod.Name, mod.Name));
                if (removedMod is not null)
                {
                    previousVersion = new IPCModVersion() { Version = removedMod.Version };
                    _unmanagedMods.Remove(removedMod);
                }
            }
            else
            {
                var oldModIndex = _managedMods.FindIndex(0, (modInfo) => modInfo.Id == mod.Id);
                if (oldModIndex >= 0)
                {
                    previousVersion = new IPCModVersion() { Version = _managedMods[oldModIndex].Version };
                }
            }

            var newModInformation = new IPCMod()
            {
                Id = mod.Id,
                Name = mod.Name,
                Author = mod.Author,
            };

            var changesIndex = _changes.FindIndex(0, (change) => change.Mod.Id == mod.Id);
            var change = new IPCUpdateModInformation()
            {
                Mod = newModInformation,
                BeforeVersion = new IPCModVersion() { Version = previousVersion?.ToString() ?? "" },
                AfterVersion = version,
            };


            if (changesIndex >= 0)
                _changes[changesIndex] = change;
            else
                _changes.Add(change);

            return GoToModManager();
        }
    
        private IScreen ApplyMods()
        {

            var message = new IPCSetManagedMods();
            message.Updates.AddRange(_changes);

            _modManager.SetModUpdates(message).GetAwaiter().GetResult();

            return new ExitScreen();
        }
    }
}
*/