using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IRawShipManifest
    {
        public void LoadManifest(IRawShipRegistry shipRegistry);
    }
}