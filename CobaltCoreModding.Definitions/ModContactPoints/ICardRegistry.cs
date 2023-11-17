using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICardRegistry : ICardLookup
    {
        bool RegisterCard(ExternalCard card, string? overwrite = null);
    }
}