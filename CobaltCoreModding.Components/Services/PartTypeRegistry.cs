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
    public class PartTypeRegistry : IPartTypeRegistry
    {
        private const int id_counter_start = 1000000;
        private static readonly Dictionary<string, ExternalPartType> registeredPartTypes = new Dictionary<string, ExternalPartType>();
        private static int id_counter = id_counter_start;
        private static ILogger? logger;
        private static FieldInfo part_ptype_field = TypesAndEnums.PartType.GetField("type") ?? throw new Exception("Cannot find part.type field.");
        private static FieldInfo ship_parts_field = TypesAndEnums.ShipType.GetField("parts") ?? throw new Exception("Cannot find ship.parts field.");
        private static FieldInfo state_ship_field = TypesAndEnums.StateType.GetField("ship") ?? throw new Exception("Cannot find state.ship field.");
        private readonly ModAssemblyHandler modAssemblyHandler;

        public PartTypeRegistry(ILogger<PartTypeRegistry> logger, ModAssemblyHandler modAssemblyHandler)
        {
            PartTypeRegistry.logger = logger;
            this.modAssemblyHandler = modAssemblyHandler;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception();

        public static ExternalPartType? LookupPartType(string globalName)
        {
            if (!registeredPartTypes.TryGetValue(globalName, out var type))
                logger?.LogWarning("ExternalPartType {0} not found.", globalName);
            return type;
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalPartType IPartTypeLookup.LookupPartType(string globalName)
        {
            return LookupPartType(globalName) ?? throw new KeyNotFoundException();
        }

        public bool RegisterPartType(ExternalPartType externalPartType)
        {
            if (string.IsNullOrWhiteSpace(externalPartType.GlobalName))
            {
                logger?.LogWarning("Attempted to register external part type without a globalName! Skipping");
                return false;
            }

            if (externalPartType.Id != null)
            {
                logger?.LogWarning("ExternalPartType {0} already has an id", externalPartType.GlobalName);
                return false;
            }

            if (!registeredPartTypes.TryAdd(externalPartType.GlobalName, externalPartType))
            {
                logger?.LogWarning("ExternalPartType with Global Name {0} already registed, skipping...", externalPartType.GlobalName);
                return false;
            }

            externalPartType.Id = id_counter++;
            return true;
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> result)
        {
            foreach (var type in registeredPartTypes.Values)
            {
                if (type.Id == null)
                    continue;
                type.GetLocalisation(locale, out var name, out var desc);

                if (name != null)
                {
                    var key = $"part.{type.Id}.name";
                    if (!result.TryAdd(key, name))
                        logger?.LogWarning("Part {0} cannot register name because key {1} already added somehow", type.GlobalName, key);
                }
                else
                {
                    logger?.LogError("Part {0} has no name found in {1} and english", type.GlobalName, locale);
                }

                if (desc != null)
                {
                    var key = $"part.{type.Id}.desc";
                    if (!result.TryAdd(key, desc))
                        logger?.LogWarning("Part {0} cannot register desc because key {1} already added somehow", type.GlobalName, key);
                }
                else
                {
                    logger?.LogError("Part {0} has no description found in {1} and english", type.GlobalName, locale);
                }
            }
        }

        internal void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.PartTypeManifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by PartTypeRegistry");
                }
            }

            //Patch blocked artifacts
            var harmony = new Harmony("modloader.parttyperegistry.general");

            var artifact_reward_get_blocked_artifacts_method = CobaltCoreHandler.CobaltCoreAssembly?.GetType("ArtifactReward")?.GetMethod("GetBlockedArtifacts", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("ArtifactReward.GetBlockedArtifacts method not found");

            var artifact_reward_get_blocked_artifacts_postfix = typeof(PartTypeRegistry).GetMethod("GetBlockedArtifacts_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("PartTypeRegistry.GetBlockedArtifacts_Postfix method not found");

            harmony.Patch(artifact_reward_get_blocked_artifacts_method, postfix: new HarmonyMethod(artifact_reward_get_blocked_artifacts_postfix));
        }

        private static void GetBlockedArtifacts_Postfix(ref HashSet<Type> __result, object s)
        {
            var ship = state_ship_field.GetValue(s) ?? throw new Exception("Unable to extract ship from state");
            var parts = ship_parts_field.GetValue(ship) as IEnumerable ?? throw new Exception("Unable to extract parts from ship");
            var forbidden = new List<Type>();
            var permitted = new List<Type>();
            foreach (var part in parts)
            {
                var obj = part_ptype_field.GetValue(part);
                if (obj == null)
                    continue;
                var match = registeredPartTypes.Values.FirstOrDefault(e => TypesAndEnums.IntToPType(e.Id)?.Equals(obj) ?? false);
                if (match != null)
                {
                    permitted.AddRange(match.ExclusiveNativeArtifacts);
                    permitted.AddRange(match.ExclusiveArtifacts.Select(e => e.ArtifactType));
                }
            }

            foreach (var p_type in registeredPartTypes.Values)
            {
                forbidden.AddRange(p_type.ExclusiveNativeArtifacts);
                forbidden.AddRange(p_type.ExclusiveArtifacts.Select(e => e.ArtifactType));
            }

            foreach (var a_type in permitted)
            {
                __result.Remove(a_type);
            }
            foreach (var a_type in forbidden.Except(permitted))
            {
                __result.Add(a_type);
            }
        }
    }
}