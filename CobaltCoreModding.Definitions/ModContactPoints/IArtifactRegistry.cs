using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IArtifactRegistry : IArtifactLookup
    {
        public bool RegisterArtifact(ExternalArtifact artifact, string? overwrite = null);
    }
}