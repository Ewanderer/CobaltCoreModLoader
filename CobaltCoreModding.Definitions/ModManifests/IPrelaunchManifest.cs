using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// These manifests are called just after all other manifests have been executed but before cobalt core is acutally started.
    /// for any last minute adjustments including cross references shenanigans.
    /// </summary>
    public interface IPrelaunchManifest : IManifest
    {
        public void FinalizePreperations(IPrelaunchContactPoint prelaunchManifest);
    }
}
