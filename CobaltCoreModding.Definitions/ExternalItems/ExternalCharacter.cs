namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// While characters are closely related to decks
    /// there is a bit more logic required to make a character.
    /// </summary>
    public class ExternalCharacter
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="globalName"></param>
        /// <param name="deck"></param>
        /// <param name="charPanelSpr"></param>
        /// <param name="starterDeck">types of card this character starts with</param>
        /// <param name="starterArtifacts">types of artifacts this character starts with</param>
        /// <param name="neutralAnimation">the default animation. will only be validated and not actually stored</param>
        /// <param name="miniAnimation">the mini potrait animation. will only be validated and not actually stored</param>
        public ExternalCharacter(string globalName, ExternalDeck deck, ExternalSprite charPanelSpr, IEnumerable<Type> starterDeck, IEnumerable<Type> starterArtifacts, ExternalAnimation neutralAnimation, ExternalAnimation miniAnimation)
        {
            Deck = deck;
            GlobalName = globalName;
            CharPanelSpr = charPanelSpr;
            StarterDeck = starterDeck.ToArray();
            StarterArtifacts = starterArtifacts.ToArray();

            if (neutralAnimation.Deck.Id != deck.Id || deck.Id == null)
                throw new ArgumentException("default animation deck doesn't match or no deck with null for id.");
            if (miniAnimation.Deck.Id != deck.Id || deck.Id == null)
                throw new ArgumentException("mini animation deck doesn't match or no deck with null for id.");

            if (neutralAnimation.Tag != "neutral")
                throw new ArgumentException("default animation must have \"neutral\" tag");
            if (miniAnimation.Tag != "mini")
                throw new ArgumentException("default animation must have \"mini\" tag");
        }

        public ExternalSprite CharPanelSpr { get; init; }
        public ExternalDeck Deck { get; init; }

        public string GlobalName { get; init; }
        public IEnumerable<Type> StarterArtifacts { get; init; }
        public IEnumerable<Type> StarterDeck { get; init; }
        private Dictionary<string, string> desc_localisations { get; init; } = new Dictionary<string, string>();
        private Dictionary<string, string> name_localisations { get; init; } = new Dictionary<string, string>();

        public void AddDescLocalisation(string desc, string locale = "en")
        {
            if (!desc_localisations.TryAdd(locale, desc))
                desc_localisations[locale] = desc;
        }

        public void AddNameLocalisation(string name, string locale = "en")
        {
            if (!name_localisations.TryAdd(locale, name))
                name_localisations[locale] = name;
        }

        public string? GetCharacterName(string loacle)
        {
            if (!name_localisations.TryGetValue(loacle, out string? name))
                if (!name_localisations.TryGetValue("en", out name))
                    return null;
            return name;
        }

        public string? GetDesc(string loacle)
        {
            if (!desc_localisations.TryGetValue(loacle, out string? name))
                if (!desc_localisations.TryGetValue("en", out name))
                    return null;
            return name;
        }
    }
}