using CobaltCoreModding.Definitions.ExternalResourceHelper;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using DemoMod.Cards;
using System.Reflection;

namespace DemoMod
{
    public class ModManifest : IModManifest, ISpriteManifest
    {
        public string ModIdentifier => "EWanderer.DemoMod";

        public IEnumerable<string> Dependencies => new string[0];

        public string Name => ModIdentifier;

        public void BootMod(IModLoaderContact contact)
        {
            //Nothing to do here lol.
        }

        public void LoadManifest(IArtRegistry artRegistry)
        {
            {
                var sprite = new ExternalSprite(new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\patched_cobalt_core.png"));
                artRegistry.RegisterArt(sprite, (int)Spr.cockpit_cobalt_core);
            }

            {
                var sprite = new ExternalSprite(new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\Sprites\\Shield.png"));
                EWandererDemoCard.card_sprite = (Spr)artRegistry.RegisterArt(sprite);
            }


        }
    }
}