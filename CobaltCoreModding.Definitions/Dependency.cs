using CobaltCoreModding.Definitions.ModManifests;

namespace CobaltCoreModding.Definitions
{
    /// <summary>
    /// Dependencies are used by manifests to siganl the loader, what over manifests need to be loaded before them.
    /// </summary>
    public class DependencyEntry
    {
        /// <summary>
        /// Closed constructor to prevent missconstruction, use DependencyEntry{T} for construction.
        /// </summary>
        /// <param name="dependencyName"></param>
        /// <param name="dependencyType"></param>
        /// <see cref="DependencyEntry{T}"/>
        protected DependencyEntry(string dependencyName, Type dependencyType)
        {
            DependencyName = dependencyName;
            DependencyType = dependencyType;
        }

        /// <summary>
        /// The name of the manifest.
        /// </summary>
        public string DependencyName { get; init; }

        /// <summary>
        /// What part of a manifest need to be loaded first. 
        /// In case a manifest for example implements Sprites, Artifacts, Events, etc, but our dependeny is only on an artifact.
        /// </summary>
        public Type DependencyType { get; init; }
    }

    /// <summary>
    /// To protect against errors while creating dependencies, this wrapper ensures proper construction of an dependency entry.
    /// </summary>
    /// <typeparam name="T">One of the manifest types in mod manifests. any other dependency will not work</typeparam>
    public class DependencyEntry<T> : DependencyEntry where T : IManifest
    {
        /// <summary>
        /// A raw constructor
        /// </summary>
        /// <param name="dependencyName">the name of the dependency manifest.</param>
        public DependencyEntry(string dependencyName) : base(dependencyName, typeof(T))
        {
        }

        /// <summary>
        /// For even more stricter control use this
        /// </summary>
        /// <param name="manifest">the manifest to which we are dependent.</param>
        public DependencyEntry(T manifest) : base(manifest.Name, typeof(T))
        {
        }
    }
}