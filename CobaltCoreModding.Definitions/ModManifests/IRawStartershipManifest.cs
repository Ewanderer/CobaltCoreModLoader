using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IRawStartershipManifest : IManifest
    {
        void LoadManifest(IRawStartershipRegistry registry);
    }
}