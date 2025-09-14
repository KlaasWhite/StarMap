using DummyProgram.Screens;

namespace DummyProgram
{
    public class Program
    {
        private static IScreen _currentScreen = new MainScreen();

        static void Main(string[] args)
        {
            var library = new ModLibrary();
            library.LoadAll();

            while (true)
            {
                Console.Clear();
                _currentScreen.Render();

                var stringInput = Console.ReadLine();

                if (_currentScreen is ExitScreen)
                {
                    break;
                }

                if (string.IsNullOrEmpty(stringInput)) continue;
                if (!int.TryParse(stringInput, out var id)) continue;

                _currentScreen = _currentScreen.HandleInput(id);
            }
        }
    }
}
