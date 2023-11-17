using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IStartershipLookup : ICardLookup, IArtifactLookup, IManifestLookup, ICobaltCoreLookup
    {
        /// <summary>
        /// Returns an externalstartership or just a raw startership object.
        /// </summary>
        /// <param name="globalName"></param>
        /// <returns></returns>
        public object LookupStarterShip(string globalName);
    }
}
