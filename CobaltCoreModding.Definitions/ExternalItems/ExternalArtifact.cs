namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalArtifact
    {

        public Type ArtifactType { get; set; }

        public string GlobalName { get; init; }

        public ExternalSprite Sprite { get; init; }

        /// <summary>
        /// Used to help with articact meta, because custom decks cannot be recognized in the meta attribute.
        /// </summary>
        public ExternalDeck? OwnerDeck { get; init; }

        /// <summary>
        /// Used to extend artifact meta, to help with tooltips. though just using extra tooltips should do the trick.
        /// </summary>
        public IEnumerable<ExternalGlossary> ExtraGlossary { get; init; }

        public ExternalArtifact(Type artifactType, string globalName, ExternalSprite sprite, ExternalDeck? ownerDeck, IEnumerable<ExternalGlossary> extraGlossary)
        {
            ArtifactType = artifactType;
            GlobalName = globalName;
            Sprite = sprite;
            OwnerDeck = ownerDeck;
            ExtraGlossary = extraGlossary.ToArray();
        }

        private Dictionary<string, Tuple<string, string>> Localisations { get; init; } = new();

        public void AddLocalisation(string locale, string name, string description)
        {
            Tuple<string, string> tuple = new(name, description);
            if (!Localisations.TryAdd(locale, tuple))
                Localisations[locale] = tuple;
        }


        public bool GetLocalisation(string locale, out string name, out string description)
        {
            name = "";
            description = "";
            if (Localisations.TryGetValue(locale, out var tuple))
            {
                name = tuple.Item1;
                description = tuple.Item2;
                return true;
            }
            return false;
        }
    }
}