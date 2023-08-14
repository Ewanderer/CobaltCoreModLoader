using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.OverwriteItems;

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

        bool RegisterAnimation(ExternalAnimation animation);

        /// <summary>
        ///
        /// </summary>
        /// <param name="statOverwrite"></param>
        /// <returns></returns>
        bool RegisterCardStatOverwrite(CardStatOverwrite statOverwrite);

        bool RegisterStatus(ExternalStatus status);

        /// <summary>
        /// Call to overwrite the card meta of an existing card.
        /// </summary>
        /// <param name="cardMeta">the meta from which overrides will be grabbed</param>
        /// <param name="card_key">the name of the card to be overwritten</param>
        /// <returns>if the overwrite was successfully registered</returns>
        bool RegisterCardMetaOverwrite(CardMetaOverwrite cardMeta, string card_key);

        ExternalSprite? GetModSprite(string globalName);

        ExternalSprite GetOriginalSprite(int sprVal);
    }
}