namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalCard
    {
        /// <summary>
        /// key value pairs are locale and localized card name. will only be applied if card implementation has a value for
        /// </summary>
        private Dictionary<string, string> localized_card_names = new Dictionary<string, string>();

        public ExternalCard(string globalName, Type cardType, ExternalSprite cardArt, ExternalDeck? actualDeck = null)
        {
            if (string.IsNullOrWhiteSpace(globalName)) throw new ArgumentException("External card without global name");
            if (cardArt.Id == null) throw new ArgumentException($"Card Art not registered in External Card {globalName}");
            if (actualDeck != null && actualDeck.Id == null) throw new ArgumentException($"Unregistered External Deck in Card {globalName}");
            GlobalName = globalName;
            CardType = cardType;
            CardArt = cardArt;
            ActualDeck = actualDeck;
        }

        /// <summary>
        /// Since you cannot put custom decks into deck meta attribute,
        /// so if need be they can be fed here and the DB Extender will overwrite the CardMeta Entry in the DB.
        /// </summary>
        public ExternalDeck? ActualDeck { get; init; }

        public ExternalSprite CardArt { get; init; }
        public Type CardType { get; init; }
        public string GlobalName { get; init; }

        public void AddLocalisation(string text, string locale = "en")
        {
            if (!localized_card_names.TryAdd(locale, text))
                localized_card_names[locale] = text;
        }

        public void GenerateCardNamesFromResourceFile()
        {
#warning todo
        }

        public string? GetLocalisation(string locale)
        {
            //look up localisations with en as a fallback.
            if (!localized_card_names.TryGetValue(locale, out var text))
                localized_card_names.TryGetValue("en", out text);
            return text;
        }

        public bool ValidReferences()
        {
            if (CardArt.Id == null)
                return false;
            if (ActualDeck != null && ActualDeck.Id == null)
                return false;
            return true;
        }
    }
}