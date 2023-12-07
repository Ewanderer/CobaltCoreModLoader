using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalCardMeta
    {
        public string GlobalName { get; init; }
        public bool? DontOffer { get; init; }
        public bool? Unreleased { get; init; }
        public bool? DontLoc { get; init; }
        public bool? WeirdCard { get; init; }
        public int[]? UpgradesTo { get; init; }
        public int? Deck { get; init; }
        public int? Rarity { get; init; }
        public string[]? ExtraGlossary { get; init; }

        public ExternalCardMeta(string globalName, bool? dontOffer = null, bool? unreleased = null, bool? dontLoc = null, bool? weirdCard = null, int? deck = null, int? rarity = null, int[]? upgradesTo = null, string[]? extraGlossary = null)
        {
            GlobalName = globalName;
            DontOffer = dontOffer;
            Unreleased = unreleased;
            DontLoc = dontLoc;
            WeirdCard = weirdCard;
            UpgradesTo = upgradesTo;
            Deck = deck;
            Rarity = rarity;
            ExtraGlossary = extraGlossary;
        }
    }
}
