using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModManifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface ICardLookup : ISpriteLookup, IDeckLookup, IManifestLookup, ICobaltCoreLookup
    {
        public ExternalCard LookupCard(string globalName);
    }
}
