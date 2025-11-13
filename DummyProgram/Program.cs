namespace KSA
{
    public class Program
    {
        public static ModLibrary ModLibrary = new ModLibrary();

        private static IScreen _currentScreen = new MainScreen();

        static void Main(string[] args)
        {
            ModLibrary.LoadAll();

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

            MainScreen.Screens = null!;
        }
    }
}
