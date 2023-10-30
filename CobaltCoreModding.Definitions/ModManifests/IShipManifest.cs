using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// Used to feed external ships
    /// </summary>
    public interface IShipManifest : IManifest
    {
        public void LoadManifest(IShipRegistry shipRegistry);
    }
}