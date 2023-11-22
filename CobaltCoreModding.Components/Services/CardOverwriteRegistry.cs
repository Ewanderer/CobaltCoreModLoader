using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.OverwriteItems;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    public class CardOverwriteRegistry : ICardOverwriteRegistry
    {
        /// <summary>
        /// GlobalName -> Card Meta
        /// </summary>
        private static Dictionary<string, CardMetaOverwrite> card_meta_lookup = new Dictionary<string, CardMetaOverwrite>();

        /// <summary>
        /// CardType -> Card Meta
        /// </summary>
        private static Dictionary<string, CardMetaOverwrite> card_meta_overwrites = new Dictionary<string, CardMetaOverwrite>();

        private static Dictionary<string, CardStatOverwrite> card_stat_lookup = new Dictionary<string, CardStatOverwrite>();
        private static Dictionary<Type, HashSet<CardStatOverwrite>> card_stat_overwrites = new Dictionary<Type, HashSet<CardStatOverwrite>>();
        private static ILogger? Logger;
        private readonly ModAssemblyHandler modAssemblyHandler;

        public CardOverwriteRegistry(ILogger<ICardOverwriteRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
            modAssemblyHandler = mah;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.CardOverwriteManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by CardOverwriteRegistry");
                }
            }
        }

        bool ICardOverwriteRegistry.RegisterCardMetaOverwrite(CardMetaOverwrite cardMeta, string card_key)
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

        bool ICardOverwriteRegistry.RegisterCardStatOverwrite(CardStatOverwrite statOverwrite)
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

        internal static void PatchLogic()
        {
            //inject card stat overwrites
            {
                var base_stat_postfix = typeof(CardOverwriteRegistry).GetMethod("OverwriteCardStats", BindingFlags.Static | BindingFlags.NonPublic);
                var harmony = new Harmony("modloader.cardoverwriteregistry.card_stat_overwrites");
                foreach (var overwrite in card_stat_overwrites)
                {
                    //These are virtual and thus need to be retrieved everytime, but also improves ingame performance.
                    var base_stat_method = overwrite.Key.GetMethod("GetData") ?? throw new Exception();
                    harmony.Patch(base_stat_method, postfix: new HarmonyMethod(base_stat_postfix));
                }
            }
        }

        internal static void PatchMeta()
        {
            IDictionary card_meta_dictionary = TypesAndEnums.DbType.GetField("cardMetas")?.GetValue(null) as IDictionary ?? throw new Exception("card meta dictionary not found");
            var deck_field = TypesAndEnums.CardMetaType.GetField("deck") ?? throw new Exception();
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

        /// <summary>
        /// Postfix
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="__instance"></param>
        /// <param name="state"></param>
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
    }
}