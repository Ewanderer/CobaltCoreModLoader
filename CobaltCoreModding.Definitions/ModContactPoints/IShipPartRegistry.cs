using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IShipPartRegistry
    {
        /// <summary>
        /// Attempts to register an external part.
        /// will reject if not well configured or global name already used.
        /// </summary>
        /// <param name="externalPart"></param>
        /// <returns>true if successfully registered.</returns>
        bool RegisterPart(ExternalPart externalPart);
    }
}
