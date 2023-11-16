using CobaltCoreModding.Definitions.ModManifests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions
{
    public class DependencyEntry
    {
        public string DependencyName { get; init; }

        public Type DependencyType { get; init; }

        protected DependencyEntry(string dependencyName, Type dependencyType)
        {
            DependencyName = dependencyName;
            DependencyType = dependencyType;
        }
    }

    public class DependencyEntry<T> : DependencyEntry where T : IManifest
    {
        public DependencyEntry(string dependencyName) : base(dependencyName, typeof(T))
        {
        }
    }

}
