using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// Allows for raw ships to be registered.
    /// they can even have external parts referenced, since these manifest are only loaded after all patching has been done during game startup.
    /// if you need custom chassis, well register chassis should be called first.
    /// </summary>
    public interface IRawShipRegistry : IShipLookup
    {
        /// <summary>
        /// Register a raw ship.
        /// </summary>
        /// <param name="ship">A cobaltcore.ship object</param>
        /// <param name="global_name">global name. must be unique among all raw and external ships.</param>
        /// <param name="playable">check to add to start ships</param>
        /// <param name="overwrite_starter_deck">check to supress starter cards from being assigned player. not working right now.</param>
        /// <returns></returns>
        bool RegisterShip(object ship, string global_name);
    }
}