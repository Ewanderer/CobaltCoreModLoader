using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// Used to feed external ships
    /// </summary>
    public interface IShipManifest : IManifest
    {
        public void LoadManifest(IShipRegistry shipRegistry);
    }
}
