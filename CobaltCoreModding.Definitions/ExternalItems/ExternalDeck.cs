using System.Drawing;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// Meta information to create a Deck and DeckDef.
    /// </summary>
    public class ExternalDeck
    {
        public string GlobalName { get; init; }

        private int? id;

        /// <summary>
        /// The Deck value assigned to this Deck. By the time any other external ressource like card needs this value, it will already be assigned aka not null
        /// </summary>
        public int? Id
        {
            get => id; set
            {
                if (id != null)
                    throw new InvalidOperationException("This ExternalSprite was already registered");
                id = value;
            }
        }

        /// <summary>
        /// Color multiplied with card texture
        /// </summary>
        private Color DeckColor { get; init; }

        /// <summary>
        /// Name of the card
        /// </summary>
        private Color TitleColor { get; init; }

        /// <summary>
        /// The card ard
        /// </summary>
        private ExternalSprite CardArtDefault { get; init; }

        /// <summary>
        /// Base Border Sprite
        /// </summary>
        private ExternalSprite BorderSprite { get; init; }

        /// <summary>
        /// Border Sprite that is overlayed the entire card and can even spill over it.
        /// </summary>
        private ExternalSprite? BordersOverSprite { get; init; }

        public ExternalDeck(string globalName, Color deckColor, Color titleColor, ExternalSprite cardArtDefault, ExternalSprite borderSprite, ExternalSprite? bordersOverSprite)
        {
            if (string.IsNullOrWhiteSpace(globalName)) throw new ArgumentException("Empty global name in externaldeck");
            if (cardArtDefault.Id == null) throw new ArgumentException($"Card Art not registered in ExternalDeck {cardArtDefault.GlobalName}");
            if (borderSprite.Id == null) throw new ArgumentException($"Border Sprite not registered in ExternalDeck {cardArtDefault.GlobalName}");
            if (bordersOverSprite != null && bordersOverSprite.Id == null) throw new ArgumentException($"BorderOvers Sprite not registered in ExternalDeck {cardArtDefault.GlobalName}");

            GlobalName = globalName;
            DeckColor = deckColor;
            TitleColor = titleColor;
            CardArtDefault = cardArtDefault;
            BorderSprite = borderSprite;
            BordersOverSprite = bordersOverSprite;
        }
    }
}