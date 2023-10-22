using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface ICustomEventManifest : IManifest
    {
        public void LoadManifest(ICustomEventHub eventHub);
    }
}