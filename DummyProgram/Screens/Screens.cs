using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSA
{
    public class MainScreen : IScreen
    {
        public static IScreen[] Screens { get; set; } = [new GameScreen(), new ExitScreen()];

        public string ScreenName => "Main";

        public IScreen HandleInput(int input)
        {
            if (input >= Screens.Length) return this;

            return Screens[input];
        }

        public void Render()
        {
            for (int i = 0; i < Screens.Length; i++)
            {
                Console.WriteLine($"{i}: {Screens[i]}");
            }
        }
    }

    public class ExitScreen : IScreen
    {
        public string ScreenName => "Quit";

        public IScreen HandleInput(int input)
        {
            return this;
        }

        public void Render()
        {
            Console.WriteLine("Quiting game");
        }
    }

    public class GameScreen : IScreen
    {
        public string ScreenName => "Game";

        public IScreen HandleInput(int input)
        {
            if (input == 1) return new MainScreen();
            Console.Clear();
            DoSomething();

            return this;
        }

        public void DoSomething()
        {
            Program.ModLibrary.Mods.ForEach((mod) => mod.DoSomething());
            Console.WriteLine("GameScreen.DoSomething");
            Console.ReadLine();
        }

        public void Render()
        {
            Console.WriteLine($"0: Do something");
            Console.WriteLine($"1: Main menu");
        }
    }
}
