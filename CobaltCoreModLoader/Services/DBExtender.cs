using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// This serice can be used to run patches
    /// </summary>
    public class DBExtender : IDbRegistry
    {


        private static ILogger<DBExtender>? Logger;

        public DBExtender(CobaltCoreHandler cobaltCoreHandler, ModAssemblyHandler modAssemblyHandler, ILogger<DBExtender> logger)
        {
            Logger = logger;
        }

        private Harmony? harmony;

        private void LoadDbManifests()
        {
            foreach (var manifest in ModAssemblyHandler.ModLookup.Select(e => e.Item3))
            {
                if (manifest == null) continue;
                manifest.LoadManifest(this);
            }
        }

        /// <summary>
        /// This functions hooks the extra data storage from DBExtender into the loading function of Cobalt Core DB.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void PatchDB()
        {

            //load 

            harmony = new Harmony("modloader.dbextender");

            //patch DB


            var make_init_queue_function = db_type.GetMethod("MakeInitQueue") ?? throw new Exception("make init queue method not found");

            var make_init_queue_postfix = typeof(DBExtender).GetMethod("MakeInitQueue_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(make_init_queue_function, postfix: new HarmonyMethod(make_init_queue_postfix));

            //patch localisation loder

            var load_strings_for_locale_method = db_type.GetMethod("LoadStringsForLocale", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("load_strings_for_locale_method not found");

            var load_strings_for_locale_postfix = typeof(DBExtender).GetMethod("LoadStringsForLocale_PostFix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(load_strings_for_locale_method, postfix: new HarmonyMethod(load_strings_for_locale_postfix));

            LoadDbManifests();

        }



        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new NullReferenceException();

        private static void LoadStringsForLocale_PostFix(string locale, ref Dictionary<string, string> __result)
        {
            //Find all localisations to be added.
            foreach (var card in registered_cards.Values)
            {
                if (card == null) continue;

                string? text = card.GetLocalisation(locale);
                if (text == null)
                    continue;
                //make card and get value
                var inst = Activator.CreateInstance(card.CardType);
                if (inst == null)
                {
                    Logger?.LogCritical($"Cannot spawn instance of card {card.CardType.Name} because no empty constructor found.");
                    continue;
                }

                var key = "card." + card.CardType.Name + ".name";
                if (!__result.TryAdd(key, text))
                    Logger?.LogCritical($"Cannot add {key} to localisations, since already know. skipping...");

            }

        }

        static Type? __db_type = null;
        static Type db_type
        {
            get
            {
                if (__db_type != null) return __db_type;

                return __db_type = (CobaltCoreHandler.CobaltCoreAssembly?.GetType("DB") ?? throw new Exception("DB not found."));
            }
        }

        private static Queue<Action> MakeInitQueue_Postfix(Queue<Action> __result)
        {
            //extract each action and splice in custom actions.

            var patched_result = new Queue<Action>();
            // Cobalt core loads logic types like cards, artifacts, midrowStuff, enemies, modifiers, backgrounds, maps
            patched_result.Enqueue(__result.Dequeue());
            // We patch our own items into the DB.
            patched_result.Enqueue(() => { InsertNewLogicItems(); });
            // Cobalt Core loads decks and status data.
            patched_result.Enqueue(__result.Dequeue());
            //we patch out own items into db.
            patched_result.Enqueue(() => { InsertNewDeckAndStatus(); });
            //cobalt core loads localisations.
            patched_result.Enqueue(__result.Dequeue());
            // We inject ourselves into the loader directly, so no extra here.
            // Cobalt Core loads platforms.
            patched_result.Enqueue(__result.Dequeue());
            // nothing to do for us here...
            // Cobalt Core loads story.
            patched_result.Enqueue(__result.Dequeue());
            // we apply any patches to story item.
            patched_result.Enqueue(() => { PatchStory(); });
            // cobalt core loads atlas
            patched_result.Enqueue(__result.Dequeue());
            //Sprite extender needs to remove stuff from atlas for overwrite...
            patched_result.Enqueue(() => { SpriteExtender.BreakAtlas(); });
            // cobalt core loads fonts
            patched_result.Enqueue(__result.Dequeue());
            // nothing to do here...
            // cobalt creates extra art dictionaries for various stuff.
            patched_result.Enqueue(__result.Dequeue());
            patched_result.Enqueue(() => { PatchExtraItemSprites(); });
            //cobalt core creates card, articfact meta, event choice functions and story commands.
            patched_result.Enqueue(__result.Dequeue());
            //we do our patches on that.
            patched_result.Enqueue(() => { PatchMetasAndStoryFunctions(); });
            //cobalt core does stuff not concering us.
            while (__result.Count > 0)
                patched_result.Enqueue(__result.Dequeue());
            //return new action queue
            return patched_result;
        }

        /// <summary>
        /// while card and artifact metas are generated by attribute, some last minute resolutions are needed.
        /// this includes card meta.deck for external decks, those ids are not know during compile time and thus cannot easily added into the attribute.
        /// </summary>
        private static void PatchMetasAndStoryFunctions()
        {
            IDictionary card_meta_dictionary = db_type.GetField("cardMetas")?.GetValue(null) as IDictionary ?? throw new Exception("card art dictionary not found");

            foreach (var card in registered_cards.Values.Where(e => e != null && e.ActualDeck != null))
            {
                if (card == null) continue;

            }

        }

        /// <summary>
        /// Cards, decks, icons, maps, characters, artifacts and other things need their sprite additionaly registered in db.
        /// this is done here.
        /// </summary>
        private static void PatchExtraItemSprites()
        {
            IDictionary card_art_dictionary = db_type.GetField("cardArt", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new Exception("card art dictionary not found");

            foreach (var card in registered_cards.Values)
            {
                if (card == null) continue;
                var spr_val = SpriteExtender.IntToSpr(card.CardArt.Id);
                if (spr_val == null)
                {
                    Logger?.LogError($"CardArt {card.GlobalName} wasn't resolved.");
                    continue;
                }
                if (card_art_dictionary.Contains(card.CardType.Name))
                {
                    Logger?.LogCritical($"Collision of type card {card.CardType.Name} detected. skipping");
                    continue;
                }
                else
                {
                    card_art_dictionary.Add(card.CardType.Name, spr_val);
                }
            }

        }

        /// <summary>
        /// Do anything to db.story.
        /// </summary>
        private static void PatchStory()
        {

        }

        /// <summary>
        /// Decks and Statuses need to be patched into DB.
        /// </summary>
        private static void InsertNewDeckAndStatus()
        {

        }

        private static void InsertNewLogicItems()
        {
            if (CobaltCoreHandler.CobaltCoreAssembly != null)
            {

                /*
                //patch into all fields
                var db_type = CobaltCoreHandler.CobaltCoreAssembly.GetType("DB") ?? throw new Exception();

                var card_dict = db_type.GetField("cards")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(card_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("Card"));

                var enemies_dict = db_type.GetField("enemies")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(enemies_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("AI"));

                var modifiers_dict = db_type.GetField("modifiers")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(modifiers_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("FightModifier"));

                var artifacts_dict = db_type.GetField("artifacts")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(artifacts_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("Artifact"));

                var midrowStuff_dict = db_type.GetField("midrowStuff")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(midrowStuff_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("StuffBase"));

                var backgrounds_dict = db_type.GetField("backgrounds")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(backgrounds_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("BG"));

                var maps_dict = db_type.GetField("maps")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(maps_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("MapBase"));
                */

                {
                    var card_dict = db_type.GetField("cards")?.GetValue(null) as Dictionary<string, Type>;
                    if (card_dict != null)
                    {
                        foreach (var card in registered_cards.Values)
                        {
                            if (card == null) continue;

                            if (!card_dict.TryAdd(card.CardType.Name, card.CardType))
                            {
                                Logger?.LogWarning($"ExternalCard {card.GlobalName} couldn't be added into DB.card dictionary because of collision in type name.");
                            }
                        }
                    }
                }


                var midrowStuff_dict = db_type.GetField("midrowStuff")?.GetValue(null) as Dictionary<string, Type>;
                var backgrounds_dict = db_type.GetField("backgrounds")?.GetValue(null) as Dictionary<string, Type>;
                var maps_dict = db_type.GetField("maps")?.GetValue(null) as Dictionary<string, Type>;
            }
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

        private static Dictionary<string, ExternalCard> registered_cards = new Dictionary<string, ExternalCard>();

        bool IDbRegistry.RegisterArtifact(ExternalArtifact artifact)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterCard(ExternalCard card)
        {
            if (registered_cards.ContainsKey(card.GlobalName))
            {
                Logger?.LogCritical($"Tried to register a card with the global name {card.GlobalName} twice! Rejecting second card");
                return false;
            }

            //check if card is valid

            if (!card.ValidReferences())
            {
                Logger?.LogCritical($"Card with gloabl name {card.GlobalName} has unregistered assets.");
                return false;
            }

            //add card into registy
            registered_cards.Add(card.GlobalName, card);
            return true;
        }

        bool IDbRegistry.RegisterCharacter(ExternalCharacter character)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterDeck(ExternalDeck deck)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterEnemy(ExternalEnemy enemy)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterMidrowItem(ExternalMidrowItem midrowItem)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterModifier(ExternalModifier modifier)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterSpaceThing(ExternalSpaceThing spaceThing)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterStatus(ExternalStatus status)
        {
            throw new NotImplementedException();
        }

        ExternalSprite? IDbRegistry.GetModSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName);
        }

        private Type? __spr_enum_type;

        private Type spr_enum_type
        {
            get
            {
                if (__spr_enum_type != null)
                    return __spr_enum_type;
                return __spr_enum_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Spr") ?? throw new Exception("Spr type not found");
            }
        }

        ExternalSprite? IDbRegistry.GetOriginalSprite(int sprVal)
        {
            //check if sprval is valid
            if (Enum.IsDefined(spr_enum_type, sprVal))
            {
                Logger?.LogWarning("Unkown spr from cobalt core game requested.");
                return null;
            }
            return ExternalSprite.GetRaw(sprVal);
        }
    }
}