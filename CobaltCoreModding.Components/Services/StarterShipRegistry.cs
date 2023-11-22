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
    public class StarterShipRegistry : IStartershipRegistry, IRawStartershipRegistry
    {
        private static readonly Dictionary<string, Dictionary<string, (string, string)>> rawLocalizations = new Dictionary<string, Dictionary<string, (string, string)>>();
        private static readonly Dictionary<string, List<Type>> rawStarterExclusiveArtifacts = new();
        private static readonly Dictionary<string, object> registeredRawStarterShips = new Dictionary<string, object>();
        private static readonly Dictionary<string, ExternalStarterShip> registeredStarterShips = new Dictionary<string, ExternalStarterShip>();
        private static FieldInfo artifacts_field = TypesAndEnums.StarterShipType.GetField("artifacts") ?? throw new Exception("Cannot find StarterShip.artifacts fieldinfo");
        private static FieldInfo cards_field = TypesAndEnums.StarterShipType.GetField("cards") ?? throw new Exception("Cannot find StarterShip.cards fieldinfo");
        private static StarterShipRegistry? instance;
        private static ILogger<StarterShipRegistry>? logger;
        private static ModAssemblyHandler? modAssemblyHandler;
        private static FieldInfo ship_field = TypesAndEnums.StarterShipType.GetField("ship") ?? throw new Exception("Cannot find Startership.ship fieldinfo");
        private static FieldInfo ship_is_player_field = TypesAndEnums.ShipType.GetField("isPlayerShip") ?? throw new Exception("Cannot find Ship.isPlayerShip fieldinfo");
        private static FieldInfo ship_key_field = TypesAndEnums.ShipType.GetField("key") ?? throw new Exception("Cannot find Ship.key fieldinfo");
        private static FieldInfo ship_parts_field = TypesAndEnums.ShipType.GetField("parts") ?? throw new Exception("Cannot find ship.parts field.");
        private static FieldInfo state_ship_field = TypesAndEnums.StateType.GetField("ship") ?? throw new Exception("Cannot find state.ship field.");
        private readonly ArtifactRegistry artifactRegistry;
        private readonly CardRegistry cardRegistry;
        private static readonly Dictionary<string, List<Type>> rawStarterExclusiveCards = new();

        public StarterShipRegistry(CardRegistry cardRegistry, ArtifactRegistry artifactRegistry, ILogger<StarterShipRegistry> logger, ModAssemblyHandler mah)
        {
            this.cardRegistry = cardRegistry;
            this.artifactRegistry = artifactRegistry;
            StarterShipRegistry.logger = logger;
            instance = this;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static void LoadRawManifests()
        {
            if (instance == null)
            {
                logger?.LogCritical("Instance is null. Cannot load raw starterships.");
                return;
            }
            foreach (var manifest in modAssemblyHandler?.LoadOrderly(ModAssemblyHandler.RawStartershipManifests, logger) ?? ModAssemblyHandler.RawStartershipManifests)
            {
                try
                {
                    manifest.LoadManifest(instance);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by StarterShipRegistry");
                }
            }
        }

        public static object? LookupStarterShip(string globalName)
        {
            object? result = null;
            if (registeredStarterShips.TryGetValue(globalName, out var ship))
            {
                result = ship;
            }
            else if (registeredRawStarterShips.TryGetValue(globalName, out var rawShip))
            {
                result = rawShip;
            }
            else
            {
                logger?.LogWarning("Startership {0} has not been found", globalName);
            }
            return result;
        }

        public static void PatchStarterShips()
        {
            var lookup = TypesAndEnums.StarterShipType.GetField("ships", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new Exception("Failed to extract StarterShip.ships as Idictionary.");
            foreach (var starter in registeredStarterShips.Values)
            {
                if (lookup.Contains(starter.GlobalName))
                {
                    logger?.LogError("StartShip with global name {0} already in StarterShip.ships by key", starter.GlobalName);
                    continue;
                }

                if (!ShipRegistry.CheckShip(starter.ShipGlobalName))
                {
                    logger?.LogError("StarterShip {0} references ship {1} which is not available! Skipping...", starter.GlobalName, starter.ShipGlobalName);
                }
                var actual_starter = ActualizeStarterShip(starter.GlobalName);
                lookup.Add(starter.GlobalName, actual_starter);
            }

            foreach (var (globalName, starterShip) in registeredRawStarterShips)
            {
                if (lookup.Contains(globalName))
                {
                    logger?.LogError("StartShip with global name {0} already in StarterShip.ships by key", globalName);
                    continue;
                }

                var ship = ship_field.GetValue(starterShip);
                var shipKey = ship_key_field.GetValue(ship) as string;

                if (shipKey == null)
                {
                    logger?.LogError("StarterShip {0} has no key in key field! Skipping...", globalName);
                }
                else if (!ShipRegistry.CheckShip(shipKey))
                {
                    logger?.LogError("StarterShip {0} references ship {1} which is not available! Skipping...", globalName, shipKey);
                    continue;
                }
                lookup.Add(globalName, starterShip);
            }
        }

        public void AddRawLocalization(string global_name, string name, string description, string locale = "en")
        {
            if (!registeredRawStarterShips.ContainsKey(global_name))
            {
                logger?.LogWarning("Raw StarterShip {0} cannot add localisation because ship is not registered.", global_name);
                return;
            }

            if (!rawLocalizations.TryGetValue(locale, out var localeDict))
            {
                localeDict = new Dictionary<string, (string, string)>();
                rawLocalizations[locale] = localeDict;
            }

            if (!localeDict.TryAdd(global_name, (name, description)))
            {
                logger?.LogWarning("Raw StarterShip {0} cannot add localisation of name because key already taken.", global_name);
            }
        }

        ExternalArtifact IArtifactLookup.LookupArtifact(string globalName)
        {
            return ArtifactRegistry.LookupArtifact(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalCard ICardLookup.LookupCard(string globalName)
        {
            return CardRegistry.LookupCard(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalDeck IDeckLookup.LookupDeck(string globalName)
        {
            return DeckRegistry.LookupDeck(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalGlossary IGlossaryLookup.LookupGlossary(string globalName)
        {
            return GlossaryRegistry.LookupGlossary(globalName) ?? throw new KeyNotFoundException();
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        object IStartershipLookup.LookupStarterShip(string globalName)
        {
            return LookupStarterShip(globalName) ?? throw new KeyNotFoundException();
        }

        bool IRawStartershipRegistry.MakeArtifactExclusive(string shipName, Type artifactType)
        {
            if (!artifactType.IsAssignableTo(TypesAndEnums.ArtifactType))
                return false;
            if (rawStarterExclusiveArtifacts.TryGetValue(shipName, out var list))
                list.Add(artifactType);
            else
                rawStarterExclusiveArtifacts.Add(shipName, new List<Type> { artifactType });
            return true;
        }

        public bool RegisterStartership(ExternalStarterShip starterShip)
        {
            if (string.IsNullOrWhiteSpace(starterShip.GlobalName))
            {
                return false;
            }

            //validate cards
            {
                var invalid_cards = starterShip.StartingCards.Where(c => !cardRegistry.ValidateCard(c));
                if (invalid_cards.Any())
                {
                    logger?.LogWarning("StarterShip {0} has not registered external cards: {1}", starterShip.GlobalName, string.Join(", ", invalid_cards.Select(p => p.GlobalName)));
                    return false;
                }
            }

            {
                var asm = CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Missing Cobalt Core Assembly");
                var invalid_cards = starterShip.NativeStartingCards.Where(ct => !ct.IsAssignableTo(TypesAndEnums.CardType) || !asm.DefinedTypes.Contains(ct));
                if (invalid_cards.Any())
                {
                    logger?.LogWarning("StarterShip {0} has invalid or not cc native extra card types: {1}", starterShip.GlobalName, string.Join(", ", invalid_cards.Select(p => p.Name)));
                    return false;
                }
            }

            //validate artifacts
            {
                var invalid_artifacts = starterShip.StartingArtifacts.Where(a => !artifactRegistry.ValidateArtifact(a));
                if (invalid_artifacts.Any())
                {
                    logger?.LogWarning("StarterShip {0} has not registered external artifacts: {1}", starterShip.GlobalName, string.Join(", ", invalid_artifacts.Select(p => p.GlobalName)));
                    return false;
                }
            }

            {
                var asm = CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Missing Cobalt Core Assembly");
                var invalid_artifacts = starterShip.NativeStartingArtifact.Where(ct => !ct.IsAssignableTo(TypesAndEnums.ArtifactType) || !asm.DefinedTypes.Contains(ct));
                if (invalid_artifacts.Any())
                {
                    logger?.LogWarning("StarterShip {0} has invalid or not cc native extra artifacts types: {1}", starterShip.GlobalName, string.Join(", ", invalid_artifacts.Select(p => p.Name)));
                    return false;
                }
            }

            //ship is not tested, since raw ships are late comers and will be checked while patching starter ships.

            // put into registry.
            if (!registeredStarterShips.TryAdd(starterShip.GlobalName, starterShip))
            {
                logger?.LogWarning("StarterShip with global name {0} already exist. skipping further entries", starterShip.GlobalName);
                return false;
            }

            return true;
        }

        public bool RegisterStartership(object starterShip, string global_name)
        {
            // check global name
            if (string.IsNullOrWhiteSpace(global_name))
            {
                return false;
            }

            // validate startership object
            if (!starterShip.GetType().IsAssignableTo(TypesAndEnums.StarterShipType))
            {
                logger?.LogCritical("Attempted to register a raw ship under global name {0} that isn't a CobaltCore.Ship object.", global_name);
                return false;
            }

            var artifacts = artifacts_field.GetValue(starterShip) as IEnumerable;
            if (artifacts == null)
            {
                logger?.LogWarning("Raw startership {0} couldn't retrieve artifact list", global_name);
                return false;
            }

            var cards = cards_field.GetValue(starterShip) as IEnumerable;
            if (cards == null)
            {
                logger?.LogWarning("Raw startership {0} couldn't retrieve card list", global_name);
                return false;
            }

            foreach (var artifact in artifacts)
            {
                if (!artifact.GetType().IsAssignableTo(TypesAndEnums.ArtifactType))
                {
                    logger?.LogWarning("Raw startership {0} has null artifact", global_name);
                    return false;
                }
            }

            foreach (var card in cards)
            {
                if (!card.GetType().IsAssignableTo(TypesAndEnums.CardType))
                {
                    logger?.LogWarning("Raw startership {0} has null card", global_name);
                    return false;
                }
            }

            if (!registeredRawStarterShips.TryAdd(global_name, starterShip))
            {
                logger?.LogWarning("StarterShip with global name {0} already exist. skipping further entries", global_name);
                return false;
            }

            return true;
        }

        public void RunLogic()
        {
            LoadManifests();
            //patch StoryVars.GetUnlockedShips() to spill all extra ships.
            var harmony = new Harmony("modloader.startershipregistry.general");

            var get_unlocked_ships_method = TypesAndEnums.StoryVarsType.GetMethod("GetUnlockedShips") ?? throw new Exception("StoryVars.GetUnlockedShips method not found");

            var get_unlocked_ships_post = typeof(StarterShipRegistry).GetMethod("GetUnlockedShipsPost", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("StarterShipRegistry.GetUnlockedShipsPost method not found");

            harmony.Patch(get_unlocked_ships_method, postfix: new HarmonyMethod(get_unlocked_ships_post));

            var artifact_reward_get_blocked_artifacts_method = CobaltCoreHandler.CobaltCoreAssembly?.GetType("ArtifactReward")?.GetMethod("GetBlockedArtifacts", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("ArtifactReward.GetBlockedArtifacts method not found");

            var artifact_reward_get_blocked_artifacts_postfix = typeof(StarterShipRegistry).GetMethod("GetBlockedArtifacts_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("StartershipRegistry.GetBlockedArtifacts_Postfix method not found");

            harmony.Patch(artifact_reward_get_blocked_artifacts_method, postfix: new HarmonyMethod(artifact_reward_get_blocked_artifacts_postfix));
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> result)
        {
            foreach (var ship in registeredStarterShips.Values)
            {
                ship.GetLocalisations(locale, out var name, out var desc);
                if (name != null)
                {
                    var key = $"ship.{ship.GlobalName}.name";
                    if (!result.TryAdd(key, name))
                    {
                        logger?.LogWarning("StarterShip {0} cannot add localisation of name because key already taken.", ship.GlobalName);
                    }
                }
                else
                {
                    logger?.LogWarning("StarterShip {0} is missing localisation of name in {1}", ship.GlobalName, locale);
                }
                if (desc != null)
                {
                    var key = $"ship.{ship.GlobalName}.desc";
                    if (!result.TryAdd(key, desc))
                    {
                        logger?.LogWarning("StarterShip {0} cannot add localisation of description because key already taken.", ship.GlobalName);
                    }
                }
                else
                {
                    logger?.LogWarning("StarterShip {0} is missing localisation of description in {1}", ship.GlobalName, locale);
                }
            }

            if (!rawLocalizations.TryGetValue(locale, out var rawLocaleDict))
            {
                return;
            }

            foreach (var (global_name, (name, desc)) in rawLocaleDict)
            {
                {
                    var key = $"ship.{global_name}.name";
                    if (!result.TryAdd(key, name))
                    {
                        logger?.LogWarning("Raw StarterShip {0} cannot add localisation of name because key already taken.", global_name);
                    }
                }
                {
                    var key = $"ship.{global_name}.desc";
                    if (!result.TryAdd(key, desc))
                    {
                        logger?.LogWarning("Raw StarterShip {0} cannot add localisation of description because key already taken.", global_name);
                    }
                }
            }
        }

        private static object ActualizeStarterShip(string global_name)
        {
            if (!registeredStarterShips.TryGetValue(global_name, out var externalShip))
                throw new Exception("Name not found");
            var result = Activator.CreateInstance(TypesAndEnums.StarterShipType) ?? throw new Exception("Failed to create StarterShip object.");

            var inner_ship = ShipRegistry.ActualizeShip(externalShip.ShipGlobalName);

            ship_field.SetValue(result, inner_ship);
            ship_key_field.SetValue(inner_ship, global_name);
            ship_is_player_field.SetValue(inner_ship, true);
            //setup artifacts
            var artifact_list = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypesAndEnums.ArtifactType));

            if (artifact_list is not IList al)
                throw new Exception("unable to get IList from List<artifact>");

            // add external artifacts
            foreach (var artifact in externalShip.StartingArtifacts)
            {
                var a = Activator.CreateInstance(artifact.ArtifactType);
                al.Add(a);
            }

            foreach (var artifact in externalShip.NativeStartingArtifact)
            {
                var a = Activator.CreateInstance(artifact);
                al.Add(a);
            }

            artifacts_field.SetValue(result, artifact_list);

            //setup cards
            var cards_list = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypesAndEnums.CardType));

            if (cards_list is not IList cl)
                throw new Exception("unable to get IList from List<Card>");

            // add external artifacts
            foreach (var card in externalShip.StartingCards)
            {
                var c = Activator.CreateInstance(card.CardType);
                cl.Add(c);
            }

            foreach (var card in externalShip.NativeStartingCards)
            {
                var c = Activator.CreateInstance(card);
                cl.Add(c);
            }

            cards_field.SetValue(result, cards_list);

            return result;
        }

        private static void GetBlockedArtifacts_Postfix(ref HashSet<Type> __result, object s)
        {
            //check which ship we have.
            var ship = state_ship_field.GetValue(s) ?? throw new Exception("Unable to extract ship from state");
            var key = (ship_key_field.GetValue(ship) as string) ?? throw new Exception("Unable to extract key from ship");
            var permitted_types = new List<Type>();
            var forbidden_types = new List<Type>();

            if (registeredStarterShips.TryGetValue(key, out var externalStarterShip))
            {
                permitted_types.AddRange(externalStarterShip.ExclusiveNativeArtifacts);
                permitted_types.AddRange(externalStarterShip.ExclusiveArtifacts.Select(e => e.ArtifactType));
            }
            else if (registeredRawStarterShips.ContainsKey(key))
            {
                //we have a raw starter ship object
                if (rawStarterExclusiveArtifacts.TryGetValue(key, out var types))
                    permitted_types.AddRange(types);
            }

            //go through all ships and put them on forbidden types
            foreach (var starter in registeredStarterShips.Values)
            {
                forbidden_types.AddRange(starter.ExclusiveNativeArtifacts);
                forbidden_types.AddRange(starter.ExclusiveArtifacts.Select(e => e.ArtifactType));
            }

            foreach (var list in rawStarterExclusiveArtifacts.Values)
            {
                forbidden_types.AddRange(list);
            }

            //Add ship specifics
            foreach (var item in forbidden_types.Except(permitted_types))
                __result.Add(item);
            //unlock existing specific artifacts in case someone uses native cc artifacts in their ships.
            foreach (var item in permitted_types)
                __result.Remove(item);
        }

        private static void GetUnlockedShipsPost(ref HashSet<string> __result)
        {
            foreach (var key in registeredStarterShips.Keys)
                __result.Add(key);
            foreach (var key in registeredRawStarterShips.Keys)
                __result.Add(key);
        }

        private void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler?.LoadOrderly(ModAssemblyHandler.StartershipManifests, logger) ?? ModAssemblyHandler.StartershipManifests)
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by StarterShipRegistry");
                }
            }
        }

        public bool MakeCardExclusive(string shipName, Type cardType)
        {
            if (!cardType.IsAssignableTo(TypesAndEnums.CardType))
                return false;
            if (rawStarterExclusiveCards.TryGetValue(shipName, out var list))
                list.Add(cardType);
            else
                rawStarterExclusiveCards.Add(shipName, new List<Type> { cardType });
            return true;
        }
    }
}