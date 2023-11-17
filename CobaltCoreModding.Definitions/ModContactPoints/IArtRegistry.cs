using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IArtRegistry : ISpriteLookup
    {
        /// <summary>
        /// Pass any sprite here for registration. it will be assigned an id and return the id.
        /// can also be used to overwrite an existing sprite within the game.
        /// </summary>
        /// <param name="sprite_data">an unregistered sprite</param>
        /// <param name="overwrite_spr">If not null and within the range of ingame sprites, this will be used to overwrite an existing sprite within cobalt core game.</param>
        /// <returns>true if accepted</returns>
        public bool RegisterArt(ExternalSprite sprite_data, int? overwrite_value = null);
    }
}