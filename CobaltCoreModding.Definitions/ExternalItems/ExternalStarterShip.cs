using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalStarterShip
    {
        public string GlobalName { get; init; }

        public string ShipGlobalName { get; init; }

        public IEnumerable<ExternalCard> ExternalCards { get; init; }

        public IEnumerable<ExternalArtifact> ExternalArtifacts { get; init; }

        /// <summary>
        /// Card types from CobaltCore Assembly
        /// </summary>
        public IEnumerable<Type> ExtraCardTypes { get; init; }


        /// <summary>
        /// Artifact types from CobaltCore Assembly
        /// </summary>
        public IEnumerable<Type> ExtraArtifactTypes { get; init; }

        public ExternalStarterShip(string globalName, string shipGlobalName, IEnumerable<ExternalCard> externalCards, IEnumerable<ExternalArtifact> externalArtifacts, IEnumerable<Type> extraCardTypes, IEnumerable<Type> extraArtifactTypes)
        {
            GlobalName = globalName;
            ShipGlobalName = shipGlobalName;
            ExternalCards = externalCards.ToArray();
            ExternalArtifacts = externalArtifacts.ToArray();
            ExtraCardTypes = extraCardTypes.ToArray();
            ExtraArtifactTypes = extraArtifactTypes.ToArray();
        }

        public ExternalStarterShip(string globalName, ExternalShip ship_template, IEnumerable<ExternalCard> externalCards, IEnumerable<ExternalArtifact> externalArtifacts, IEnumerable<Type> extraCardTypes, IEnumerable<Type> extraArtifactTypes) : this(globalName, ship_template.GlobalName, externalCards, externalArtifacts, extraCardTypes, extraArtifactTypes)
        {

        }

        private readonly Dictionary<string, string> NameLocalisations = new Dictionary<string, string>();
        private readonly Dictionary<string, string> DescriptionLocalisations = new Dictionary<string, string>();

        public void GetLocalisations(string locale, out string? name, out string? description)
        {
            if (!NameLocalisations.TryGetValue(locale, out name))
                NameLocalisations.TryGetValue("en", out name);
            if (!DescriptionLocalisations.TryGetValue(locale, out description))
                DescriptionLocalisations.TryGetValue("en", out description);
        }

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

    }
}
