using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IShipPartRegistry : IPartLookup
    {
        /// <summary>
        /// Attempts to register an external part.
        /// will reject if not well configured or global name already used.
        /// </summary>
        /// <param name="externalPart"></param>
        /// <returns>true if successfully registered.</returns>
        bool RegisterPart(ExternalPart externalPart);

        /// <summary>
        /// Register a raw part, for use in a raw ship.
        /// </summary>
        /// <param name="global_name">Name for the chassis. must be unique for all chassis. It will be registered under @mod_extra_part:global_name to safeguard against conflicts.</param>
        /// <param name="spr_value">A Spr value or ExternalSprite.Id.</param>
        /// <param name="off_spr_value">An option value for offPart version</param>
        /// <returns>if was successful</returns>
        bool RegisterRawPart(string global_name, int spr_value, int? off_spr_value = null);
    }
}