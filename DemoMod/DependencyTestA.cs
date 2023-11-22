using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;

namespace DemoMod
{
    public class DependencyTestA : ISpriteManifest, IArtifactManifest
    {
        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[] {
            new DependencyEntry<ISpriteManifest>("EWanderer.DemoMod.DependecyTestB")
        };

        public DirectoryInfo? GameRootFolder { get; set; }
        public ILogger? Logger { get; set; }
        public DirectoryInfo? ModRootFolder { get; set; }
        public string Name => "EWanderer.DemoMod.DependecyTestA";

        public void LoadManifest(ISpriteRegistry artRegistry)
        {
        }

        public void LoadManifest(IArtifactRegistry registry)
        {
        }
    }
}