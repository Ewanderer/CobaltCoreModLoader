using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IGlossaryRegisty
    {
        bool RegisterGlossary(ExternalGlossary glossary);
    }
}