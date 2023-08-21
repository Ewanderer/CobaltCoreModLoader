using CobaltCoreModding.Definitions.OverwriteItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICardOverwriteRegistry
    {
        /// <summary>
        /// Call to overwrite the card meta of an existing card.
        /// </summary>
        /// <param name="cardMeta">the meta from which overrides will be grabbed</param>
        /// <param name="card_key">the name of the card to be overwritten</param>
        /// <returns>if the overwrite was successfully registered</returns>
        bool RegisterCardMetaOverwrite(CardMetaOverwrite cardMeta, string card_key);

        /// <summary>
        ///
        /// </summary>
        /// <param name="statOverwrite"></param>
        /// <returns></returns>
        bool RegisterCardStatOverwrite(CardStatOverwrite statOverwrite);
    }
}