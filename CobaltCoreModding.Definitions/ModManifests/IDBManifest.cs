using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IDBManifest : IManifest
    {
      

        /// <summary>
        /// What other db manifest should be first loaded before this mod.
        /// </summary>
        IEnumerable<string> Dependencies { get; }

        /// <summary>
        /// Called by art registry when it times for add extra sprites into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IDbRegistry dbRegistry);
    }
}