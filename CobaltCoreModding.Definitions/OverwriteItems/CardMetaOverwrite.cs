using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.OverwriteItems
{
    /// <summary>
    /// Used to overwrite a card meta.
    /// only assign values that actually should be changed.y
    /// </summary>
    public class CardMetaOverwrite
    {
        public CardMetaOverwrite(string globalName)
        {
            if (string.IsNullOrWhiteSpace(globalName))
                throw new ArgumentNullException(nameof(globalName));
            GlobalName = globalName;
        }

        public ExternalDeck? Deck { get; init; }
        public bool? DontLoc { get; init; }
        public bool? DontOffer { get; init; }
        public string[]? ExtraGlossary { get; init; }
        public string GlobalName { get; init; }
        public int? Rarity { get; init; }
        public bool? Unreleased { get; init; }
        public int[]? UpgradesTo { get; init; }
        public bool? WeirdCard { get; init; }
    }
}