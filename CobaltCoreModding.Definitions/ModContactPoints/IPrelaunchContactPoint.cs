using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModManifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IPrelaunchContactPoint : IManifestLookup
    {

        /// <summary>
        /// A list of all manifest. use if you want to discover all mods for some reason
        /// </summary>
        public IEnumerable<IManifest> LoadedManifests { get; }

        /// <summary>
        /// Request an API provided by another mod via the <see cref="IApiProviderManifest.GetApi(IManifest)"/> method.
        /// </summary>
        /// <typeparam name="TApi">An interface type matching the public members of the API provided by the other mod.</typeparam>
        /// <param name="modName">The unique mod name of the mod that provides the API.</param>
        /// <returns>A proxy for the API provided by the other mod, or `null` if the mod is not loaded or the interface does not match the API's public members.</returns>
        public TApi? GetApi<TApi>(string modName) where TApi : class;
    }
}
