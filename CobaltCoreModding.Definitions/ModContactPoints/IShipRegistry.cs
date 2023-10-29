using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IShipRegistry
    {
        bool RegisterShip(ExternalShip ship);
    }
}