namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// Allows for raw starter ships to be registered.
    /// </summary>
    public interface IRawStartershipRegistry
    {
        /// <summary>
        /// Register a raw ship.
        /// </summary>
        /// <param name="starterShip">A cobaltcore.startership object</param>
        /// <param name="global_name">global name. must be unique among all raw and external starterships.</param>
        bool RegisterStartership(object starterShip, string global_name);
    }
}