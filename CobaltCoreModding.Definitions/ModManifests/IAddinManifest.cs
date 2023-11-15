using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// If a mod comes with an ui addin for the launcher ui, this is the moment to launch itself in.
    /// </summary>
    public interface IAddinManifest : IManifest
    {
        /// <summary>
        /// Function called to ask for UI fixes
        /// </summary>
        /// <param name="launcherUI">the object for ui. if null will be classic launcher otherwise it might be a winform or some other ui class, check current state of the loader and adjust</param>
        void ModifyLauncher(object? launcherUI);
    }
}
