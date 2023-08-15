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

        private static Dictionary<string, IManifest> registered_manifests = new();

        private static List<IModManifest> modManifests = new();
        public static IEnumerable<IModManifest> ModManifests => modManifests.ToArray();

        private static HashSet<Assembly> modAssemblies = new();
        public static IEnumerable<Assembly> ModAssemblies => modAssemblies.ToArray();

        private static List<IDBManifest> dBManifests = new();
        public static IEnumerable<IDBManifest> DBManifests => dBManifests.ToArray();

        private static List<ISpriteManifest> spriteManifests = new();
        public static IEnumerable<ISpriteManifest> SpriteManifests => spriteManifests.ToArray();

        private static List<IAnimationManifest> animationManifests = new();
        public static IEnumerable<IAnimationManifest> AnimationManifests => animationManifests.ToArray();

        private static List<IDeckManifest> deckManifests = new();
        public static IEnumerable<IDeckManifest> DeckManifests => deckManifests.ToArray();

        private static List<ICardManifest> cardManifests = new();
        public static IEnumerable<ICardManifest> CardManifests => cardManifests.ToArray();

        private static List<ICardOverwriteManifest> cardOverwriteManifests = new();
        public static IEnumerable<ICardOverwriteManifest> CardOverwriteManifests => cardOverwriteManifests.ToArray();

        private void ExtractManifestFromAssembly(Assembly assembly)
        {
            var manifest_types = assembly.GetTypes().Where(e => e.IsClass && !e.IsAbstract && e.GetInterface("IManifest") != null);

            foreach (var type in manifest_types)
            {
                IManifest? spanwed_manifest = null;
                try
                {
                    spanwed_manifest = Activator.CreateInstance(type) as IManifest;
                }
                catch
                {
                    logger.LogError("mod manifest type {0} has no empty constructor", type.Name);
                    continue;
                }
                //should not happen so we don't bother with logging
                if (spanwed_manifest == null)
                    continue;
                //sort manifest into the various manifest lists.

                if (!registered_manifests.TryAdd(spanwed_manifest.Name, spanwed_manifest))
                {
                    logger.LogCritical("Collision in manifest name {0}. skipping type {1} for {2}", spanwed_manifest.Name, type.Name, registered_manifests[spanwed_manifest.Name].GetType().Name);
                    continue;
                }

                if (spanwed_manifest is IModManifest mod_manifest)
                    modManifests.Add(mod_manifest);
                if (spanwed_manifest is ISpriteManifest sprite_manifest)
                    spriteManifests.Add(sprite_manifest);
                if (spanwed_manifest is IDBManifest db_manifest)
                    dBManifests.Add(db_manifest);
                if (spanwed_manifest is IAnimationManifest anim_manifest)
                    animationManifests.Add(anim_manifest);
                if (spanwed_manifest is IDeckManifest deckManifest)
                    deckManifests.Add(deckManifest);
                if (spanwed_manifest is ICardManifest card_manifest)
                    cardManifests.Add(card_manifest);
                if(spanwed_manifest is ICardOverwriteManifest card_overwrite_manifest)
                    cardOverwriteManifests.Add(card_overwrite_manifest);
            }
        }

        IEnumerable<Assembly> IModLoaderContact.LoadedModAssemblies => ModAssemblies;

        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("No Cobalt Core found.");

        public void RunModLogics()
        {
            for (int i = 0; i < modManifests.Count; i++)
            {
                var manifest = modManifests[i];
                if (manifest == null)
                    continue;
                manifest.BootMod(this);
            }
        }

        public void LoadModAssembly(FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var assembly = Assembly.LoadFile(mod_file.FullName);
                if (modAssemblies.Add(assembly))
                    ExtractManifestFromAssembly(assembly);
            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }

        bool IModLoaderContact.RegisterNewAssembly(Assembly assembly)
        {
            if (modAssemblies.Add(assembly))
                ExtractManifestFromAssembly(assembly);

            return true;
        }

        IManifest? IModLoaderContact.GetManifest(string name)
        {
            registered_manifests.TryGetValue(name, out var manifest);
            return manifest;
        }
    }
}