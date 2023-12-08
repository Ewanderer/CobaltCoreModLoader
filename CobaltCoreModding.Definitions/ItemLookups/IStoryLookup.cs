using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IStoryLookup
    {
        public object LookupStory(string globalName);
    }
}
