using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICharacterRegistry
    {
        bool RegisterCharacter(ExternalCharacter character);
    }
}