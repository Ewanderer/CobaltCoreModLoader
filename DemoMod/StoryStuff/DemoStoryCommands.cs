using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod.StoryStuff
{
    public static class DemoStoryCommands
    {
        public static void DemoStoryCommand(G g)
        {
            g.state.ship.hull --;
        }
    }
}
