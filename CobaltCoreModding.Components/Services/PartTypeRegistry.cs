using CobaltCoreModding.Components.Services;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CobaltCoreModdding.Components.Services
{
    public class PartTypeRegistry : IPartTypeRegistry
    {
        private const int id_counter_start = 1000000;
        private static readonly Dictionary<string, ExternalPartType> registeredPartTypes = new Dictionary<string, ExternalPartType>();
        private static ILogger? logger;
        private static int id_counter = id_counter_start;
        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception();

        public PartTypeRegistry(ILogger<PartTypeRegistry> logger)
        {
            PartTypeRegistry.logger = logger;
        }

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
            foreach (var manifest in ModAssemblyHandler.PartTypeManifests)
            {
                manifest.LoadManifest(this);
            }
        }
    }
}