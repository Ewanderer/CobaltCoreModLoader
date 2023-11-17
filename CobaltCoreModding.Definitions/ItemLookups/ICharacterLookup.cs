using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface ICharacterLookup : ISpriteLookup, IDeckLookup, ICardLookup, IAnimationLookup, IManifestLookup, ICobaltCoreLookup
    {
        public ExternalCharacter LookupCharacter(string globalName);
    }
}
