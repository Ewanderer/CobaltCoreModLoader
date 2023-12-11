using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;
using CobaltCoreModding.Definitions;

namespace CobaltCoreModding.Components.Services
{
    /// <summary>
    /// A singleton to help store any assembly and its manifest.
    /// Can also run their bootup according to dependency.
    /// </summary>
    public class ModAssemblyHandler
    {
        private static List<IAddinManifest> addinManifests = new();
        private static List<IAnimationManifest> animationManifests = new();
        private static List<IArtifactManifest> artifactManifests = new();
        private static List<IModManifest> bootManifests = new();
        private static List<ICardManifest> cardManifests = new();
        private static List<ICardOverwriteManifest> cardOverwriteManifests = new();
        private static List<ICharacterManifest> characterManifests = new();
        private static List<ICustomEventManifest> customEventManifests = new();
        private static List<IDeckManifest> deckManifests = new();
        private static List<IGlossaryManifest> glossaryManifests = new();
        private static ILoggerFactory? loggerFactory;
        private static HashSet<Assembly> modAssemblies = new();
        private static List<IPartTypeManifest> partTypeManifests = new();
        private static List<IPrelaunchManifest> prelaunchManifests = new();
        private static List<IRawShipManifest> rawShipManifests = new();
        private static List<IRawStartershipManifest> rawStartershipManifests = new();
        private static Dictionary<string, IManifest> registered_manifests = new();
        private static List<IShipManifest> shipManifests = new();
        private static List<IShipPartManifest> shippartsManifests = new();
        private static List<ISpriteManifest> spriteManifests = new();
        private static List<IStartershipManifest> startershipManifests = new();
        private static List<IStatusManifest> statusManifests = new();
        private static List<IStoryManifest> storyManifests = new();
        private static List<IApiProviderManifest> apiProviderManifests = new();
        private readonly List<AssemblyLoadContext> contexts = new List<AssemblyLoadContext>();
        private readonly Dictionary<Type, List<IManifest>> loadedManifests = new Dictionary<Type, List<IManifest>>();
        private readonly Dictionary<IManifest, PerModModLoaderContact> modLoaderContacts = new();

        public ModAssemblyHandler(ILogger<ModAssemblyHandler> logger, CobaltCoreHandler cobalt_core_handler, ILoggerFactory loggerFactory)
        {
            this.logger = logger;
            ModAssemblyHandler.loggerFactory = loggerFactory;
        }

        public static IEnumerable<IAddinManifest> AddinManifests => addinManifests.ToArray();

        public static IEnumerable<IAnimationManifest> AnimationManifests => animationManifests.ToArray();

        public static IEnumerable<IArtifactManifest> ArtifactManifests => artifactManifests.ToArray();

        public static IEnumerable<IModManifest> BootManifests => bootManifests.ToArray();

        public static IEnumerable<ICardManifest> CardManifests => cardManifests.ToArray();

        public static IEnumerable<ICardOverwriteManifest> CardOverwriteManifests => cardOverwriteManifests.ToArray();

        public static IEnumerable<ICharacterManifest> CharacterManifests => characterManifests.ToArray();

        public static IEnumerable<ICustomEventManifest> CustomEventManifests => customEventManifests.ToArray();

        public static IEnumerable<IDeckManifest> DeckManifests => deckManifests.ToArray();

        public static IEnumerable<IGlossaryManifest> GlossaryManifests => glossaryManifests.ToArray();

        public static IEnumerable<Assembly> ModAssemblies => modAssemblies.ToArray();

        public static IEnumerable<IPartTypeManifest> PartTypeManifests => partTypeManifests.ToArray();

        public static IEnumerable<IPrelaunchManifest> PrelaunchManifests => prelaunchManifests.ToArray();

        public static IEnumerable<IRawShipManifest> RawShipManifests => rawShipManifests.ToArray();

        public static IEnumerable<IRawStartershipManifest> RawStartershipManifests => rawStartershipManifests.ToArray();

        public static IEnumerable<IShipManifest> ShipManifests => shipManifests.ToArray();

        public static IEnumerable<IShipPartManifest> ShipPartsManifests => shippartsManifests.ToArray();

        public static IEnumerable<ISpriteManifest> SpriteManifests => spriteManifests.ToArray();

