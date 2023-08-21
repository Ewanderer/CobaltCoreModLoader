using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface ICardManifest : IManifest
    {
        /// <summary>
        /// Called by art registry when it times for add extra sprites into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(ICardRegistry registry);
    }
}