using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// This manifest's load function is called after all manifest have been loaded from dll file but before anything else happens.
    /// Can be used by any mod that needs to do some booting routine.
    /// </summary>
    public interface IModManifest : IManifest
    {

        void BootMod(IModLoaderContact contact);
    }
}
