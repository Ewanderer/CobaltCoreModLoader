using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IShipLookup : ISpriteLookup, IPartLookup, ICobaltCoreLookup, IManifestLookup
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="globalName"></param>
        /// <returns>An externalship or raw ship object</returns>
        object LookupShip(string globalName);
    }
}