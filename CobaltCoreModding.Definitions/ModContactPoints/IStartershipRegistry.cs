using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IStartershipRegistry
    {
        bool RegisterStartership(ExternalStarterShip starterShip);
    }
}