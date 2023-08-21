using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDbRegistry : ICobaltCoreContact
    {
        ExternalSprite? GetModSprite(string globalName);

        bool RegisterArtifact(ExternalArtifact artifact);

        bool RegisterEnemy(ExternalEnemy enemy);

        bool RegisterMidrowItem(ExternalMidrowItem midrowItem);

        bool RegisterModifier(ExternalModifier modifier);

        bool RegisterSpaceThing(ExternalSpaceThing spaceThing);

        bool RegisterStatus(ExternalStatus status);
    }
}