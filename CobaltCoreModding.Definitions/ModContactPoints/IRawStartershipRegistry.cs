using CobaltCoreModding.Definitions.ItemLookups;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// Allows for raw starter ships to be registered.
    /// </summary>
    public interface IRawStartershipRegistry : IStartershipLookup
    {
        /// <summary>
        /// Add localization to a raw ship
        /// </summary>
        /// <param name="global_name">global name. must be the same as a previously registered raw ship.</param>
        /// <param name="name">Name of the ship</param>
        /// <param name="description">Description of the ship</param>
        /// <param name="locale">Language to add localization for</param>
        void AddRawLocalization(string global_name, string name, string description, string locale = "en");

        bool MakeArtifactExclusive(string shipName, Type artifactType);

        /// <summary>
        /// Register a raw ship.
        /// </summary>
        /// <param name="starterShip">A cobaltcore.startership object</param>
        /// <param name="global_name">global name. must be unique among all raw and external starterships.</param>
        bool RegisterStartership(object starterShip, string global_name);

        bool MakeCardExclusive(string shipName, Type cardType);
    }
}