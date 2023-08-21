using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IAnimationManifest : IManifest
    {
        /// <summary>
        /// Called by art registry when it times for add extra sprites into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IAnimationRegistry registry);
    }
}