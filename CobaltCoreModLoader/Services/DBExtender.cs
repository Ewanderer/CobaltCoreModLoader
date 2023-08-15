using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.OverwriteItems;
using CobaltCoreModLoader.Utils;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Data;
using System.Reflection;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// This serice can be used to run patches
    /// </summary>
    public class DBExtender : IDbRegistry
    {
        private const int deck_counter_start = 10000;
        private static Type? __db_type = null;

        /// <summary>
        /// GlobalName -> Card Meta
        /// </summary>
        private static Dictionary<string, CardMetaOverwrite> card_meta_lookup = new Dictionary<string, CardMetaOverwrite>();

        /// <summary>
        /// CardType -> Card Meta
        /// </summary>
        private static Dictionary<string, CardMetaOverwrite> card_meta_overwrites = new Dictionary<string, CardMetaOverwrite>();

        private static Dictionary<string, ExternalCard> card_overwrites = new Dictionary<string, ExternalCard>();
        private static Dictionary<string, CardStatOverwrite> card_stat_lookup = new Dictionary<string, CardStatOverwrite>();
        private static Dictionary<Type, HashSet<CardStatOverwrite>> card_stat_overwrites = new Dictionary<Type, HashSet<CardStatOverwrite>>();
        private static int deck_counter = deck_counter_start;
        private static Dictionary<string, ExternalDeck> deck_lookup = new Dictionary<string, ExternalDeck>();
        private static ILogger<DBExtender>? Logger;

        private static Dictionary<string, ExternalAnimation> registered_animations = new Dictionary<string, ExternalAnimation>();
        private static Dictionary<string, ExternalCard> registered_cards = new Dictionary<string, ExternalCard>();
        private static Dictionary<string, ExternalCharacter> registered_characters = new Dictionary<string, ExternalCharacter>();
        private static Dictionary<int, ExternalDeck> registered_decks = new Dictionary<int, ExternalDeck>();
        private Harmony? harmony;

        public DBExtender(CobaltCoreHandler cobaltCoreHandler, ModAssemblyHandler modAssemblyHandler, ILogger<DBExtender> logger)
        {
            Logger = logger;
            card_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("Card") ?? throw new Exception();
            PartialCardStatOverwrite.SprType = TypesAndEnums.SprType;
        }

        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new NullReferenceException();

        private static Type db_type
        {
            get
            {
                if (__db_type != null) return __db_type;

                return __db_type = (CobaltCoreHandler.CobaltCoreAssembly?.GetType("DB") ?? throw new Exception("DB not found."));
            }
        }

        private Type card_type { get; init; }

        ExternalSprite? IDbRegistry.GetModSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName);
        }

        ExternalSprite IDbRegistry.GetOriginalSprite(int sprVal)
        {
            return TypesAndEnums.GetOriginalSprite(sprVal);
        }

        /// <summary>
        /// This functions hooks the extra data storage from DBExtender into the loading function of Cobalt Core DB.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void PatchDB()
        {
            //load

            harmony = new Harmony("modloader.dbextender.general");

            //patch DB

            var make_init_queue_function = db_type.GetMethod("MakeInitQueue") ?? throw new Exception("make init queue method not found");

            var make_init_queue_postfix = typeof(DBExtender).GetMethod("MakeInitQueue_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(make_init_queue_function, postfix: new HarmonyMethod(make_init_queue_postfix));

            //patch localisation loder

            var load_strings_for_locale_method = db_type.GetMethod("LoadStringsForLocale", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("load_strings_for_locale_method not found");

            var load_strings_for_locale_postfix = typeof(DBExtender).GetMethod("LoadStringsForLocale_PostFix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(load_strings_for_locale_method, postfix: new HarmonyMethod(load_strings_for_locale_postfix));

            //patch unlocked characters in the game

            var get_unlocked_characters_method = TypesAndEnums.StoryVarsType.GetMethod("GetUnlockedChars", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("GetUnlockedChars method not found.");
            var get_unlocked_characters_postfix = typeof(DBExtender).GetMethod("GetUnlockedCharactersPostfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("GetUnlockedCharactersPostfix not found");

            harmony.Patch(get_unlocked_characters_method, postfix: new HarmonyMethod(get_unlocked_characters_postfix));

            LoadDbManifests();

            PatchEnumExtension();

            PatchNewRunOptions();

            PatchStarterSets();
        }

        bool IDbRegistry.RegisterAnimation(ExternalAnimation animation)
        {
            //
            if (string.IsNullOrEmpty(animation.GlobalName))
            {
                Logger?.LogWarning("animation without global name");
                return false;
            }

            if (string.IsNullOrEmpty(animation.Tag))
            {
                Logger?.LogWarning("ExternalAnimation {0} has not tag value", animation.GlobalName);
                return false;
            }

            if (!registered_animations.TryAdd(animation.GlobalName, animation))
            {
                Logger?.LogWarning("ExternalAnimation {0} already has an entry in registry. possible global name collision!", animation.GlobalName);
                return false;
            }

            return true;
        }

        bool IDbRegistry.RegisterArtifact(ExternalArtifact artifact)
        {
            throw new NotImplementedException();
        }

        bool IDbRegistry.RegisterCard(ExternalCard card, string? overwrite)
        {
            if (!card.CardType.IsSubclassOf(card_type))
            {
                Logger?.LogCritical("ExternalCard {0} isn't a card but type {1}", card.GlobalName, card.CardType.Name);
                return false;
            }

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

            //mark overwrite
            if (overwrite != null)
            {
                if (card_overwrites.TryAdd(overwrite, card))
                {
                    Logger?.LogInformation("Collision in overwrite of type {0}. Replacing {1} with {2}", overwrite, card_overwrites[overwrite].GlobalName, card.GlobalName);
                    card_overwrites[overwrite] = card;
                }
            }
            else
            {
                //add card into registy
                registered_cards.Add(card.GlobalName, card);
            }

            return true;
        }

        bool IDbRegistry.RegisterCardMetaOverwrite(CardMetaOverwrite cardMeta, string card_key)
        {
            if (string.IsNullOrEmpty(cardMeta.GlobalName))
            {
                Logger?.LogWarning("Attempted to register card meta without global name. rejected.");
                return false;
            }
            if (!card_meta_lookup.TryAdd(cardMeta.GlobalName, cardMeta))
            {
                Logger?.LogWarning("ExternalCardMeta {0} cannot be added, because global name already registered", cardMeta.GlobalName);
                return false;
            }
            if (!card_meta_overwrites.TryAdd(card_key, cardMeta))
            {
                Logger?.LogWarning("ExternalCardMeta {0} will overwrite anoter overwrite meta {1}", cardMeta.GlobalName, card_meta_lookup[card_key].GlobalName);
                card_meta_lookup[card_key] = cardMeta;
            }
            return true;
        }

        bool IDbRegistry.RegisterCardStatOverwrite(CardStatOverwrite statOverwrite)
        {
            if (string.IsNullOrWhiteSpace(statOverwrite.GlobalName))
            {
                Logger?.LogWarning("Attempted to register card stat overwrite without global name. rejected.");
                return false;
            }
            if (!statOverwrite.CardType.IsSubclassOf(TypesAndEnums.CardType))
            {
                Logger?.LogWarning("Card overwrite {0} doesn't target a class of the card type.", statOverwrite.GlobalName);
                return false;
            }
            if (!card_stat_lookup.TryAdd(statOverwrite.GlobalName, statOverwrite))
            {
                Logger?.LogWarning("Collission in card overwrite global name {0} ", statOverwrite.GlobalName);
                return false;
            }
            if (!card_stat_overwrites.TryAdd(statOverwrite.CardType, new HashSet<CardStatOverwrite> { statOverwrite }))
                card_stat_overwrites[statOverwrite.CardType].Add(statOverwrite);
            return true;
        }

        bool IDbRegistry.RegisterCharacter(ExternalCharacter character)
        {
            if (string.IsNullOrEmpty(character.GlobalName))
            {
                return false;
            }

            if (!registered_characters.TryAdd(character.GlobalName, character))
            {
                return false;
            }

            return true;
        }

        bool IDbRegistry.RegisterDeck(ExternalDeck deck, int? overwrite)
        {
            if (deck.Id != null)
            {
                Logger?.LogWarning("Deck already has an ID");
                return false;
            }
            if (string.IsNullOrWhiteSpace(deck.GlobalName))
            {
                Logger?.LogWarning("Deck with empty global name");
                return false;
            }
            if (deck_lookup.ContainsKey(deck.GlobalName))
            {
                Logger?.LogWarning("Deck GlobalName collision:" + deck.GlobalName);
                return false;
            }

            if (overwrite == null)
            {
                deck_lookup.Add(deck.GlobalName, deck);
                registered_decks.Add(deck_counter, deck);
                deck.Id = deck_counter;
                deck_counter++;
                return true;
            }
            else
            {
                if (overwrite < 0 || deck_counter_start <= overwrite)
                {
                    Logger?.LogError("Attempted overwrite of non card value");
                    return false;
                }

                deck_lookup.Add(deck.GlobalName, deck);
                if (!registered_decks.TryAdd(overwrite.Value, deck))
                {
                    Logger?.LogWarning("Collision Deck Overwrite between {0} and {1} on value {2}. {1} will be used unless other overwrite happens.",
                        registered_decks[overwrite.Value].GlobalName, deck.GlobalName, overwrite.Value, deck.GlobalName);
                    registered_decks[overwrite.Value] = deck;
                }
                deck.Id = overwrite.Value;
                return true;
            }
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

        private static void GetUnlockedCharactersPostfix(ref object __result)
        {
            var hash_type = typeof(HashSet<>).MakeGenericType(TypesAndEnums.DeckType);
            var add_method = hash_type.GetMethod("Add") ?? throw new Exception("HashSet<Deck> doesn't have Add method.");

            foreach (var character in registered_characters.Values)
            {
                var deck_val = TypesAndEnums.IntToDeck(character.Deck.Id);
                if (deck_val == null)
                {
                    Logger?.LogError("ExternalCharacter {0} unlocked patch failed because missign deck id {1}", character.GlobalName, character.Deck.Id?.ToString() ?? "NULL");
                    continue;
                }
                add_method.Invoke(__result, new object[] { deck_val });
            }
        }

        /// <summary>
        /// Decks and Statuses need to be patched into DB.
        /// </summary>
        private static void InsertNewDeckAndStatus()
        {
            IDictionary deck_dict = db_type.GetField("decks")?.GetValue(null) as IDictionary ?? throw new Exception("decks dictinoary not found");

            var color_field = TypesAndEnums.DeckDefType.GetField("color") ?? throw new Exception("DeckDef.color not found");
            var title_color_field = TypesAndEnums.DeckDefType.GetField("titleColor") ?? throw new Exception("DeckDef.titleColor not found");

            foreach (var deck in registered_decks.Values)
            {
                var deck_val = TypesAndEnums.IntToDeck(deck.Id);
                if (deck_val == null)
                {
                    Logger?.LogError("externaldeck {0} id {1} not converted into deck enum. skipping...", deck.GlobalName, deck.Id?.ToString() ?? "NULL");
                    continue;
                }

                //create colors
                var deck_color = Activator.CreateInstance(TypesAndEnums.CobaltColorType, (UInt32)deck.DeckColor.ToArgb());

                if (deck_color == null)
                {
                    Logger?.LogWarning("ExternalDeck {0} Color couldn't be initalized", deck.GlobalName);
                    continue;
                }


                var title_color = Activator.CreateInstance(TypesAndEnums.CobaltColorType, (UInt32)deck.TitleColor.ToArgb());
                if (title_color == null)
                {
                    Logger?.LogWarning("ExternalDeck {0} Title Color couldn't be initalized", deck.GlobalName);
                    continue;
                }

                object? deck_registry;
                //check if overwrite or new val
                if (deck_dict.Contains(deck_val))
                {
                    deck_registry = deck_dict[deck_val];
                }
                else
                {
                    //new entrie
                    //spawn instance.
                    deck_registry = Activator.CreateInstance(TypesAndEnums.DeckDefType);
                    if (deck_registry == null)
                    {
                        continue;
                    }
                    deck_dict.Add(deck_val, deck_registry);

                }

                if (deck_registry == null)
                    continue;
                //assign colors
                color_field.SetValue(deck_registry, deck_color);
                title_color_field.SetValue(deck_registry, title_color);
                //store reference in deck
                deck.DeckDefReference = deck_registry;
            }
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

                    //Overwrite card dictionary.

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

                        foreach (var overwrite in card_overwrites)
                        {
                            if (!card_dict.ContainsKey(overwrite.Key))
                            {
                                Logger?.LogWarning("ExternalCard {0} overwrite of {1} failed because no key found.", overwrite.Value.GlobalName, overwrite.Key);
                                continue;
                            }
                            else
                            {
                                card_dict[overwrite.Key] = overwrite.Value.CardType;
                            }
                        }
                    }
                }
                //inject card stat overwrites
                {
                    var base_stat_postfix = typeof(DBExtender).GetMethod("OverwriteCardStats", BindingFlags.Static | BindingFlags.NonPublic);
                    var harmony = new Harmony("modloader.dbextender.card_stat_overwrites");
                    foreach (var overwrite in card_stat_overwrites)
                    {
                        //These are virtual and thus need to be retrieved everytime, but also improves ingame performance.
                        var base_stat_method = overwrite.Key.GetMethod("GetData") ?? throw new Exception();
                        harmony.Patch(base_stat_method, postfix: new HarmonyMethod(base_stat_postfix));
                    }
                }

                var midrowStuff_dict = db_type.GetField("midrowStuff")?.GetValue(null) as Dictionary<string, Type>;
                var backgrounds_dict = db_type.GetField("backgrounds")?.GetValue(null) as Dictionary<string, Type>;
                var maps_dict = db_type.GetField("maps")?.GetValue(null) as Dictionary<string, Type>;
            }
        }

        private static void LoadStringsForLocale_PostFix(string locale, ref Dictionary<string, string> __result)
        {
            //Find all localisations to be added.
            foreach (var card in registered_cards.Values.Concat(card_overwrites.Values))
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
            //character names + descriptuions
            foreach (var character in registered_characters.Values)
            {
                if (string.IsNullOrWhiteSpace(character.GlobalName))
                    continue;
                string? text = character.GetCharacterName(locale);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var key = "char." + character.Deck.GlobalName;
                    if (!__result.TryAdd(key, text))
                        Logger?.LogCritical("Cannot add {0} to localisations, since already know. skipping", key);
                }
                string? desc = character.GetDesc(locale);
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    var deck_val = TypesAndEnums.IntToSpr(character.Deck.Id);
                    if (deck_val != null)
                    {
                        var key = "char." + deck_val.ToString() + ".desc";
                        if (!__result.TryAdd(key, desc))
                            Logger?.LogCritical("Cannot add {0} to localisations, since already know. skipping", key);
                    }
                }
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

        private static void OverwriteCardStats(ref object __result, object __instance, object state)
        {
            var lookup_type = __instance.GetType();
            if (card_stat_overwrites.TryGetValue(lookup_type, out var overwrite_passes))
            {
                foreach (var pass in overwrite_passes)
                {
                    if (pass is ActiveCardStatOverwrite active_overwrite)
                    {
                        var new_stats = active_overwrite.GetStatFunction(state, __result);
                        __result = new_stats;
                    }
                    else if (pass is PartialCardStatOverwrite partial_overwrite)
                    {
                        partial_overwrite.ApplyOverwrite(ref __result);
                    }
                    else
                    {
                        Logger?.LogCritical("Unkown card stat overwrite {0} type {1}. skipping", pass.GlobalName, pass.GetType().Name);
                    }
                }
            }
        }

        /// <summary>
        /// Cards, decks, icons, maps, characters, artifacts and other things need their sprite additionaly registered in db.
        /// this is done here.
        /// </summary>
        private static void PatchExtraItemSprites()
        {
            //cards
            IDictionary card_art_dictionary = db_type.GetField("cardArt", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new Exception("card art dictionary not found");

            foreach (var card in registered_cards.Values)
            {
                if (card == null) continue;
                var spr_val = TypesAndEnums.IntToSpr(card.CardArt.Id);
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

            foreach (var overwrite in card_overwrites)
            {
                if (!card_art_dictionary.Contains(overwrite.Key))
                {
                    Logger?.LogWarning("Overwrite {0} failed because no such entry known in sprites.", overwrite.Key);
                    continue;
                }
                var spr_val = TypesAndEnums.IntToSpr(overwrite.Value.CardArt.Id);
                if (spr_val == null)
                {
                    Logger?.LogError("Unexpected null id in cardart sprite. For External card {0}", overwrite.Value.GlobalName);
                    continue;
                }
                card_art_dictionary[overwrite.Key] = spr_val;
            }

            //decks
            IDictionary deck_borders_dict = db_type.GetField("deckBorders")?.GetValue(null) as IDictionary ?? throw new Exception();
            IDictionary deck_borders_over_dict = db_type.GetField("deckBordersOver")?.GetValue(null) as IDictionary ?? throw new Exception();
            IDictionary card_art_deck_default_dict = db_type.GetField("cardArtDeckDefault")?.GetValue(null) as IDictionary ?? throw new Exception();
            foreach (var deck in registered_decks.Values)
            {
                if (deck.Id == null)
                {
                    Logger?.LogError("ExternalDeck {0} has no idea despite being registered", deck.GlobalName);
                    continue;
                }
                var deck_key = TypesAndEnums.IntToDeck(deck.Id);
                if (deck_key == null)
                {
                    Logger?.LogError("ExternalDeck {0} id couldn't be converted into deck enum", deck.GlobalName);
                    continue;
                }

                var border_spr = TypesAndEnums.IntToSpr(deck.BorderSprite.Id);
                if (border_spr == null)
                {
                    Logger?.LogError("ExternalDeck {0} border sprite id {1} couldn't be converted into spr enum", deck.GlobalName, deck.BorderSprite.Id?.ToString() ?? "NULL");
                    continue;
                }

                if (deck_borders_dict.Contains(deck_key))
                {
                    deck_borders_dict[deck_key] = border_spr;
                }
                else
                {
                    deck_borders_dict.Add(deck_key, border_spr);
                }

                var border_over_spr = TypesAndEnums.IntToSpr(deck.BordersOverSprite?.Id);
                if (deck.BordersOverSprite != null && border_over_spr == null)
                {
                    Logger?.LogError("ExternalDeck {0} border over sprite id {1} couldn't be converted into spr enum", deck.GlobalName, deck.BordersOverSprite.Id?.ToString() ?? "NULL");
                    continue;
                }

                if (border_over_spr != null)
                {
                    if (deck_borders_over_dict.Contains(deck_key))
                    {
                        deck_borders_over_dict[deck_key] = border_over_spr;
                    }
                    else
                    {
                        deck_borders_over_dict.Add(deck_key, border_over_spr);
                    }
                }

                var card_default_spr = TypesAndEnums.IntToSpr(deck.CardArtDefault.Id);
                if (card_default_spr == null)
                {
                    Logger?.LogError("ExternalDeck {0} card default sprite id {1} couldn't be converted into spr enum", deck.GlobalName, deck.CardArtDefault.Id?.ToString() ?? "NULL");
                    continue;
                }

                if (border_over_spr != null)
                {
                    if (card_art_deck_default_dict.Contains(deck_key))
                    {
                        card_art_deck_default_dict[deck_key] = card_default_spr;
                    }
                    else
                    {
                        card_art_deck_default_dict.Add(deck_key, card_default_spr);
                    }
                }
            }

            // animations

            IDictionary char_animation_dictionary = db_type.GetField("charAnimations")?.GetValue(null) as IDictionary ?? throw new Exception();
            var spr_list_type = typeof(List<>).MakeGenericType(TypesAndEnums.SprType);
            var new_char_anim_dict_type = typeof(Dictionary<,>).MakeGenericType(typeof(string), spr_list_type);

            foreach (var character_animation_group in registered_animations.Values.GroupBy(e => e.Deck.GlobalName))
            {
                IDictionary? animation_lookup = null;
                if (!char_animation_dictionary.Contains(character_animation_group.Key))
                {
                    animation_lookup = Activator.CreateInstance(new_char_anim_dict_type) as IDictionary ?? throw new Exception();
                    char_animation_dictionary.Add(character_animation_group.Key, animation_lookup);
                }
                else
                {
                    animation_lookup = char_animation_dictionary[character_animation_group.Key] as IDictionary ?? throw new Exception();
                }

                //iterate over all animations in group

                foreach (var animation in character_animation_group)
                {
                    //create sprite list
                    var spr_list = Activator.CreateInstance(spr_list_type) as IList ?? throw new Exception();
                    //populate it
                    bool valid = true;

                    foreach (var frame in animation.Frames)
                    {
                        var spr_val = TypesAndEnums.IntToSpr(frame?.Id);
                        if (spr_val == null)
                        {
                            Logger?.LogWarning("ExternalAnimation {0} Frame Sprite {1} was not resolved to Spr object. skipping animation entirely", animation.GlobalName, frame?.GlobalName ?? "NULL");
                            valid = false;
                            continue;
                        }

                        spr_list.Add(spr_val);
                    }
                    if (!valid) { continue; }

                    //either overwrite or add list.

                    if (!animation_lookup.Contains(animation.Tag))
                    {
                        animation_lookup.Add(animation.Tag, spr_list);
                    }
                    else if (animation.IntendedOverwrite)
                    {
                        animation_lookup[animation.Tag] = spr_list;
                    }
                    else
                    {
                        Logger?.LogWarning("Collision of external animation {0} detected for character {1} with tag {2}. if you inteded to overwrite, set ExternalAnimation.intendedoverwrite during construction", animation.GlobalName, character_animation_group.Key, animation.Tag);
                    }
                }
            }

            //characters

            IDictionary char_panels_dict = db_type.GetField("charPanels")?.GetValue(null) as IDictionary ?? throw new Exception();

            foreach (var character in registered_characters.Values)
            {
                if (string.IsNullOrWhiteSpace(character.GlobalName))
                    continue;
                var spr_val = TypesAndEnums.IntToSpr(character.CharPanelSpr.Id);
                if (spr_val == null)
                    continue;
                if (char_panels_dict.Contains(character.GlobalName))
                {
                    continue;
                }

                char_panels_dict.Add(character.GlobalName, spr_val);
            }
        }

        /// <summary>
        /// while card and artifact metas are generated by attribute, some last minute resolutions are needed.
        /// this includes card meta.deck for external decks, those ids are not know during compile time and thus cannot easily added into the attribute.
        /// </summary>
        private static void PatchMetasAndStoryFunctions()
        {
            IDictionary card_meta_dictionary = db_type.GetField("cardMetas")?.GetValue(null) as IDictionary ?? throw new Exception("card meta dictionary not found");

            {
                var deck_field = TypesAndEnums.CardMetaType.GetField("deck");
                if (deck_field != null)
                {
                    foreach (var card in registered_cards.Values.Where(e => e != null && e.ActualDeck != null))
                    {
                        if (card == null) continue;

                        if (!card_meta_dictionary.Contains(card.CardType.Name))
                            continue;
                        var meta = card_meta_dictionary[card.CardType.Name];
                        var deck_val = TypesAndEnums.IntToDeck(card.ActualDeck?.Id);
                        if (deck_val == null)
                        {
                            Logger?.LogError("External Card {0} bad actual deck definition", card.GlobalName);
                            continue;
                        }
                        deck_field.SetValue(meta, deck_val);
                    }

                    foreach (var overwrite in card_overwrites.Where(e => e.Value != null && e.Value.ActualDeck != null))
                    {
                        var card = overwrite.Value;
                        if (card == null) continue;

                        if (!card_meta_dictionary.Contains(overwrite.Value))
                            continue;
                        var meta = card_meta_dictionary[overwrite.Value];
                        var deck_val = TypesAndEnums.IntToDeck(card.ActualDeck?.Id);
                        if (deck_val == null)
                        {
                            Logger?.LogError("External Card {0} bad actual deck definition", card.GlobalName);
                            continue;
                        }
                        deck_field.SetValue(meta, deck_val);
                    }

                    var dont_offer_field = TypesAndEnums.CardMetaType.GetField("dontOffer") ?? throw new Exception("deck meta dont offer field not found");
                    var unreleased_field = TypesAndEnums.CardMetaType.GetField("unreleased") ?? throw new Exception("deck meta unreleased field not found");
                    var dont_loc_field = TypesAndEnums.CardMetaType.GetField("dontLoc") ?? throw new Exception("deck meta dontLoc field not found");
                    var upgrades_to_field = TypesAndEnums.CardMetaType.GetField("upgradesTo") ?? throw new Exception("deck meta upgradesTo field not found");
                    var rarity_field = TypesAndEnums.CardMetaType.GetField("rarity") ?? throw new Exception("deck meta rarity field not found");
                    var extra_glossary_field = TypesAndEnums.CardMetaType.GetField("extraGlossary") ?? throw new Exception("deck meta extraGlossary field not found");
                    var weird_card_field = TypesAndEnums.CardMetaType.GetField("weirdCard") ?? throw new Exception("deck meta weirdCard field not found");

                    foreach (var meta_overwrite in card_meta_overwrites)
                    {
                        if (!card_meta_dictionary.Contains(meta_overwrite.Key))
                        {
                            Logger?.LogInformation("ExternalCardMeta {0} no target for overwrite {1}.", meta_overwrite.Value.GlobalName, meta_overwrite.Key);
                            continue;
                        }
                        var original_meta = card_meta_dictionary[meta_overwrite.Key];

                        var new_meta = meta_overwrite.Value;
                        if (new_meta == null)
                            continue;
                        //only set fields that are not null
                        if (new_meta.Unreleased != null)
                        {
                            unreleased_field.SetValue(original_meta, new_meta.Unreleased);
                        }
                        if (new_meta.WeirdCard != null)
                        {
                            weird_card_field.SetValue(original_meta, new_meta.WeirdCard);
                        }
                        if (new_meta.ExtraGlossary != null)
                        {
                            extra_glossary_field.SetValue(original_meta, new_meta.ExtraGlossary);
                        }

                        if (new_meta.UpgradesTo != null)
                        {
                            var upgrade_vals = new_meta.UpgradesTo.Select(e => TypesAndEnums.IntToUpgrade(e)).Where(e => e != null).ToArray();
                            var upgrade_arr = Array.CreateInstance(TypesAndEnums.UpgradeType, upgrade_vals.Length);
                            for (int i = 0; i < upgrade_vals.Length; i++)
                            {
                                upgrade_arr.SetValue(upgrade_vals[i], i);
                            }
                            upgrades_to_field.SetValue(original_meta, upgrade_arr);
                        }

                        if (new_meta.Deck != null)
                        {
                            var deck_val = TypesAndEnums.IntToDeck(new_meta.Deck.Id);
                            deck_field.SetValue(original_meta, deck_val);
                        }

                        if (new_meta.DontLoc != null)
                        {
                            dont_loc_field.SetValue(original_meta, new_meta.DontLoc);
                        }

                        if (new_meta.DontOffer != null)
                        {
                            dont_offer_field.SetValue(original_meta, new_meta.DontOffer);
                        }
                        if (new_meta.Rarity != null)
                        {
                            var rarity_val = TypesAndEnums.IntToRarity(new_meta.Rarity);
                            if (rarity_val != null)
                                rarity_field.SetValue(original_meta, rarity_val);
                        }
                    }
                }
                else
                    throw new Exception("Deck field in card meta type not found.");
            }
        }

        /// <summary>
        /// Do anything to db.story.
        /// </summary>
        private static void PatchStory()
        {
        }

        private void LoadDbManifests()
        {
            foreach (var manifest in ModAssemblyHandler.DBManifests)
            {
                if (manifest == null) continue;
                manifest.LoadManifest(this);
            }
        }

        private void PatchEnumExtension()
        {
            var deck_str_dictionary = TypesAndEnums.EnumExtensionsType.GetField("deckStrs", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as IDictionary ?? throw new Exception("deckStrs dictionary not found");

            foreach (var deck in registered_decks)
            {
                var deck_val = TypesAndEnums.IntToDeck(deck.Key);



                if (deck_val == null || string.IsNullOrEmpty(deck.Value.GlobalName))
                    continue;
                //prevent existing enums to be prematurely written and breaking their sprites.
                if (Enum.IsDefined(TypesAndEnums.DeckType, deck_val))
                    continue;                
                if (!deck_str_dictionary.Contains(deck_val))
                    deck_str_dictionary.Add(deck_val, deck.Value.GlobalName);
            }
        }

        /// <summary>
        /// Activate characters
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void PatchNewRunOptions()
        {
            IList all_char_list = TypesAndEnums.NewRunOptionsType.GetField("allChars", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as IList ?? throw new Exception();
            foreach (var character in registered_characters.Values)
            {
                var deck_val = TypesAndEnums.IntToDeck(character.Deck.Id);
                if (deck_val == null)
                    continue;
                if (all_char_list.Contains(deck_val))
                    continue;
                all_char_list.Add(deck_val);
            }
        }

        private void PatchStarterSets()
        {
            var starter_sets_dictionary = TypesAndEnums.StarterDeckType.GetField("starterSets")?.GetValue(null) as IDictionary ?? throw new Exception("couldn't find starterdeck.startersets");
            var card_list_type = typeof(List<>).MakeGenericType(TypesAndEnums.CardType);
            var artifact_list_type = typeof(List<>).MakeGenericType(TypesAndEnums.ArtifactType);

            var cards_field = TypesAndEnums.StarterDeckType.GetField("cards") ?? throw new Exception("StarterDeck.cards not found");
            var artifacts_field = TypesAndEnums.StarterDeckType.GetField("artifacts") ?? throw new Exception("StarterDeck.artifacts not found");

            foreach (var character in registered_characters.Values)
            {
                var deck_val = TypesAndEnums.IntToDeck(character.Deck.Id);

                if (deck_val == null)
                    continue;
                if (starter_sets_dictionary.Contains(deck_val))
                {
                    Logger?.LogWarning("ExternalCharacter {0} Starter Deck {1} is already registered. Skipping. Consider using StarterDeckOverwrite", character.GlobalName, deck_val.ToString());
                    continue;
                }
                var card_arr = Array.CreateInstance(TypesAndEnums.CardType, character.StarterDeck.Count());
                for (int i = 0; i < character.StarterDeck.Count(); i++)
                {
                    var card_instance = Activator.CreateInstance(character.StarterDeck.ElementAt(i));
                    card_arr.SetValue(card_instance, i);
                }

                var artifact_arr = Array.CreateInstance(TypesAndEnums.ArtifactType, character.StarterArtifacts.Count());
                for (int i = 0; i < character.StarterArtifacts.Count(); i++)
                {
                    var artifact_instance = Activator.CreateInstance(character.StarterArtifacts.ElementAt(i));
                    artifact_arr.SetValue(artifact_instance, i);
                }

                var card_list = Activator.CreateInstance(card_list_type, new object[] { card_arr });
                var artifact_list = Activator.CreateInstance(artifact_list_type, new object[] { artifact_arr });

                var new_starter_deck = Activator.CreateInstance(TypesAndEnums.StarterDeckType);

                if (new_starter_deck == null)
                {
                    continue;
                }

                cards_field.SetValue(new_starter_deck, card_list);
                artifacts_field.SetValue(new_starter_deck, artifact_list);
                starter_sets_dictionary.Add(deck_val, new_starter_deck);
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
    }
}