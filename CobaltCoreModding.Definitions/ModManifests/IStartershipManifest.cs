using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IStartershipManifest : IManifest
    {
        void LoadManifest(IStartershipRegistry registry);
    }
}