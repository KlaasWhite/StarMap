using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyProgram.Screens
{
    public interface IScreen
    {
        string ScreenName { get; }
        void Render();
        IScreen HandleInput(int input);
    }
}
