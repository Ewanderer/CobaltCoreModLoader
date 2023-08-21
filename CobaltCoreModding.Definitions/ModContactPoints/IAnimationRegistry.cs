using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IAnimationRegistry
    {
        bool RegisterAnimation(ExternalAnimation animation);
    }
}