        public static IEnumerable<IStartershipManifest> StartershipManifests => startershipManifests.ToArray();

        public static IEnumerable<IStatusManifest> StatusManifests => statusManifests.ToArray();
        public static IEnumerable<IStoryManifest> StoryManifests => storyManifests.ToArray();

        public static IEnumerable<IApiProviderManifest> ApiProviderManifests => apiProviderManifests.ToArray();

        public Assembly CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("No Cobalt Core found.");

        public IEnumerable<IManifest> LoadedManifests => registered_manifests.Values;

        private ILogger<ModAssemblyHandler> logger { get; init; }

        public static IManifest? LookupManifest(string globalName)
        {
            registered_manifests.TryGetValue(globalName, out var manifest);
            return manifest;
        }

        public void FinalizeModLoading()
        {
            foreach (var manifest in LoadOrderly(ModAssemblyHandler.prelaunchManifests, logger))
            {
                if (!modLoaderContacts.TryGetValue(manifest, out var contact))
                {
                    contact = new PerModModLoaderContact(this, logger, manifest);
                    modLoaderContacts[manifest] = contact;
                }
                manifest.FinalizePreperations(contact);
            }
        }

        public void LoadModAssembly(FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var context = new AssemblyLoadContext(mod_file.DirectoryName ?? throw new Exception("Mod doesn't have folder???"));
                var assembly = context.LoadFromAssemblyPath(mod_file.FullName);
                context.Resolving += ModContext_Resolving;
                contexts.Add(context);
                modAssemblies.Add(assembly);
            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }

        public IEnumerable<T> LoadOrderly<T>(IEnumerable<T> manifests, ILogger? missing_logger) where T : IManifest
        {
            var remaining_manifests = manifests.ToList();
            loadedManifests.Add(typeof(T), new List<IManifest>());
            while (remaining_manifests.Count > 0)
            {
                bool hit = false;
                //turn through all remaining manifests
                for (int i = 0; i < remaining_manifests.Count; i++)
                {
                    var candidate = remaining_manifests[i];
                    bool failed = false;

                    //check each dependency
                    foreach (var dependency in candidate.Dependencies ?? Array.Empty<DependencyEntry>())
                    {
                        //Skip dependecies not yet relevant.
                        if (!loadedManifests.Any(e => dependency.DependencyType.IsAssignableTo(e.Key)))
                            continue;
                        var loaded_list = loadedManifests.First(e => dependency.DependencyType.IsAssignableTo(e.Key)).Value;
                        //check if dependeny has been loaded.
                        if (!dependency.IgnoreIfMissing && !loaded_list.Any(m => m.Name.Equals(dependency.DependencyName)))
                        {
                            failed = true;
                            break;
                        }
                    }
                    //check if failed
                    if (failed)
                        continue;
                    //store as loaded. if there are exceptions during loading that is not our concern here.
                    loadedManifests[typeof(T)].Add(candidate);
                    remaining_manifests.RemoveAt(i);
                    i--;
                    hit = true;
                    //put out for operation
                    yield return candidate;
                }
                //no more dependencies cannot be loaded.
                if (!hit)
                    break;
            }
            //any remaining entries are unresolvabe and need to be reported.
            if (missing_logger != null)
            {
                //report all levtover mods as broken
                foreach (var leftover in remaining_manifests)
                {
                    //Determine missing dependency names
                    var missing_dependency_names = leftover.Dependencies.Where(d =>
                    {
                        if (d.IgnoreIfMissing)
                            return false;
                        if (!loadedManifests.Any(e => d.DependencyType.IsAssignableTo(e.Key)))
                            return false;
                        var loaded_list = loadedManifests.First(e => d.DependencyType.IsAssignableTo(e.Key)).Value;
                        //check if dependeny has been loaded.
                        return !loaded_list.Any(m => m.Name.Equals(d.DependencyName));
                    }).Select(e => e.DependencyName);
                    var mdn_list = string.Join("\n", missing_dependency_names);
                    missing_logger.LogCritical("The Manifest '{0}' is missing the following dependencies and thus cannot be loaded:\n {1}", leftover.Name, mdn_list);
                }
            }
        }

