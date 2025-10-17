using StarMap.Types.Pipes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StarMapLoader
{
    internal class GameProcessSupervisor
    {
        private readonly string _gamePath;
        private readonly LoaderFacade _facade;
        private readonly PipeServer _pipeServer;

        private Process? _game;


        public GameProcessSupervisor(string path, LoaderFacade facade, PipeServer pipeServer)
        {
            _gamePath = path;
            _facade = facade;
            _pipeServer = pipeServer;
        }

        public async Task<Task> TryStartGameAsync(CancellationToken cancellationToken = default)
        {
            var processExitedTsc = new TaskCompletionSource();

            var psi = new ProcessStartInfo
            {
                FileName = Path.Combine(Path.GetFullPath("./StarMap.exe")),
                Arguments = $"{_pipeServer.PipeName}",
                UseShellExecute = true, // Use the operating system shell to start the process
                CreateNoWindow = false, // Create a new window for the process
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            var pipeConntection = _pipeServer.StartAsync(default);

            if (Debugger.IsAttached)
            {
                _game = FindProcess();
            }

            if (_game is null)
            {
                _game = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                _game.Start();
            }

            await pipeConntection;

            var processStartedTcs = new TaskCompletionSource();

            void OnPrcessStarted(object? sender, EventArgs args)
            {
                processStartedTcs.TrySetResult();
            }

            _facade.OnProcessStarted += OnPrcessStarted;

            await processStartedTcs.Task;

            Console.WriteLine("Process started");

            _game.EnableRaisingEvents = true;
            _game.Exited += (s, e) =>
            {
                processExitedTsc.TrySetResult();
            };

            return processExitedTsc.Task;
        }

        private static Process? FindProcess()
        {
            string processName = "StarMap";
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.FirstOrDefault();
        }
    }
}
