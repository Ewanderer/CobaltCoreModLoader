using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModManifests;
using System.Reflection;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// This interface serves as the contact point between any services of the
    /// mod loader and a mod and will be passed during its bootup.
    /// Examples are: Adding additional localisations, graphics and other ressources not covered by the mod loader magic (such as loading cards which is done straight from assembly)
    /// </summary>
    public interface IModLoaderContact : IManifestLookup
    {
        /// <summary>
        /// A list of all manifest. use if you want to discover all mods for some reason
        /// </summary>
        public IEnumerable<IManifest> LoadedManifests { get; }

        /// <summary>
        /// Attempts to put a new assembly into the mod loader.
        /// think a runtime emitted assembly from maybe lua scripts or something.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="working_directory">The directory which serves as the mod loader path. to ensure it can find physcial resources.</param>
        /// <returns></returns>
        public bool RegisterNewAssembly(Assembly assembly, DirectoryInfo working_directory);

        /// <summary>
        /// Request an API provided by another mod via the <see cref="IApiProviderManifest.GetApi(IManifest)"/> method.
        /// </summary>
        /// <typeparam name="TApi">An interface type matching the public members of the API provided by the other mod.</typeparam>
        /// <param name="modName">The unique mod name of the mod that provides the API.</param>
        /// <returns>A proxy for the API provided by the other mod, or `null` if the mod is not loaded or the interface does not match the API's public members.</returns>
        public TApi? GetApi<TApi>(string modName) where TApi : class;
    }
}