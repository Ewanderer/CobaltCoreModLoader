namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalCard
    {
        private Dictionary<string, string> localized_card_a_descriptions = new Dictionary<string, string>();

        private Dictionary<string, string> localized_card_b_descriptions = new Dictionary<string, string>();

        private Dictionary<string, string> localized_card_descriptions = new Dictionary<string, string>();

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
            ExtraGlossary = Array.Empty<string>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="globalName"></param>
        /// <param name="cardType"></param>
        /// <param name="cardArt"></param>
        /// <param name="actualDeck"></param>
        /// <param name="extraGlossary"></param>
        /// <exception cref="ArgumentException"></exception>
        public ExternalCard(string globalName, Type cardType, ExternalSprite cardArt, ExternalDeck? actualDeck = null, IEnumerable<string>? extraGlossary = null)
        {
            if (string.IsNullOrWhiteSpace(globalName)) throw new ArgumentException("External card without global name");
            if (cardArt.Id == null) throw new ArgumentException($"Card Art not registered in External Card {globalName}");
            if (actualDeck != null && actualDeck.Id == null) throw new ArgumentException($"Unregistered External Deck in Card {globalName}");
            GlobalName = globalName;
            CardType = cardType;
            CardArt = cardArt;
            ActualDeck = actualDeck;
            ExtraGlossary = extraGlossary?.ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Since you cannot put custom decks into deck meta attribute,
        /// so if need be they can be fed here and the DB Extender will overwrite the CardMeta Entry in the DB.
        /// </summary>
        public ExternalDeck? ActualDeck { get; init; }

        public ExternalSprite CardArt { get; init; }

        public Type CardType { get; init; }

        public string DescALocKey => "card." + CardType.Name + ".descA";

        public string DescBLocKey => "card." + CardType.Name + ".descB";

        public string DescLocKey => "card." + CardType.Name + ".desc";

        /// <summary>
        /// Glossary entries that will be added! to card meta.
        /// </summary>
        public IEnumerable<string> ExtraGlossary { get; init; }

        public string GlobalName { get; init; }

        public string NameLocKey => "card." + CardType.Name + ".name";

        /// <summary>
        /// Adds name and optional description for a card.
        /// </summary>
        /// <param name="name">Card name</param>
        /// <param name="desc">Description</param>
        /// <param name="descA">Description for Upgrade A</param>
        /// <param name="descB">Description for Upgrade b</param>
        /// <param name="locale">Language code</param>
        public void AddLocalisation(string name, string? desc = null, string? descA = null, string? descB = null, string locale = "en")
        {
            if (!localized_card_names.TryAdd(locale, name))
                localized_card_names[locale] = name;
            if (desc != null)
            {
                if (!localized_card_descriptions.TryAdd(locale, desc))
                    localized_card_descriptions[locale] = desc;
            }
            if (descA != null)
            {
                if (!localized_card_a_descriptions.TryAdd(locale, descA))
                    localized_card_a_descriptions[locale] = descA;
            }
            if (descB != null)
            {
                if (!localized_card_b_descriptions.TryAdd(locale, descB))
                    localized_card_b_descriptions[locale] = descB;
            }
        }

        public void GenerateCardNamesFromResourceFile()
        {
        }

        public void GetLocalisation(string locale, out string? name, out string? description, out string? descriptionA, out string? descriptionB)
        {
            //look up localisations with en as a fallback.
            if (!localized_card_names.TryGetValue(locale, out name))
                localized_card_names.TryGetValue("en", out name);
            if (!localized_card_descriptions.TryGetValue(locale, out description))
                localized_card_descriptions.TryGetValue("en", out description);
            if (!localized_card_a_descriptions.TryGetValue(locale, out descriptionA))
                localized_card_a_descriptions.TryGetValue("en", out descriptionA);
            if (!localized_card_b_descriptions.TryGetValue(locale, out descriptionB))
                localized_card_b_descriptions.TryGetValue("en", out descriptionB);
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