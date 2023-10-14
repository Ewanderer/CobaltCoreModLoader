using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IArtifactRegistry
    {
        public bool RegisterArtifact(ExternalArtifact artifact, string? overwrite = null);
    }
}