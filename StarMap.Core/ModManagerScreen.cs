using DummyProgram;
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

        private List<ManagedModInformation> _managedMods = [];
        private List<AssemblyName> _unmanagedMods = [];

        private List<(string modName, Version? before, Version after)> _changes = [];

        private (string name, Version? version, bool unmanaged)? _currentMod;

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
            _unmanagedMods = _modManager.GetLoadedMods().Where(loadedMod => !_managedMods.Any(managedMod => managedMod.Name == loadedMod.Name)).ToList();
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

        private ModManagerScreen GoToSpecificMod(string modName, Version? modVersion, bool unmanaged)
        {
            _managerState = ManagerState.MOD_INFO;
            _currentMod = (modName, modVersion, unmanaged);

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
                _actions.Add(() => GoToSpecificMod(localMod.Name, Version.Parse(localMod.Version), false));
            }

            Console.WriteLine("Unmanaged mods: ");
            foreach (var mod in _unmanagedMods)
            {
                var localMod = mod;
                Console.WriteLine($"{index++}: {localMod.Name}:{localMod.Version}");
                _actions.Add(() => GoToSpecificMod(localMod.Name ?? "", localMod.Version, true));
            }

            Console.WriteLine($"");

            if (_changes.Count > 0)
            {
                Console.WriteLine($"Current changes");
                foreach (var (name, before, after) in _changes)
                {
                    if (before is not null)
                        Console.WriteLine($"{name}: {before} => {after}");
                    else
                        Console.WriteLine($"{name}: {after}");

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
            var modInformation = _modManager.GetModInformationAsync(_currentMod.Value.name).GetAwaiter().GetResult();
            if (modInformation is null)
            {
                Console.WriteLine($"Unable to retrieve information for mod: {_currentMod.Value.name}");
                Console.WriteLine($"Return");
                _actions.Add(GoToModManager);
                return;
            }

           var modName = _currentMod.Value.name;
            var version = _currentMod.Value.version;
            var unmanaged = _currentMod.Value.unmanaged;

            Console.WriteLine($"Alter version for mod: {modName}");
            if (unmanaged)
                Console.WriteLine($"Will remove the unmanaged mod and make it managed");

            var index = 0;

            foreach (var possibleVersion in modInformation.AvailableVersions)
            {
                if (!unmanaged && possibleVersion.Equals(version))
                {
                    Console.WriteLine($"* {version}");
                    continue;
                }

                var localName = modName;
                var localVersion = possibleVersion;
                var localUnmanaged = unmanaged;

                Console.WriteLine($"{index++}: {possibleVersion}");
                _actions.Add(() => SetModVersion(localName, Version.Parse(localVersion), localUnmanaged));
            }

            Console.WriteLine($"{index}: Return");
            _actions.Add(GoToModManager);
        }

        private ModManagerScreen SetModVersion(string modName, Version modVersion, bool unmanaged)
        {
            _managerState = ManagerState.MAIN;
            Version? previousVersion = null;

            if (unmanaged)
            {
                var assembly = _unmanagedMods.FirstOrDefault((assembly) => string.Equals(assembly.Name, modName));
                if (assembly is not null)
                {
                    previousVersion = assembly.Version;
                    _unmanagedMods.Remove(assembly);
                }
            }

            var newModInformation = new ManagedModInformation()
            {
                Name = modName,
                Version = modVersion.ToString(),
            };

            var index = _managedMods.FindIndex(0, (modInfo) => modInfo.Name == modName);

            if (index >= 0)
            {
                previousVersion = Version.Parse(_managedMods[index].Version);
                _managedMods[index] = newModInformation;
            }
                
            else
                _managedMods.Add(newModInformation);

            var change = (modName, previousVersion, modVersion);
            var changesIndex = _changes.FindIndex(0, (change) => change.modName == modName);

            if (changesIndex > 0)
                _changes[changesIndex] = change;
            else
                _changes.Add(change);

            return GoToModManager();
        }
    
        private IScreen ApplyMods()
        {
            var modUpdates = _changes.Select(change => new ManagedModUpdate()
            {
                Name = change.modName,
                BeforeVersion = change.before?.ToString() ?? "",
                AfterVersion = change.after.ToString(),
            });

            var message = new SetModUpdates();
            message.Updates.AddRange(modUpdates);

            _modManager.SetModUpdates(message).GetAwaiter().GetResult();

            return new ExitScreen();
        }
    }
}
