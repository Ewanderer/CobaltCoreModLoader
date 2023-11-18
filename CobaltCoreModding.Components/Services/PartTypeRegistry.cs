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
        private int id_counter = id_counter_start;
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
                return false;
            }

            if (externalPartType.Id != null)
            {
                return false;
            }

            if (!registeredPartTypes.TryAdd(externalPartType.GlobalName, externalPartType))
            {
                return false;
            }

            externalPartType.Id = id_counter++;
            return true;
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