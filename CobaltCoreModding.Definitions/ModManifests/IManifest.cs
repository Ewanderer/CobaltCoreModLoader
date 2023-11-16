namespace CobaltCoreModding.Definitions.ModManifests
{
    /// <summary>
    /// Mods contain manifests, which the mod loader uses to have their data setup.
    /// this is the parent containing base defentions of manifest such as global name.
    /// </summary>
    public interface IManifest
    {
        /// <summary>
        /// Will be set by the mod loader to help a manifest find its physical ressources.
        /// </summary>
        public DirectoryInfo? ModRootFolder { get; set; }

        /// <summary>
        /// Will be set by the mod loader to help a manifest find the game's root folder
        /// </summary>
        public DirectoryInfo? GameRootFolder { get; set; }

        /// <summary>
        /// The unique modifier of this manifest. must be unique within and across all assemblies for mods.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The mod loader will load all manifests with their name listed here before handling this one at every step.
        /// </summary>
        IEnumerable<DependencyEntry> Dependencies { get; }
    }
}