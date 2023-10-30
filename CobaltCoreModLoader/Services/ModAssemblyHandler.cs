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
        private static List<IAnimationManifest> animationManifests = new();
        private static List<IArtifactManifest> artifactManifests = new();
        private static List<ICardManifest> cardManifests = new();
        private static List<ICardOverwriteManifest> cardOverwriteManifests = new();
        private static List<ICharacterManifest> characterManifests = new();
        private static List<ICustomEventManifest> customEventManifests = new();
        private static List<IDBManifest> dBManifests = new();
        private static List<IDeckManifest> deckManifests = new();
        private static List<IGlossaryManifest> glossaryManifests = new();

        private static HashSet<Assembly> modAssemblies = new();
        private static List<IModManifest> modManifests = new();
        private static List<IRawShipManifest> rawShipManifests = new();
        private static Dictionary<string, IManifest> registered_manifests = new();
        private static List<IShipManifest> shipManifests = new();
        private static List<IShipPartManifest> shippartsManifests = new();
        private static List<ISpriteManifest> spriteManifests = new();
        private static List<IStatusManifest> statusManifests = new();
        private static List<IStartershipManifest> startershipManifests = new();

        public ModAssemblyHandler(ILogger<ModAssemblyHandler> logger, CobaltCoreHandler cobalt_core_handler)
        {
            this.logger = logger;
        }

        public static IEnumerable<IAnimationManifest> AnimationManifests => animationManifests.ToArray();
        public static IEnumerable<IArtifactManifest> ArtifactManifests => artifactManifests.ToArray();
        public static IEnumerable<ICardManifest> CardManifests => cardManifests.ToArray();
        public static IEnumerable<ICardOverwriteManifest> CardOverwriteManifests => cardOverwriteManifests.ToArray();
        public static IEnumerable<ICharacterManifest> CharacterManifests => characterManifests.ToArray();
        public static IEnumerable<ICustomEventManifest> CustomEventManifests => customEventManifests.ToArray();
        public static IEnumerable<IDBManifest> DBManifests => dBManifests.ToArray();
        public static IEnumerable<IDeckManifest> DeckManifests => deckManifests.ToArray();
        public static IEnumerable<IGlossaryManifest> GlossaryManifests => glossaryManifests.ToArray();
        public static IEnumerable<Assembly> ModAssemblies => modAssemblies.ToArray();
        public static IEnumerable<IModManifest> ModManifests => modManifests.ToArray();
        public static IEnumerable<IRawShipManifest> RawShipManifests => rawShipManifests.ToArray();
        public static IEnumerable<IShipManifest> ShipManifests => shipManifests.ToArray();
        public static IEnumerable<IShipPartManifest> ShipPartsManifests => shippartsManifests.ToArray();
        public static IEnumerable<ISpriteManifest> SpriteManifests => spriteManifests.ToArray();
        public static IEnumerable<IStatusManifest> StatusManifests => statusManifests.ToArray();
        public static IEnumerable<IStartershipManifest> StartershipManifests => startershipManifests.ToArray();
        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("No Cobalt Core found.");
        IEnumerable<Assembly> IModLoaderContact.LoadedModAssemblies => ModAssemblies;
        private ILogger<ModAssemblyHandler> logger { get; init; }

        IManifest? IModLoaderContact.GetManifest(string name)
        {
            registered_manifests.TryGetValue(name, out var manifest);
            return manifest;
        }

        public void LoadModAssembly(FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var assembly = Assembly.LoadFile(mod_file.FullName);
                if (modAssemblies.Add(assembly))
                    ExtractManifestFromAssembly(assembly, mod_file.Directory ?? throw new Exception("Mod file has no parent directory!"));
            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }

        bool IModLoaderContact.RegisterNewAssembly(Assembly assembly, DirectoryInfo working_directory)
        {
            if (modAssemblies.Add(assembly))
                ExtractManifestFromAssembly(assembly, working_directory);

            return true;
        }

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

        private void ExtractManifestFromAssembly(Assembly assembly, DirectoryInfo working_directory)
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
                //set working directoy
                spanwed_manifest.ModRootFolder = working_directory;

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
                if (spanwed_manifest is ICardOverwriteManifest card_overwrite_manifest)
                    cardOverwriteManifests.Add(card_overwrite_manifest);
                if (spanwed_manifest is ICharacterManifest character_manifest)
                    characterManifests.Add(character_manifest);
                if (spanwed_manifest is IGlossaryManifest glossary_manifest)
                    glossaryManifests.Add(glossary_manifest);
                if (spanwed_manifest is IArtifactManifest artifact_manifest)
                    artifactManifests.Add(artifact_manifest);
                if (spanwed_manifest is IStatusManifest status_manifest)
                    statusManifests.Add(status_manifest);
                if (spanwed_manifest is ICustomEventManifest event_manifest)
                    customEventManifests.Add(event_manifest);
                if (spanwed_manifest is IShipPartManifest ship_part_manifest)
                    shippartsManifests.Add(ship_part_manifest);
                if(spanwed_manifest is IStartershipManifest startership_manifest)
                    startershipManifests.Add(startership_manifest);
            }
        }
    }
}