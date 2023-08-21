using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDeckRegistry
    {
        bool RegisterDeck(ExternalDeck deck, int? overwrite = null);
    }
}