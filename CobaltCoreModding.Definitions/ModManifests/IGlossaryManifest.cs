using CobaltCoreModding.Definitions.ModContactPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IGlossaryManifest: IManifest
    {
        /// <summary>
        /// Called by glossary registry when it times for add extra glossary items into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IGlossaryRegisty registry);
    }
}
