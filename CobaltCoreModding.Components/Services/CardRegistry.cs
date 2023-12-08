using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    public class CardRegistry : ICardRegistry
    {
        private static Dictionary<string, ExternalCard> card_overwrites = new Dictionary<string, ExternalCard>();
        private static ILogger<IDeckRegistry>? Logger;
        private static Dictionary<string, ExternalCard> registered_cards = new Dictionary<string, ExternalCard>();
        private readonly ModAssemblyHandler modAssemblyHandler;

        public CardRegistry(ILogger<IDeckRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            modAssemblyHandler = mah;
            Logger = logger;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalCard? LookupCard(string globalName)
        {
            if (registered_cards.TryGetValue(globalName, out var card))
                Logger?.LogWarning("ExternalCard {0} not found", globalName);
            return card;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.CardManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by CardRegistry");
                }
            }
        }

        ExternalCard ICardLookup.LookupCard(string globalName)
        {
            return LookupCard(globalName) ?? throw new KeyNotFoundException();
        }

        public ExternalDeck LookupDeck(string globalName)
        {
            return DeckRegistry.LookupDeck(globalName) ?? throw new KeyNotFoundException();
        }

        public IManifest LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        public ExternalSprite LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        bool ICardRegistry.RegisterCard(ExternalCard card, string? overwrite)
        {
            if (!card.CardType.IsSubclassOf(TypesAndEnums.CardType))
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

        internal static void PatchCardData()
        {
            var card_dict = TypesAndEnums.DbType.GetField("cards")?.GetValue(null) as Dictionary<string, Type>;

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

        internal static void PatchCardLocalisation(string locale, ref Dictionary<string, string> __result)
        {
            foreach (var card in registered_cards.Values.Concat(card_overwrites.Values))
            {
                if (card == null) continue;

                card.GetLocalisation(locale, out var name, out var description, out var descriptionA, out var descriptionB);
                if (name == null && description == null)
                    continue;
                //make card and get value
                var inst = Activator.CreateInstance(card.CardType);
                if (inst == null)
                {
                    Logger?.LogCritical($"Cannot spawn instance of card {card.CardType.Name} because no empty constructor found.");
                    continue;
                }

                if (name != null)
                {
                    var key = card.NameLocKey;
                    if (!__result.TryAdd(key, name))
                        Logger?.LogCritical($"Cannot add {key} to localisations, since already know. skipping...");
                }

                if (description != null)
                {
                    var key = card.DescLocKey;
                    if (!__result.TryAdd(key, description))
                        Logger?.LogCritical($"Cannot add {key} to localisations, since already know. skipping...");
                }

                if (descriptionA != null)
                {
                    var key = card.DescALocKey;
                    if (!__result.TryAdd(key, descriptionA))
                        Logger?.LogCritical($"Cannot add {key} to localisations, since already know. skipping...");
                }

                if (descriptionB != null)
                {
                    var key = card.DescBLocKey;
                    if (!__result.TryAdd(key, descriptionB))
                        Logger?.LogCritical($"Cannot add {key} to localisations, since already know. skipping...");
                }
            }
        }

        internal static void PatchCardMetas()
        {
            IDictionary card_meta_dictionary = TypesAndEnums.DbType.GetField("cardMetas")?.GetValue(null) as IDictionary ?? throw new Exception("card meta dictionary not found");
            var deck_field = TypesAndEnums.CardMetaType.GetField("deck") ?? throw new Exception();
            var extra_glossary_field = TypesAndEnums.CardMetaType.GetField("extraGlossary") ?? throw new Exception();

            foreach (var card in registered_cards.Values.Where(e => e != null && e.ActualDeck != null))
            {
                if (!card_meta_dictionary.Contains(card.CardType.Name))
                {
                    //make new meta
                    var new_meta = Activator.CreateInstance(TypesAndEnums.CardMetaType);
                    card_meta_dictionary.Add(card.CardType.Name, new_meta);
                }
                var meta = card_meta_dictionary[card.CardType.Name];
                var deck_val = TypesAndEnums.IntToDeck(card.ActualDeck?.Id);
                if (deck_val == null)
                {
                    Logger?.LogError("External Card {0} bad actual deck definition", card.GlobalName);
                    continue;
                }
                deck_field.SetValue(meta, deck_val);
            }

            foreach (var card in registered_cards.Values.Where(e => e.ExtraGlossary.Any()))
            {
                if (!card_meta_dictionary.Contains(card.CardType.Name))
                {
                    //make new meta
                    var new_meta = Activator.CreateInstance(TypesAndEnums.CardMetaType);
                    card_meta_dictionary.Add(card.CardType.Name, new_meta);
                }
                var meta = card_meta_dictionary[card.CardType.Name];
                var old_val = extra_glossary_field.GetValue(meta) as IEnumerable<string>;
                if (old_val == null)
                {
                    Logger?.LogError("Cannot extract extra glossary from card meta for {0}", card.GlobalName);
                    continue;
                }
                var new_val = old_val.Concat(card.ExtraGlossary).ToArray();
                extra_glossary_field.SetValue(meta, new_val);
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
        }

        internal static void PatchCardSprites()
        {
            //cards
            IDictionary card_art_dictionary = TypesAndEnums.DbType.GetField("cardArt", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new Exception("card art dictionary not found");

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
        }

        internal bool ValidateCard(ExternalCard card)
        {
            return registered_cards.TryGetValue(card.GlobalName, out var reg_card) && reg_card == card;
        }
    }
}