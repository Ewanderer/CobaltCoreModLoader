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
    }
}
