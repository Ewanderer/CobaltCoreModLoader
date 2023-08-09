using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;

namespace DemoMod
{
    public class ModManifest : IModManifest
    {
        public string ModIdentifier => "EWanderer.DemoMod";

        public IEnumerable<string> Dependencies => new string[0];

        public void BootMod(IModLoaderContact contact)
        {
            //Nothing to do here lol.
        }

    
    }
}