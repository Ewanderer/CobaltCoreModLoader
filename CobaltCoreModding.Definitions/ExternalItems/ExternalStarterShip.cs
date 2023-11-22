namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalStarterShip
    {
        private readonly Dictionary<string, string> DescriptionLocalisations = new Dictionary<string, string>();
        private readonly Dictionary<string, string> NameLocalisations = new Dictionary<string, string>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="globalName">Unique identifying name of the starter ship</param>
        /// <param name="shipGlobalName"></param>
        /// <param name="startingCards">starting cards</param>
        /// <param name="startingArtifacts">starting artifacts</param>
        /// <param name="nativeStartingCards"></param>
        /// <param name="nativeStartingArtifacts"></param>
        /// <param name="exclusiveArtifacts"></param>
        /// <param name="exclusiveNativeArtifacts"></param>
        public ExternalStarterShip(string globalName,
            string shipGlobalName,
            IEnumerable<ExternalCard>? startingCards = null,
            IEnumerable<ExternalArtifact>? startingArtifacts = null,
            IEnumerable<Type>? nativeStartingCards = null,
            IEnumerable<Type>? nativeStartingArtifacts = null,
            IEnumerable<ExternalArtifact>? exclusiveArtifacts = null,
            IEnumerable<Type>? exclusiveNativeArtifacts = null)
        {
            GlobalName = globalName;
            ShipGlobalName = shipGlobalName;
            StartingCards = startingCards?.ToArray() ?? new ExternalCard[0];
            StartingArtifacts = startingArtifacts?.ToArray() ?? new ExternalArtifact[0];
            NativeStartingCards = nativeStartingCards?.ToArray() ?? new Type[0];
            NativeStartingArtifact = nativeStartingArtifacts?.ToArray() ?? new Type[0];
            ExclusiveArtifacts = exclusiveArtifacts?.ToArray() ?? new ExternalArtifact[0];
            ExclusiveNativeArtifacts = exclusiveNativeArtifacts?.ToArray() ?? new Type[0];
        }

        public ExternalStarterShip(string globalName,
            ExternalShip ship_template,
            IEnumerable<ExternalCard>? startingCards = null,
            IEnumerable<ExternalArtifact>? startingArtifacts = null,
            IEnumerable<Type>? nativeStartingCards = null,
            IEnumerable<Type>? nativeStartingArtifacts = null,
            IEnumerable<ExternalArtifact>? exclusiveArtifacts = null,
            IEnumerable<Type>? exclusiveNativeArtifacts = null) :
            this(globalName, ship_template.GlobalName, startingCards, startingArtifacts, nativeStartingCards, nativeStartingArtifacts, exclusiveArtifacts, exclusiveNativeArtifacts)
        {
        }

        /// <summary>
        /// All artifacts added here will only show if this ship has been selected.
        /// If an artifact was made exclusive to multiple ships, don't worry, they will be avaialbe to all of them.
        /// </summary>
        public IEnumerable<ExternalArtifact> ExclusiveArtifacts { get; init; }

        /// <summary>
        /// Raw cobalt core artifacts that will be added ot the ships reward pool even if they are exclusive to a native cobalt core ship.
        /// </summary>
        public IEnumerable<Type> ExclusiveNativeArtifacts { get; init; }

        public string GlobalName { get; init; }

        /// <summary>
        /// Artifact types from CobaltCore Assembly
        /// </summary>
        public IEnumerable<Type> NativeStartingArtifact { get; init; }

        /// <summary>
        /// Card types from CobaltCore Assembly
        /// </summary>
        public IEnumerable<Type> NativeStartingCards { get; init; }

        public string ShipGlobalName { get; init; }
        public IEnumerable<ExternalArtifact> StartingArtifacts { get; init; }
        public IEnumerable<ExternalCard> StartingCards { get; init; }

        public void AddLocalisation(string name, string description, string locale = "en")
        {
            if (NameLocalisations.ContainsKey(locale))
            {
                NameLocalisations[locale] = name;
            }
            else
            {
                NameLocalisations.Add(locale, name);
            }

            if (DescriptionLocalisations.ContainsKey(locale))
            {
                DescriptionLocalisations[locale] = description;
            }
            else
            {
                DescriptionLocalisations.Add(locale, description);
            }
        }

        public void GetLocalisations(string locale, out string? name, out string? description)
        {
            if (!NameLocalisations.TryGetValue(locale, out name))
                NameLocalisations.TryGetValue("en", out name);
            if (!DescriptionLocalisations.TryGetValue(locale, out description))
                DescriptionLocalisations.TryGetValue("en", out description);
        }
    }
}