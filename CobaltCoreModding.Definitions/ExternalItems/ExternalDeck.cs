using System.Drawing;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// Meta information to create a Deck and DeckDef.
    /// </summary>
    public class ExternalDeck
    {
        private object? deckDef;
        private int? id;

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

        private ExternalDeck(int id)
        {
            Id = id;
            GlobalName = "";
            DeckColor = Color.White;
            TitleColor = Color.Black;
            CardArtDefault = ExternalSprite.GetRaw(0);
            BorderSprite = ExternalSprite.GetRaw(0);
        }

        /// <summary>
        /// Border Sprite that is overlayed the entire card and can even spill over it.
        /// </summary>
        public ExternalSprite? BordersOverSprite { get; init; }

        /// <summary>
        /// Base Border Sprite
        /// </summary>
        public ExternalSprite BorderSprite { get; init; }

        /// <summary>
        /// The card ard
        /// </summary>
        public ExternalSprite CardArtDefault { get; init; }

        /// <summary>
        /// Color multiplied with card texture
        /// </summary>
        public Color DeckColor { get; init; }

        /// <summary>
        /// After initalisation will contain the DeckDef object within cobalt core.
        /// </summary>
        public object? DeckDefReference
        {
            get => deckDef;
            set
            {
                if (deckDef != null)
                    deckDef = value;
            }
        }

        public string GlobalName { get; init; }

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
        /// Name of the card
        /// </summary>
        public Color TitleColor { get; init; }

        public static ExternalDeck GetRaw(int id)
        {
            return new ExternalDeck(id);
        }
    }
}