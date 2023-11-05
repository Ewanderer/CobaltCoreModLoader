﻿using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModLoader.Utils;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModLoader.Services
{
    public class StarterShipRegistry : IStartershipRegistry, IRawStartershipRegistry
    {
        private static readonly Dictionary<string, ExternalStarterShip> registeredStarterShips = new Dictionary<string, ExternalStarterShip>();
        private static readonly Dictionary<string, object> registeredRawStarterShips = new Dictionary<string, object>();
        private static FieldInfo artifacts_field = TypesAndEnums.StarterShipType.GetField("artifacts") ?? throw new Exception("Cannot find StarterShip.artifacts fieldinfo");
        private static FieldInfo cards_field = TypesAndEnums.StarterShipType.GetField("cards") ?? throw new Exception("Cannot find StarterShip.cards fieldinfo");
        private static StarterShipRegistry? instance;
        private static ILogger<StarterShipRegistry>? logger;

        private static FieldInfo ship_field = TypesAndEnums.StarterShipType.GetField("ship") ?? throw new Exception("Cannot find Startership.ship fieldinfo");
        private static FieldInfo ship_is_player_field = TypesAndEnums.ShipType.GetField("isPlayerShip") ?? throw new Exception("Cannot find Ship.isPlayerShip fieldinfo");
        private static FieldInfo ship_key_field = TypesAndEnums.ShipType.GetField("key") ?? throw new Exception("Cannot find Ship.key fieldinfo");
        private readonly ArtifactRegistry artifactRegistry;
        private readonly CardRegistry cardRegistry;

        public StarterShipRegistry(CardRegistry cardRegistry, ArtifactRegistry artifactRegistry, ILogger<StarterShipRegistry> logger)
        {
            this.cardRegistry = cardRegistry;
            this.artifactRegistry = artifactRegistry;
            StarterShipRegistry.logger = logger;
            instance = this;
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

        public bool RegisterStartership(ExternalStarterShip starterShip)
        {
            if (string.IsNullOrWhiteSpace(starterShip.GlobalName))
            {
                return false;
            }

            //validate cards
            {
                var invalid_cards = starterShip.ExternalCards.Where(c => !cardRegistry.ValidateCard(c));
                if (invalid_cards.Any())
                {
                    logger?.LogWarning("StarterShip {0} has not registered external cards: {1}", starterShip.GlobalName, string.Join(", ", invalid_cards.Select(p => p.GlobalName)));
                    return false;
                }
            }

            {
                var asm = CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Missing Cobalt Core Assembly");
                var invalid_cards = starterShip.ExtraCardTypes.Where(ct => ct.IsAssignableTo(TypesAndEnums.CardType) || !asm.DefinedTypes.Contains(ct));
                if (invalid_cards.Any())
                {
                    logger?.LogWarning("StarterShip {0} has invalid or not cc native extra card types: {1}", starterShip.GlobalName, string.Join(", ", invalid_cards.Select(p => p.Name)));
                    return false;
                }
            }

            //validate artifacts
            {
                var invalid_artifacts = starterShip.ExternalArtifacts.Where(a => !artifactRegistry.ValidateArtifact(a));
                if (invalid_artifacts.Any())
                {
                    logger?.LogWarning("StarterShip {0} has not registered external artifacts: {1}", starterShip.GlobalName, string.Join(", ", invalid_artifacts.Select(p => p.GlobalName)));
                    return false;
                }
            }

            {
                var asm = CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Missing Cobalt Core Assembly");
                var invalid_artifacts = starterShip.ExtraArtifactTypes.Where(ct => ct.IsAssignableTo(TypesAndEnums.ArtifactType) || !asm.DefinedTypes.Contains(ct));
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
            foreach (var artifact in externalShip.ExternalArtifacts)
            {
                var a = Activator.CreateInstance(artifact.ArtifactType);
                al.Add(a);
            }

            foreach (var artifact in externalShip.ExtraArtifactTypes)
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
            foreach (var card in externalShip.ExternalCards)
            {
                var c = Activator.CreateInstance(card.CardType);
                cl.Add(c);
            }

            foreach (var card in externalShip.ExtraCardTypes)
            {
                var c = Activator.CreateInstance(card);
                cl.Add(c);
            }

            cards_field.SetValue(result, cards_list);

            return result;
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
            foreach (var manifest in ModAssemblyHandler.StartershipManifests)
            {
                manifest.LoadManifest(this);
            }
        }
        
        public static void LoadRawManifests()
        {
            if (instance == null)
            {
                logger?.LogCritical("Instance is null. Cannot load raw starterships.");
                return;
            }
            foreach (var manifest in ModAssemblyHandler.RawStartershipManifests)
            {
                manifest.LoadManifest(instance);
            }
        }
    }
}