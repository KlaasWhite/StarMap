using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSA
{
    public interface IScreen
    {
        string ScreenName { get; }
        void Render();
        IScreen HandleInput(int input);
    }
}
