using CobaltCoreModding.Definitions.ModManifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IManifestLookup : ICobaltCoreLookup
    {
        IManifest LookupManifest(string globalName);
    }
}
