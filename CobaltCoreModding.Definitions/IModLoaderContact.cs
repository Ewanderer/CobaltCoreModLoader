using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions
{
    /// <summary>
    /// This interface serves as the contact point between any services of the 
    /// mod loader and a mod and will be passed during its bootup.
    /// Examples are: Adding additional localisations, graphics and other ressources not covered by the mod loader magic (such as loading cards which is done straight from assembly)
    /// </summary>
    public interface IModLoaderContact
    {
    }
}
