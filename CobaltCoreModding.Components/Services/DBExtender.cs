using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.OverwriteItems;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    /// <summary>
    /// This serice can be used to run patches
    /// </summary>
    public class DBExtender
    {
        private static ILogger<DBExtender>? Logger;

        private Harmony? harmony;

        public DBExtender(CobaltCoreHandler cobaltCoreHandler, ModAssemblyHandler modAssemblyHandler, ILogger<DBExtender> logger)
        {
            Logger = logger;
            card_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Card") ?? throw new Exception();
            PartialCardStatOverwrite.SprType = TypesAndEnums.SprType;
        }

        private Type card_type { get; init; }

        /// <summary>
        /// This functions hooks the extra data storage from DBExtender into the loading function of Cobalt Core DB.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void PatchDB()
        {
            //load

            harmony = new Harmony("modloader.dbextender.general");

            //patch DB

            var make_init_queue_function = TypesAndEnums.DbType.GetMethod("MakeInitQueue") ?? throw new Exception("make init queue method not found");

            var make_init_queue_postfix = typeof(DBExtender).GetMethod("MakeInitQueue_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");
            var make_init_queue_prefix = typeof(DBExtender).GetMethod("MakeInitQueue_Prefix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(make_init_queue_function, postfix: new HarmonyMethod(make_init_queue_postfix));
            harmony.Patch(make_init_queue_function, prefix: new HarmonyMethod(make_init_queue_prefix));

            //patch localisation loder

            var load_strings_for_locale_method = TypesAndEnums.DbType.GetMethod("LoadStringsForLocale", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("load_strings_for_locale_method not found");

            var load_strings_for_locale_postfix = typeof(DBExtender).GetMethod("LoadStringsForLocale_PostFix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(load_strings_for_locale_method, postfix: new HarmonyMethod(load_strings_for_locale_postfix));
        }

        /// <summary>
        /// Decks and Statuses need to be patched into DB.
        /// </summary>
        private static void InsertNewDeckAndStatus()
        {
            DeckRegistry.PatchDeckData();
            StatusRegistry.PatchStatusData();
        }

        private static void InsertNewLogicItems()
        {
            if (CobaltCoreHandler.CobaltCoreAssembly != null)
            {
                CardRegistry.PatchCardData();

                CardOverwriteRegistry.PatchLogic();

                ArtifactRegistry.PatchArtifactData();

                var midrowStuff_dict = TypesAndEnums.DbType.GetField("midrowStuff")?.GetValue(null) as Dictionary<string, Type>;
                var backgrounds_dict = TypesAndEnums.DbType.GetField("backgrounds")?.GetValue(null) as Dictionary<string, Type>;
                var maps_dict = TypesAndEnums.DbType.GetField("maps")?.GetValue(null) as Dictionary<string, Type>;
            }
        }

        private static void LoadStringsForLocale_PostFix(string locale, ref Dictionary<string, string> __result)
        {
            //Find all localisations to be added.
            CardRegistry.PatchCardLocalisation(locale, ref __result);
            CharacterRegistry.PatchCharacterLocalisation(locale, ref __result);
            GlossaryRegistry.PatchLocalisations(locale, ref __result);
            ArtifactRegistry.PatchLocalisations(locale, ref __result);
            StatusRegistry.PatchLocalisations(locale, ref __result);
            StarterShipRegistry.PatchLocalisations(locale, ref __result);
            PartTypeRegistry.PatchLocalisations(locale, ref __result);
            StoryRegistry.PatchLocalisations(locale, ref __result);
        }

        private static Queue<ValueTuple<string, Action>> MakeInitQueue_Postfix(Queue<ValueTuple<string, Action>> __result)
        {
            //extract each action and splice in custom actions.

            var patched_result = new Queue<ValueTuple<string, Action>>();
            // Cobalt core loads logic types like cards, artifacts, midrowStuff, enemies, modifiers, backgrounds, maps
            patched_result.Enqueue(__result.Dequeue());
            // We patch our own items into the DB.
            patched_result.Enqueue(new("loading modded classes", () => { InsertNewLogicItems(); }));
            // Cobalt Core loads decks and status data.
            patched_result.Enqueue(__result.Dequeue());
            //we patch out own items into db.
            patched_result.Enqueue(new("loading modded decks and statuses", () => { InsertNewDeckAndStatus(); }));
            // Cobalt Core loads story.
            patched_result.Enqueue(__result.Dequeue());
            // we apply any patches to story item.
            patched_result.Enqueue(new("patching story", () => { PatchStory(); }));
            // cobalt core loads atlas
            patched_result.Enqueue(__result.Dequeue());
            //Sprite extender needs to remove stuff from atlas for overwrite...
            patched_result.Enqueue(new("breaking atlas", () => { SpriteExtender.BreakAtlas(); }));
            // cobalt core loads fonts
            patched_result.Enqueue(__result.Dequeue());
            // nothing to do here...
            // cobalt creates extra art dictionaries for various stuff.
            patched_result.Enqueue(__result.Dequeue());
            patched_result.Enqueue(new("patch custom art into categories", () => { PatchExtraItemSprites(); }));
            //cobalt core creates card, articfact meta, event choice functions and story commands.
            patched_result.Enqueue(__result.Dequeue());
            //we do our patches on that.
            patched_result.Enqueue(new("patch card and artifact metadata, event choice functions, story commands", () => { PatchMetasAndStoryFunctions(); }));
            /*
            //cobalt core loads localisations.
            patched_result.Enqueue(__result.Dequeue());
            // We inject ourselves into the loader directly, so no extra here.
            // Cobalt Core loads platforms.
            patched_result.Enqueue(__result.Dequeue());
            // nothing to do for us here...
            */
            //at this point all things needed for raw ships is avaialbe and we load their manifests.
            patched_result.Enqueue(new("load raw ship manifests", () => { ShipRegistry.LoadRawManifests(); }));
            //cobalt core does stuff not concering us.
            while (__result.Count > 0)
                patched_result.Enqueue(__result.Dequeue());
            //load raw starting ship
            patched_result.Enqueue(new("load raw starter ships", () => { StarterShipRegistry.LoadRawManifests(); }));
            //patch starting ship
            patched_result.Enqueue(new("patch starter ships", () => { StarterShipRegistry.PatchStarterShips(); }));
            //return new action queue
            return patched_result;
        }

        private static void MakeInitQueue_Prefix(ref bool preloadSprites)
        {
            preloadSprites = true;
        }

        /// <summary>
        /// Cards, decks, icons, maps, characters, artifacts and other things need their sprite additionaly registered in db.
        /// this is done here.
        /// </summary>
        private static void PatchExtraItemSprites()
        {
            CardRegistry.PatchCardSprites();

            DeckRegistry.PatchDeckSprites();

            AnimationRegistry.PatchAnimations();

            CharacterRegistry.PatchCharacterSprites();

            GlossaryRegistry.PathIconSprites();

            ArtifactRegistry.PatchArtifactSprites();

            PartRegistry.PatchPartSprites();

            ShipRegistry.PatchChassisArt();
        }

        /// <summary>
        /// while card and artifact metas are generated by attribute, some last minute resolutions are needed.
        /// this includes card meta.deck for external decks, those ids are not know during compile time and thus cannot easily added into the attribute.
        /// </summary>
        private static void PatchMetasAndStoryFunctions()
        {
            CardRegistry.PatchCardMetas();
            ArtifactRegistry.PatchArtifactMetas();
            CardOverwriteRegistry.PatchMeta();
            StoryRegistry.PatchChoicesAndCommands();
        }

        /// <summary>
        /// Do anything to db.story.
        /// </summary>
        private static void PatchStory()
        {
            StoryRegistry.PatchStories();
        }

        /*
        private static void LoadAllSubclasses(Dictionary<string, Type>? target, Type? lookup_type)
        {
            if (target == null || lookup_type == null)
                return;

            if (ModStorage == null)
            {
                Logger?.LogCritical("LoadAllSubclasses was called without a mod assembly storage. No data provided!");
            }
            else
            {
                Logger?.LogInformation($"Loading '{lookup_type.Name}' subclasses");

                //locate all attributes.
                var qualifying_types = ModStorage.ModLookup.Select(e => e.Item1).SelectMany(asembly => asembly.GetTypes().Where(my_type => my_type.IsClass && !my_type.IsAbstract && my_type.IsSubclassOf(lookup_type)));
                //filter those with ignore attribute
                qualifying_types = qualifying_types.Where(type => Attribute.GetCustomAttribute(type, typeof(IgnoreModComponentAttribute)) == null);
                //sort into dictionary
                var counter = 0;
                foreach (var type in qualifying_types)
                {
                    if (target.TryAdd(type.Name, type))
                    {
                        Logger?.LogDebug("Loaded:" + type.Name);
                        counter++;
                    }
                    else
                        Logger?.LogWarning($"Type {type.Name} was already in dictionary. Naming conflict!");
                }
                Logger?.LogInformation($"Successfully loaded {counter} subclasses of {lookup_type.Name} from {ModStorage.ModLookup.Count()} mod assemblies.");
            }
        }
        */
    }
}