using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDeckRegistry : IDeckLookup
    {
        bool RegisterDeck(ExternalDeck deck, int? overwrite = null);
    }
}