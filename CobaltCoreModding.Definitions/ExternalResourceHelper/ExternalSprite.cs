namespace CobaltCoreModding.Definitions.ExternalResourceHelper
{
    /// <summary>
    /// A container for sprite registry
    /// </summary>
    public class ExternalSprite
    {
        private int? id;

        public int? Id
        {
            get => id; set
            {
                if (id != null)
                    throw new InvalidOperationException("This ExternalSprite was already registered");
                id = value;
            }
        }

        public FileInfo? physical_location;
        public Func<Stream>? virtual_location;

        /// <summary>
        /// for mods way out there they are free to create their own texture2d object and feed it here. will only be used if both location entries are null.
        /// </summary>
        /// <returns>A texture2d object</returns>
        public virtual object? GetTexture()
        {
            return null;
        }

        /// <summary>
        /// Path to the sprite file on disc to be used.
        /// </summary>
        /// <param name="physical_location"></param>
        public ExternalSprite(FileInfo physical_location)
        {
            this.physical_location = physical_location;
        }

        /// <summary>
        /// For funky people using embedded resources for their mods rather than physical storage.
        /// or who want to dynamically generate texture for whatever reason.
        /// </summary>
        /// <param name="virtual_location"></param>
        public ExternalSprite(Func<Stream> virtual_location)
        {
            this.virtual_location = virtual_location;
        }
    }
}