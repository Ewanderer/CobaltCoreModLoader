using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    public class CharacterRegistry : ICharacterRegistry
    {
        private static ILogger<ICharacterRegistry>? Logger;
        private static Dictionary<string, ExternalCharacter> registered_characters = new Dictionary<string, ExternalCharacter>();
        private readonly ModAssemblyHandler modAssemblyHandler;

        public CharacterRegistry(ILogger<ICharacterRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalCharacter? LookupCharacter(string globalName)
        {
            if (registered_characters.TryGetValue(globalName, out var character))
                Logger?.LogWarning("ExternalCharacter {0} not found", globalName);
            return character;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.CharacterManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by CharacterRegistry");
                }
            }

            HarmonyPatches();
            PatchNewRunOptions();
            PatchStarterSets();
        }

        public ExternalAnimation LookupAnimation(string globalName)
        {
            return AnimationRegistry.LookupAnimation(globalName) ?? throw new KeyNotFoundException();
        }

        public ExternalCard LookupCard(string globalName)
        {
            return CardRegistry.LookupCard(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalCharacter ICharacterLookup.LookupCharacter(string globalName)
        {
            return LookupCharacter(globalName) ?? throw new KeyNotFoundException();
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

        bool ICharacterRegistry.RegisterCharacter(ExternalCharacter character)
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

        internal static void PatchCharacterLocalisation(string locale, ref Dictionary<string, string> __result)
        {
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

        internal static void PatchCharacterSprites()
        {
            //characters
            IDictionary char_panels_dict = TypesAndEnums.DbType.GetField("charPanels")?.GetValue(null) as IDictionary ?? throw new Exception();

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
                char_panels_dict.Add(character.Deck.GlobalName, spr_val);
            }
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

        private void HarmonyPatches()
        {
            //patch unlocked characters in the game
            var harmony = new Harmony("modloader.characterregistry.unlocked_character");
            var get_unlocked_characters_method = TypesAndEnums.StoryVarsType.GetMethod("GetUnlockedChars", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("GetUnlockedChars method not found.");
            var get_unlocked_characters_postfix = typeof(CharacterRegistry).GetMethod("GetUnlockedCharactersPostfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("GetUnlockedCharactersPostfix not found");
            harmony.Patch(get_unlocked_characters_method, postfix: new HarmonyMethod(get_unlocked_characters_postfix));
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
    }
}