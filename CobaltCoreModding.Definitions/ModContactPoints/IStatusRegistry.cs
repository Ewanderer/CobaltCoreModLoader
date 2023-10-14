using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IStatusRegistry
    {
        /// <summary>
        /// Registers a status.
        /// Can also used to overwrite a status.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="overwrite_status_id"></param>
        /// <returns></returns>
        public bool RegisterStatus(ExternalStatus status);
    }
}
