namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalArtifact
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="globalName">The global name of the artifact for cross referencing. must be unique among all externalartifacts</param>
        /// <param name="artifactType">a class of the artifact type containing the logic</param>
        /// <param name="sprite">the sprite used to represent the artifact</param>
        /// <param name="extraGlossary"></param>
        /// <param name="ownerDeck">use in case you need a owner not natively in the deck enum</param>
        /// <param name="exclusiveToNativeParts">a collect of native PType to which this artifact is excluse. example an artifact exclusive to wings. for custom parts set in externalpart.</param>
        /// <param name="exclusiveToShips">put ship.key values here to make artifact only availabe if that ship is selected. for custom ships use the raw starter ship registry / externalstarter ship properties</param>
        public ExternalArtifact(string globalName,
                                Type artifactType,
                                ExternalSprite sprite,
                                IEnumerable<ExternalGlossary>? extraGlossary = null,
                                ExternalDeck? ownerDeck = null,
                                IEnumerable<int>? exclusiveToNativeParts = null,
                                IEnumerable<string>? exclusiveToShips = null)
        {
            ArtifactType = artifactType;
            GlobalName = globalName;
            Sprite = sprite;
            OwnerDeck = ownerDeck;
            ExtraGlossary = extraGlossary?.ToArray() ?? Array.Empty<ExternalGlossary>();
            ExclusiveToNativeParts = exclusiveToNativeParts?.ToArray() ?? Array.Empty<int>();
            ExclusiveToShips = exclusiveToShips?.ToArray() ?? Array.Empty<string>();
        }

        public Type ArtifactType { get; init; }
        public IEnumerable<int> ExclusiveToNativeParts { get; init; }
        public IEnumerable<string> ExclusiveToShips { get; init; }

        /// <summary>
        /// Used to extend artifact meta, to help with tooltips. though just using extra tooltips should do the trick.
        /// </summary>
        public IEnumerable<ExternalGlossary> ExtraGlossary { get; init; }

        public string GlobalName { get; init; }

        /// <summary>
        /// Used to help with articact meta, because custom decks cannot be recognized in the meta attribute.
        /// </summary>
        public ExternalDeck? OwnerDeck { get; init; }

        public ExternalSprite Sprite { get; init; }
        private Dictionary<string, Tuple<string, string>> Localisations { get; init; } = new();

        public void AddLocalisation(string name, string description, string locale="en")
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