using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IShipRegistry : IShipLookup
    {
        bool RegisterShip(ExternalShip ship);
    }
}