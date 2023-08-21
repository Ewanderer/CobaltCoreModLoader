using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICardRegistry
    {
        bool RegisterCard(ExternalCard card, string? overwrite = null);
    }
}