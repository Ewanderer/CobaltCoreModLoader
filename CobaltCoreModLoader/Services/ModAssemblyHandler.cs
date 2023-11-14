using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        private static List<IRawStartershipManifest> rawStartershipManifests = new();
        private static Dictionary<string, IManifest> registered_manifests = new();
        private static List<IShipManifest> shipManifests = new();
        private static List<IShipPartManifest> shippartsManifests = new();
        private static List<ISpriteManifest> spriteManifests = new();
        private static List<IStartershipManifest> startershipManifests = new();
        private static List<IStatusManifest> statusManifests = new();

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
        public static IEnumerable<IRawStartershipManifest> RawStartershipManifests => rawStartershipManifests.ToArray();
        public static IEnumerable<IStartershipManifest> StartershipManifests => startershipManifests.ToArray();
        public static IEnumerable<IStatusManifest> StatusManifests => statusManifests.ToArray();
        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("No Cobalt Core found.");
        IEnumerable<Assembly> IModLoaderContact.LoadedModAssemblies => ModAssemblies;
        private ILogger<ModAssemblyHandler> logger { get; init; }

        IManifest? IModLoaderContact.GetManifest(string name)
        {
            registered_manifests.TryGetValue(name, out var manifest);
            return manifest;
        }

        public void LoadModAssembly(IHost host_for_logging, FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var assembly = Assembly.LoadFile(mod_file.FullName);
                if (modAssemblies.Add(assembly))
                    ExtractManifestFromAssembly(
                        host_for_logging,
                        assembly,
                        mod_file.Directory ?? throw new Exception("Mod file has no parent directory!")
                    );
            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }

        bool IModLoaderContact.RegisterNewAssembly(IHost host_for_logging, Assembly assembly, DirectoryInfo working_directory)
        {
            if (modAssemblies.Add(assembly))
                ExtractManifestFromAssembly(host_for_logging, assembly, working_directory);

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

        private void ExtractManifestFromAssembly(IHost host_for_logging, Assembly assembly, DirectoryInfo working_directory)
        {
            var manifest_types = assembly.GetTypes().Where(e => e.IsClass && !e.IsAbstract && e.GetInterface("IManifest") != null);

            foreach (var type in manifest_types)
            {
                IManifest? spawned_manifest = null;
                try
                {
                    spawned_manifest = Activator.CreateInstance(type) as IManifest;
                }
                catch
                {
                    logger.LogError("mod manifest type {0} has no empty constructor", type.Name);
                    continue;
                }
                //should not happen so we don't bother with logging
                if (spawned_manifest == null)
                    continue;
                //set working directoy
                spawned_manifest.ModRootFolder = working_directory;
                spawned_manifest.GameRootFolder = CobaltCoreHandler.CobaltCoreAppPath;
                //spawn a generic logger and fill in the generic parameters at runtime
                spawned_manifest.Logger = (ILogger)host_for_logging.Services.GetService(typeof(ILogger<>).MakeGenericType(type));


                //sort manifest into the various manifest lists.
                if (!registered_manifests.TryAdd(spawned_manifest.Name, spawned_manifest))
                {
                    logger.LogCritical("Collision in manifest name {0}. skipping type {1} for {2}", spawned_manifest.Name, type.Name, registered_manifests[spawned_manifest.Name].GetType().Name);
                    continue;
                }
                if (spawned_manifest is IModManifest mod_manifest)
                    modManifests.Add(mod_manifest);
                if (spawned_manifest is ISpriteManifest sprite_manifest)
                    spriteManifests.Add(sprite_manifest);
                if (spawned_manifest is IDBManifest db_manifest)
                    dBManifests.Add(db_manifest);
                if (spawned_manifest is IAnimationManifest anim_manifest)
                    animationManifests.Add(anim_manifest);
                if (spawned_manifest is IDeckManifest deckManifest)
                    deckManifests.Add(deckManifest);
                if (spawned_manifest is ICardManifest card_manifest)
                    cardManifests.Add(card_manifest);
                if (spawned_manifest is ICardOverwriteManifest card_overwrite_manifest)
                    cardOverwriteManifests.Add(card_overwrite_manifest);
                if (spawned_manifest is ICharacterManifest character_manifest)
                    characterManifests.Add(character_manifest);
                if (spawned_manifest is IGlossaryManifest glossary_manifest)
                    glossaryManifests.Add(glossary_manifest);
                if (spawned_manifest is IArtifactManifest artifact_manifest)
                    artifactManifests.Add(artifact_manifest);
                if (spawned_manifest is IStatusManifest status_manifest)
                    statusManifests.Add(status_manifest);
                if (spawned_manifest is ICustomEventManifest event_manifest)
                    customEventManifests.Add(event_manifest);
                if (spawned_manifest is IShipPartManifest ship_part_manifest)
                    shippartsManifests.Add(ship_part_manifest);
                if (spawned_manifest is IShipManifest shipManifest)
                    shipManifests.Add(shipManifest);
                if (spawned_manifest is IRawShipManifest rawShipManifest)
                    rawShipManifests.Add(rawShipManifest);
                if (spawned_manifest is IStartershipManifest startership_manifest)
                    startershipManifests.Add(startership_manifest);
                if (spawned_manifest is IRawStartershipManifest rawStartership_manifest)
                    rawStartershipManifests.Add(rawStartership_manifest);
            }
        }
    }
}