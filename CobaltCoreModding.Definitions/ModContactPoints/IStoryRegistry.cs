using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IStoryRegistry
    {
        public bool RegisterStory(ExternalStory story);
        public bool RegisterChoice(string key, MethodInfo choice, bool intendedOverride = false);
        public bool RegisterCommand(string key, MethodInfo command, bool intendedOverride = false);
        public bool RegisterInjector(ExternalStoryInjector injector);
    }
}
