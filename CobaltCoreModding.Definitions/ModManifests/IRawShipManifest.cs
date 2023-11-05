using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IRawShipManifest : IManifest
    {
        public void LoadManifest(IRawShipRegistry shipRegistry);
    }
}