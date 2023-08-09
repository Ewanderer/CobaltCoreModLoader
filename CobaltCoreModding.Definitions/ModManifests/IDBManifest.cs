using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IDBManifest
    {
        /// <summary>
        /// The unique modifier of this manifest. can be shared by other manifest types
        /// in the same mod as long as this is the only db manifest.
        /// </summary>
        string Name { get; }

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