        internal bool RegisterNewAssembly(Assembly assembly, DirectoryInfo working_directory)
        {
            if (modAssemblies.Add(assembly))
                ExtractManifestFromAssembly(assembly, working_directory);

            return true;
        }

        private IModLoaderContact ObtainModLoaderContact(IModManifest modManifest)
        {
            if (!modLoaderContacts.TryGetValue(modManifest, out var contact))
            {
                contact = new PerModModLoaderContact(this, logger, modManifest);
                modLoaderContacts[modManifest] = contact;
            }
            return contact;
        }

        public void WarumMods(object? ui_object)
        {
            //actually extractr manifests from all assemblies
            foreach (var assembly in modAssemblies)
            {
                try
                {
                    ExtractManifestFromAssembly(
                                assembly,
                              new DirectoryInfo(Path.GetDirectoryName(assembly.Location) ?? throw new Exception("No directory found for assembly"))
                            );
                }
                catch (Exception err)
                {
                    logger?.LogError(err, "Extraction of manifest from directory failed");
                }
            }

            foreach (var manifest in LoadOrderly(ModAssemblyHandler.bootManifests, logger))
            {
                if (manifest == null)
                    continue;
                manifest.BootMod(ObtainModLoaderContact(manifest));
            }

            foreach (var manifest in LoadOrderly(ModAssemblyHandler.addinManifests, logger))
            {
                if (manifest == null)
                    continue;
                manifest.ModifyLauncher(ui_object);
            }
        }

        private void ExtractManifestFromAssembly(Assembly assembly, DirectoryInfo working_directory)
        {
            var manifest_types = assembly.GetTypes().Where(e => e.IsClass && !e.IsAbstract && e.GetInterface("IManifest") != null);

            foreach (var type in manifest_types)
            {
                IManifest? spawned_manifest = null;
                try
                {
                    spawned_manifest = Activator.CreateInstance(type) as IManifest;
                }
                catch (Exception err)
                {
                    logger.LogError(err, "mod manifest type {0} not loaded with the following error.", type.Name);
                    continue;
                }
                //should not happen so we don't bother with logging
                if (spawned_manifest == null)
                    continue;
                //set working directoy
                spawned_manifest.ModRootFolder = working_directory;
                spawned_manifest.GameRootFolder = CobaltCoreHandler.CobaltCoreAppPath;
                if (loggerFactory != null)
                    spawned_manifest.Logger = loggerFactory.CreateLogger(spawned_manifest.GetType());

                //sort manifest into the various manifest lists.
                if (!registered_manifests.TryAdd(spawned_manifest.Name, spawned_manifest))
                {
                    logger.LogCritical("Collision in manifest name {0}. skipping type {1} for {2}", spawned_manifest.Name, type.Name, registered_manifests[spawned_manifest.Name].GetType().Name);
                    continue;
                }

                if (spawned_manifest is ISpriteManifest sprite_manifest)
                    spriteManifests.Add(sprite_manifest);

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
                if (spawned_manifest is IStoryManifest story_manifest)
                    storyManifests.Add(story_manifest);
                if (spawned_manifest is IModManifest boot_manifest)
                    bootManifests.Add(boot_manifest);
                if (spawned_manifest is IPrelaunchManifest prelaunchManifest)
                    prelaunchManifests.Add(prelaunchManifest);
                if (spawned_manifest is IAddinManifest addinManifest)
                    addinManifests.Add(addinManifest);
                if (spawned_manifest is IPartTypeManifest partTypeManifest)
                    partTypeManifests.Add(partTypeManifest);
                if (spawned_manifest is IApiProviderManifest apiProviderManifest)
                    apiProviderManifests.Add(apiProviderManifest);
            }
        }

        private Assembly? ModContext_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            //Mods should either cross reference another mod.
            Assembly? result = modAssemblies.Concat(new Assembly[] { CobaltCoreAssembly }).FirstOrDefault(e => e.GetName().FullName == assemblyName.FullName);
            //or an internal dependency, which we will load here to its context to avoid collision between mods.
            if (result == null)
            {
                try
                {
                    result = context.LoadFromAssemblyPath(Path.Combine(context.Name ?? throw new Exception(), (assemblyName.Name ?? throw new Exception()) + ".dll"));
                }
                catch
                {
                }
            }
            return result;
        }
    }
}