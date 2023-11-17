using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IDeckLookup : ISpriteLookup, IManifestLookup, ICobaltCoreLookup
    {
        ExternalDeck LookupDeck(string globalName);
    }
}