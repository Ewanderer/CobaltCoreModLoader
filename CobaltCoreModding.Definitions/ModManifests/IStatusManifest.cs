using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IStatusManifest : IManifest
    {
        /// <summary>
        /// Called by status registry when it times for add extra statuses into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IStatusRegistry statusRegistry);
    }
}