using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IAnimationRegistry : IAnimationLookup
    {
        bool RegisterAnimation(ExternalAnimation animation);
    }
}