namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// While characters are closely related to decks 
    /// there is a bit more logic required to make a character.
    /// </summary>
    public class ExternalCharacter
    {

        public ExternalDeck Deck { get; init; }

        public string GlobalName { get; init; }

        public ExternalSprite CharPanelSpr { get; init; }

        public IEnumerable<Type> StarterDeck { get; init; }

        public IEnumerable<Type> StarterArtifacts { get; init; }

        public ExternalCharacter(string globalName, ExternalDeck deck, ExternalSprite charPanelSpr, IEnumerable<Type> starterDeck, IEnumerable<Type> starterArtifacts)
        {
            Deck = deck;
            GlobalName = globalName;
            CharPanelSpr = charPanelSpr;
            StarterDeck = starterDeck.ToArray();
            StarterArtifacts = starterArtifacts.ToArray();
        }

        private Dictionary<string, string> name_localisations { get; init; } = new Dictionary<string, string>();

        public string? GetCharacterName(string loacle)
        {
            if (!name_localisations.TryGetValue(loacle, out string? name))
                if (!name_localisations.TryGetValue("en", out name))
                    return null;
            return name;
        }

        public void AddNameLocalisation(string name, string locale = "en")
        {
            if (!name_localisations.TryAdd(locale, name))
                name_localisations[locale] = name;
        }


    }
}