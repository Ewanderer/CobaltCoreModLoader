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
        [Obsolete("Use the 3-arg overload instead.")]
        protected DependencyEntry(string dependencyName, Type dependencyType) : this(dependencyName, dependencyType, false)
        {
        }

        /// <summary>
        /// Closed constructor to prevent missconstruction, use DependencyEntry{T} for construction.
        /// </summary>
        /// <param name="dependencyName"></param>
        /// <param name="dependencyType"></param>
        /// <param name="ignoreIfMissing"></param>
        /// <see cref="DependencyEntry{T}"/>
        protected DependencyEntry(string dependencyName, Type dependencyType, bool ignoreIfMissing = false)
        {
            DependencyName = dependencyName;
            DependencyType = dependencyType;
            IgnoreIfMissing = ignoreIfMissing;
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

        public bool IgnoreIfMissing { get; init; }
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
        [Obsolete("Use the 2-arg overload instead.")]
        public DependencyEntry(string dependencyName) : this(dependencyName, false)
        {
        }

        /// <summary>
        /// A raw constructor
        /// </summary>
        /// <param name="dependencyName">the name of the dependency manifest.</param>
        /// <param name="ignoreIfMissing">If <c>true</c>, the mod will continue to load if the dependency is missing</param>
        public DependencyEntry(string dependencyName, bool ignoreIfMissing = false) : base(dependencyName, typeof(T), ignoreIfMissing)
        {
        }

        /// <summary>
        /// For even more stricter control use this
        /// </summary>
        /// <param name="manifest">the manifest to which we are dependent.</param>
        [Obsolete("Use the 2-arg overload instead.")]
        public DependencyEntry(T manifest) : this(manifest, false)
        {
        }


        /// <summary>
        /// For even more stricter control use this
        /// </summary>
        /// <param name="manifest">the manifest to which we are dependent.</param>
        /// <param name="ignoreIfMissing">If <c>true</c>, the mod will continue to load if the dependency is missing</param>
        public DependencyEntry(T manifest, bool ignoreIfMissing = false) : base(manifest.Name, typeof(T), ignoreIfMissing)
        {
        }
    }
}