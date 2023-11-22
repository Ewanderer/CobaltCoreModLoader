using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IPartTypeManifest : IManifest
    {
        public void LoadManifest(IPartTypeRegistry partTypeRegistry);
    }
}
