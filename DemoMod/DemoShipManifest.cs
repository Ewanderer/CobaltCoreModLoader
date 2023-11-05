using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod
{
    public class DemoShipManifest : IShipPartManifest, IShipManifest, IStartershipManifest
    {
        public DirectoryInfo? ModRootFolder { get; set; }

        public string Name => "EWanderer.Demomod.DemoShipManifest";

        private ExternalPart CrystalStructure = new ExternalPart(
            "EWanderer.Demomod.DemoShip.CrystalStructure",
            new Part()
            {
                active = true,
                damageModifier = PDamMod.none,
                type = PType.wing,
                invincible = true,
            },
            ExternalSprite.GetRaw((int)Spr.parts_crystal_1));

        private ExternalPart Cockpit = new ExternalPart(
            "EWanderer.Demomod.DemoShip.Cockpit",
            new Part()
            {
                active = true,
                damageModifier = PDamMod.none,
                type = PType.cockpit,
            },
            ExternalSprite.GetRaw((int)Spr.parts_cockpit_bubble));

        private ExternalPart Cannon = new ExternalPart(
         "EWanderer.Demomod.DemoShip.Cannon",
         new Part()
         {
             active = true,
             damageModifier = PDamMod.weak,
             type = PType.cannon,
         },
         ExternalSprite.GetRaw((int)Spr.)parts_cannon_freezeB);

        private ExternalPart Launcher = new ExternalPart(
         "EWanderer.Demomod.DemoShip.Launcher",
         new Part()
         {
             active = true,
             damageModifier = PDamMod.none,
             type = PType.missiles,
         },
         ExternalSprite.GetRaw((int)Spr.parts_missiles_conveyor));

        public void LoadManifest(IShipPartRegistry registry)
        {
            registry.RegisterPart(CrystalStructure);
            registry.RegisterPart(Cockpit);
            registry.RegisterPart(Cannon);
            registry.RegisterPart(Launcher);
        }

        ExternalShip? demoship;

        public void LoadManifest(IShipRegistry shipRegistry)
        {
            demoship = new ExternalShip("EWanderer.Demomod.DemoShip.Ship",
                new Ship()
                {
                    baseDraw = 5,
                    baseEnergy = 3,
                    heatTrigger = 3,
                    heatMin = 0,
                    hull = 5,
                    hullMax = 5,
                    shieldMaxBase = 5
                },
                new ExternalPart[] { Cannon, CrystalStructure, Cockpit, CrystalStructure, Launcher },
                ExternalSprite.GetRaw((int)Spr.parts_chassis_boxy),
                null
                );
            shipRegistry.RegisterShip(demoship);
        }

        public void LoadManifest(IStartershipRegistry registry)
        {
            if (demoship == null)
                return;
            var starter = new ExternalStarterShip("EWanderer.Demomod.DemoShip.StarterShip",
                demoship.GlobalName, new ExternalCard[0],new ExternalArtifact[0], new Type[0], new Type[0]);

            starter.AddLocalisation("Hyrbid", "A crystal-tech hybrid ship. Demoship using existing assets by EWanderer");

            registry.RegisterStartership(starter);
        }
    }
}
