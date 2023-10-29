using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IShipPartManifest : IManifest
    {
        public void LoadManifest(IShipPartRegistry registry);
    }
}