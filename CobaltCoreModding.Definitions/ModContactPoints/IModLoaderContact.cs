using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// This interface serves as the contact point between any services of the
    /// mod loader and a mod and will be passed during its bootup.
    /// Examples are: Adding additional localisations, graphics and other ressources not covered by the mod loader magic (such as loading cards which is done straight from assembly)
    /// </summary>
    public interface IModLoaderContact : ICobaltCoreContact
    {
        public IEnumerable<Assembly> LoadedModAssemblies { get; }

        public IManifest? GetManifest(string name);

        /// <summary>
        /// Attempts to put a new assembly into the mod loader.
        /// think a runtime emitted assembly from maybe lua scripts or something.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="working_directory">The directory which serves as the mod loader path. to ensure it can find physcial resources.</param>
        /// <returns></returns>
        public bool RegisterNewAssembly(IHost host_for_loggingAssembly, Assembly assembly, DirectoryInfo working_directory);
    }
}