using CobaltCoreModding.Definitions.ModContactPoints;

namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IGlossaryManifest : IManifest
    {
        /// <summary>
        /// Called by glossary registry when it times for add extra glossary items into the system.
        /// </summary>
        /// <param name="artRegistry"></param>
        public void LoadManifest(IGlossaryRegisty registry);
    }
}