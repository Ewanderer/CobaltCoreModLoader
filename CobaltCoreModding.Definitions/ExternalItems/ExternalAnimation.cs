namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalAnimation
    {
        public string GlobalName { get; init; }

        public ExternalDeck Deck { get; init; }

        public string Tag { get; init; }

        public IEnumerable<ExternalSprite> Frames { get; init; }

        public bool IntendedOverwrite { get; set; }

        public ExternalAnimation(string globalNamey, ExternalDeck deck, string tag, bool intendedOverwrite, IEnumerable<ExternalSprite> frames)
        {
            GlobalName = globalNamey;
            Deck = deck;
            Tag = tag.ToLower();
            if (!frames.Any())
                throw new ArgumentException("Animation needs at least one frame.");
            Frames = frames.ToArray();
            IntendedOverwrite = intendedOverwrite;
        }
    }
}