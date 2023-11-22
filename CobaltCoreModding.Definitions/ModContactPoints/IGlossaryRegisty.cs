using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IGlossaryRegisty : IGlossaryLookup
    {
        bool RegisterGlossary(ExternalGlossary glossary);
    }
}