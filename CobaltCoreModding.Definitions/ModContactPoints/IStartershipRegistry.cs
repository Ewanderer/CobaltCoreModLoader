using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IStartershipRegistry : IStartershipLookup
    {
        bool RegisterStartership(ExternalStarterShip starterShip);
    }
}