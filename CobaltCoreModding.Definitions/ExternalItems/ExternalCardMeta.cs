using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// Used to overwrite a card meta.
    /// only assign values that actually should be changed.y
    /// </summary>
    public class ExternalCardMeta
    {
        public string GlobalName { get; init; }

        public bool? DontOffer { get; init; }

        public bool? Unreleased { get; init; }

        public bool? DontLoc { get; init; }

        public ExternalDeck? Deck { get; init; }

        public int[]? UpgradesTo { get; init; }

        public int? Rarity { get; init; }

        public string[]? ExtraGlossary { get; init; }

        public bool? WeirdCard { get; init; }

        public ExternalCardMeta(string globalName)
        {
            if (string.IsNullOrWhiteSpace(globalName))
                throw new ArgumentNullException(nameof(globalName));
            GlobalName = globalName;
        }
    }
}
