﻿using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDbRegistry : ICobaltCoreContact
    {
        bool RegisterArtifact(ExternalArtifact artifact);

        bool RegisterCard(ExternalCard card, string? overwrite = null);

        bool RegisterCharacter(ExternalCharacter character);

        bool RegisterDeck(ExternalDeck deck, int? overwrite = null);

        bool RegisterEnemy(ExternalEnemy enemy);

        bool RegisterMidrowItem(ExternalMidrowItem midrowItem);

        bool RegisterModifier(ExternalModifier modifier);

        bool RegisterSpaceThing(ExternalSpaceThing spaceThing);

        bool RegisterStatus(ExternalStatus status);

        ExternalSprite? GetModSprite(string globalName);

        ExternalSprite GetOriginalSprite(int sprVal);
    }
}