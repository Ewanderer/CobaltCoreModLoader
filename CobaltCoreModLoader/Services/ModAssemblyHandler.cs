using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// A singleton to help store any assembly and its manifest.
    /// Can also run their bootup according to dependency.
    /// </summary>
    public class ModAssemblyHandler : IModLoaderContact
    {
        private ILogger<ModAssemblyHandler> logger { get; init; }

        public ModAssemblyHandler(ILogger<ModAssemblyHandler> logger, CobaltCoreHandler cobalt_core_handler)
        {
            this.logger = logger;
        }

        private static List<Tuple<Assembly, IModManifest?, IDBManifest?, ISpriteManifest?>> mod_lookup_list = new();

        public static IEnumerable<Tuple<Assembly, IModManifest?, IDBManifest?, ISpriteManifest?>> ModLookup => mod_lookup_list;

        IEnumerable<Assembly> IModLoaderContact.LoadedModAssemblies => mod_lookup_list.Select(e => e.Item1);

        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("No Cobalt Core found.");

        public void RunModLogics()
        {
            foreach (var mod in mod_lookup_list)
            {
                var manifest = mod.Item2;
                if (manifest == null)
                    continue;
                manifest.BootMod(this);
            }
        }

        private T? FindManifest<T>(Assembly assembly) where T : class
        {
            var manifest_types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces().Contains(typeof(T)));
            T? manifest = null;
            if (manifest_types.Count() == 0)
            {
                logger.LogInformation($"Mod assembly contains no {typeof(T).Name}.");
            }
            else
            {
                if (manifest_types.Count() > 1)
                {
                    logger.LogWarning($"Mod assembly contains more than one {typeof(T).Name}. Will use manifest type {manifest_types.First().Name}");
                }
                var manifest_instance = (manifest_types.First().GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]));
                if (manifest_instance == null)
                    logger.LogError($"No empty constructor found in manifest {manifest_types.First().Name}");
                manifest = manifest_instance as T;
            }
            return manifest;
        }

        public void LoadModAssembly(FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var assembly = Assembly.LoadFile(mod_file.FullName);

                //make entry
                mod_lookup_list.Add(new Tuple<Assembly, IModManifest?, IDBManifest?, ISpriteManifest?>(
                    assembly,
                    FindManifest<IModManifest>(assembly),
                    FindManifest<IDBManifest>(assembly),
                    FindManifest<ISpriteManifest>(assembly)));
            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }
    }
}