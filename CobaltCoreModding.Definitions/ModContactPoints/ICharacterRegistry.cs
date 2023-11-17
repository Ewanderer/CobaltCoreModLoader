using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICharacterRegistry : ICharacterLookup
    {
        bool RegisterCharacter(ExternalCharacter character);
    }
}