using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.OverwriteItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDbRegistry : ICobaltCoreContact
    {
        bool RegisterArtifact(ExternalArtifact artifact);



        bool RegisterCharacter(ExternalCharacter character);



        bool RegisterEnemy(ExternalEnemy enemy);

        bool RegisterMidrowItem(ExternalMidrowItem midrowItem);

        bool RegisterModifier(ExternalModifier modifier);

        bool RegisterSpaceThing(ExternalSpaceThing spaceThing);



        bool RegisterStatus(ExternalStatus status);



        ExternalSprite? GetModSprite(string globalName);
    }
}