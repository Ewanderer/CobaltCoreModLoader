using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod
{
    public class DependencyTestA : ISpriteManifest, IArtifactManifest
    {
        public DirectoryInfo? ModRootFolder { get; set; }
        public DirectoryInfo? GameRootFolder { get; set; }

        public string Name => "EWanderer.DemoMod.DependecyTestA";

        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[] {
            new DependencyEntry<ISpriteManifest>("EWanderer.DemoMod.DependecyTestB")
        };

        public ILogger? Logger { get; set ; }

        public void LoadManifest(ISpriteRegistry artRegistry)
        {

        }

        public void LoadManifest(IArtifactRegistry registry)
        {

        }
    }
}
