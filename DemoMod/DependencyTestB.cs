using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;

namespace DemoMod
{
    public class DependencyTestB : ISpriteManifest, IPrelaunchManifest
    {
        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[] {
            new DependencyEntry<IArtifactManifest>("EWanderer.DemoMod.DependecyTestA")
        };

        public DirectoryInfo? GameRootFolder { get; set; }
        public ILogger? Logger { get; set; }
        public DirectoryInfo? ModRootFolder { get; set; }
        public string Name => "EWanderer.DemoMod.DependecyTestB";

        public void FinalizePreperations(IPrelaunchContactPoint prelaunchManifest)
        {
        }

        public void LoadManifest(ISpriteRegistry artRegistry)
        {
        }
    }
}