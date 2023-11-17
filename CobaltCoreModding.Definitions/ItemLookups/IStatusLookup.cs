using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IStatusLookup : ISpriteLookup, IManifestLookup, ICobaltCoreLookup
    {
        ExternalStatus LookupStatus(string globalName);
    }
}