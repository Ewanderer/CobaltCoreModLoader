using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IStatusRegistry : IStatusLookup
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