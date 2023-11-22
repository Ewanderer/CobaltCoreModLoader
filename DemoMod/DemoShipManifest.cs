using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using DemoMod.Artifacts;
using Microsoft.Extensions.Logging;

namespace DemoMod
{
    public class DemoShipManifest : IShipPartManifest, IShipManifest, IStartershipManifest, IPartTypeManifest, IArtifactManifest, IRawStartershipManifest
    {
        private ExternalPart Cannon = new ExternalPart(
         "EWanderer.Demomod.DemoShip.Cannon",
         new Part()
         {
             active = true,
             damageModifier = PDamMod.weak,
             type = PType.cannon,
         },
         ExternalSprite.GetRaw((int)Spr.parts_cannon_freezeB));

        private ExternalPart Cockpit = new ExternalPart(
            "EWanderer.Demomod.DemoShip.Cockpit",
            new Part()
            {
                active = true,
                damageModifier = PDamMod.none,
                type = PType.cockpit,
            },
            ExternalSprite.GetRaw((int)Spr.parts_cockpit_bubble));

        private ExternalPart? CrystalStructure;
        private ExternalArtifact? demo_part_artifact;
        private ExternalArtifact? demo_ship_artifact;

        private ExternalShip? demoship;
        private ExternalArtifact? jupiter_demo_artifact;

        private ExternalPart Launcher = new ExternalPart(
         "EWanderer.Demomod.DemoShip.Launcher",
         new Part()
         {
             active = true,
             damageModifier = PDamMod.none,
             type = PType.missiles,
         },
         ExternalSprite.GetRaw((int)Spr.parts_missiles_conveyor));

        public DemoShipManifest()
        {
        }

        public static ExternalPartType? DemoPartType { get; private set; }
        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[0];
        public DirectoryInfo? GameRootFolder { get; set; }
        public ILogger? Logger { get; set; }
        public DirectoryInfo? ModRootFolder { get; set; }
        public string Name => "EWanderer.Demomod.DemoShipManifest";

        public void LoadManifest(IShipPartRegistry registry)
        {
            CrystalStructure = new ExternalPart(
            "EWanderer.Demomod.DemoShip.CrystalStructure",
            new Part()
            {
                active = true,
                damageModifier = PDamMod.none,
                type = (PType)(DemoPartType?.Id ?? throw new Exception()),
                invincible = true,
            },
            ExternalSprite.GetRaw((int)Spr.parts_crystal_1));

            registry.RegisterPart(CrystalStructure);
            registry.RegisterPart(Cockpit);
            registry.RegisterPart(Cannon);
            registry.RegisterPart(Launcher);
        }

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
                new ExternalPart[] { Cannon, CrystalStructure ?? throw new Exception(), Cockpit, CrystalStructure ?? throw new Exception(), Launcher },
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
                demoship.GlobalName, new ExternalCard[0], new ExternalArtifact[0], new Type[0], new Type[0], exclusiveArtifacts: new ExternalArtifact[] { demo_ship_artifact ?? throw new Exception() });

            starter.AddLocalisation("Hyrbid", "A crystal-tech hybrid ship. Demoship using existing assets by EWanderer");

            registry.RegisterStartership(starter);
        }

        public void LoadManifest(IPartTypeRegistry partTypeRegistry)
        {
            DemoPartType = new ExternalPartType("EWanderer.Demomod.PartTypes.DemoType", new ExternalArtifact[] { demo_part_artifact ?? throw new Exception() });
            partTypeRegistry.RegisterPartType(DemoPartType);
            DemoPartType.AddLocalisation("Chicken coop", "BAWK!");
        }

        public void LoadManifest(IArtifactRegistry registry)
        {
            {
                var spr = ExternalSprite.GetRaw((int)Spr.artifacts_ShardEnchanter);
                demo_ship_artifact = new ExternalArtifact("EWanderer.Demomod.Demoship.DemoShipArtifact", typeof(Artifacts.DemoShipArtifact), spr, new ExternalGlossary[0], null);
                demo_ship_artifact.AddLocalisation("Cup Holder", "Extra cup holders. Remove Chicken Coops somehow.");
                registry.RegisterArtifact(demo_ship_artifact);
            }

            {
                var spr = ExternalSprite.GetRaw((int)Spr.artifacts_ShardEnchanter);
                demo_part_artifact = new ExternalArtifact("EWanderer.Demomod.Demoship.DemoPartArtifact", typeof(Artifacts.DemoPartArtifact), spr, new ExternalGlossary[0], null);
                demo_part_artifact.AddLocalisation("Crystal Lining", "Shiny.");
                registry.RegisterArtifact(demo_part_artifact);
            }

            {
                var spr = ExternalSprite.GetRaw((int)Spr.artifacts_AresCannonV2);
                jupiter_demo_artifact = new ExternalArtifact("EWanderer.Demomod.Demoship.SomethingWeird", typeof(Artifacts.DemoJupiterArtifact), spr, new ExternalGlossary[0], null);
                jupiter_demo_artifact.AddLocalisation("BÄM", "BÄM.");
                registry.RegisterArtifact(jupiter_demo_artifact);
            }
        }

        public void LoadManifest(IRawStartershipRegistry registry)
        {
            registry.MakeArtifactExclusive("jupiter", typeof(DemoJupiterArtifact));
        }
    }
}