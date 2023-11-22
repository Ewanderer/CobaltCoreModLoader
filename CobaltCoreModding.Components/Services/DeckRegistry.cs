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
    public class DeckRegistry : IDeckRegistry
    {
        private const int deck_counter_start = 1000000;
        private static int deck_counter = deck_counter_start;
        private static Dictionary<string, ExternalDeck> deck_lookup = new Dictionary<string, ExternalDeck>();
        private static ILogger<IDeckRegistry>? Logger;
        private static Dictionary<int, ExternalDeck> registered_decks = new Dictionary<int, ExternalDeck>();
        private readonly ModAssemblyHandler modAssemblyHandler;

        public DeckRegistry(ILogger<IDeckRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalDeck? LookupDeck(string globalName)
        {
            if (!deck_lookup.TryGetValue(globalName, out var deck))
                Logger?.LogWarning("ExternalDeck {0} not found", globalName);
            return deck;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.DeckManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by DeckRegistry");
                }
            }
            PatchEnumExtension();
        }

        ExternalDeck IDeckLookup.LookupDeck(string globalName)
        {
            return LookupDeck(globalName) ?? throw new KeyNotFoundException();
        }

        public IManifest LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        public ExternalSprite LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        bool IDeckRegistry.RegisterDeck(ExternalDeck deck, int? overwrite)
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

        internal static void PatchDeckData()
        {
            IDictionary deck_dict = TypesAndEnums.DbType.GetField("decks")?.GetValue(null) as IDictionary ?? throw new Exception("decks dictinoary not found");

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

        internal static void PatchDeckSprites()
        {
            //decks
            IDictionary deck_borders_dict = TypesAndEnums.DbType.GetField("deckBorders")?.GetValue(null) as IDictionary ?? throw new Exception();
            IDictionary deck_borders_over_dict = TypesAndEnums.DbType.GetField("deckBordersOver")?.GetValue(null) as IDictionary ?? throw new Exception();
            IDictionary card_art_deck_default_dict = TypesAndEnums.DbType.GetField("cardArtDeckDefault")?.GetValue(null) as IDictionary ?? throw new Exception();
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
    }
}