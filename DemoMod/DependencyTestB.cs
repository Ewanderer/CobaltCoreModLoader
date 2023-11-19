using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using CobaltCoreModding.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DemoMod
{
    public class DependencyTestB : ISpriteManifest, IPrelaunchManifest
    {
        public DirectoryInfo? ModRootFolder { get; set; }
        public DirectoryInfo? GameRootFolder { get; set; }

        public string Name => "EWanderer.DemoMod.DependecyTestB";

        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[] {
            new DependencyEntry<IArtifactManifest>("EWanderer.DemoMod.DependecyTestA")
        };

        public ILogger? Logger { get; set; }

        public void FinalizePreperations()
        {

        }

        public void LoadManifest(IArtRegistry artRegistry)
        {

        }


    }
}
