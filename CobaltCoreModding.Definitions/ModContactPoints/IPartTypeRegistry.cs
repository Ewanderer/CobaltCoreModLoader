using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IPartTypeRegistry : IPartTypeLookup
    {
        public bool RegisterPartType(ExternalPartType externalPartType);
    }
}
