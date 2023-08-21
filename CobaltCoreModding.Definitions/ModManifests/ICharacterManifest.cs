using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface ICharacterManifest : IManifest
    {
        /// <summary>
        /// Called by art registry when it times for add extra sprites into the system.
        /// </summary>
        /// <param name="registry"></param>
        public void LoadManifest(ICharacterRegistry registry);
    }
}