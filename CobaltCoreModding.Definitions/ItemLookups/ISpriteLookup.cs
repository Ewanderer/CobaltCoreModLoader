using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface ISpriteLookup : ICobaltCoreLookup, IManifestLookup
    {
        /// <summary>
        /// Retrieves an external sprite.
        /// </summary>
        /// <param name="globalName">Global Name of the sprite</param>
        /// <exception cref="KeyNotFoundException">No Sprite with global name found.</exception>
        /// <returns></returns>
        public ExternalSprite LookupSprite(string globalName);
    }
}
