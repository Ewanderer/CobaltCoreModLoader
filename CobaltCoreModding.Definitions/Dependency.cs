using CobaltCoreModding.Definitions.ModManifests;

namespace CobaltCoreModding.Definitions
{
    public class DependencyEntry
    {
        protected DependencyEntry(string dependencyName, Type dependencyType)
        {
            DependencyName = dependencyName;
            DependencyType = dependencyType;
        }

        public string DependencyName { get; init; }

        public Type DependencyType { get; init; }
    }

    public class DependencyEntry<T> : DependencyEntry where T : IManifest
    {
        public DependencyEntry(string dependencyName) : base(dependencyName, typeof(T))
        {
        }
    }
}