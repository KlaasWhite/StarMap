using DummyProgram.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DummyProgram
{
    public class Mod
    {
        public required string DirectoryPath { get; init; }
        public required AssemblyName Assembly { get; init; }

        public Mod() { }
        public void PrepareSystems()
        {
        }
    }
}
