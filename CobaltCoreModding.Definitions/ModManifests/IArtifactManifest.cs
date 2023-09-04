using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IArtifactManifest : IManifest
    {
        /// <summary>
        /// Called by art registry when it times for add extra sprites into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IArtifactRegistry registry);
    }
}
