namespace CobaltCoreModding.Definitions.ExternalItems
{
    /// <summary>
    /// A container for sprite registry
    /// </summary>
    public class ExternalSprite
    {
        public FileInfo? physical_location;

        public Func<Stream>? virtual_location;

        private int? id;

        /// <summary>
        /// Path to the sprite file on disc to be used.
        /// </summary>
        /// <param name="physical_location"></param>
        public ExternalSprite(string global_name, FileInfo physical_location)
        {
            this.physical_location = physical_location;
            this.GlobalName = global_name;
        }

        /// <summary>
        /// For funky people using embedded resources for their mods rather than physical storage.
        /// or who want to dynamically generate texture for whatever reason.
        /// </summary>
        /// <param name="virtual_location"></param>
        public ExternalSprite(string global_name, Func<Stream> virtual_location)
        {
            this.virtual_location = virtual_location;
            this.GlobalName = global_name;
        }

        protected ExternalSprite(string globalName)
        {
            GlobalName = globalName;
        }

        private ExternalSprite(int id)
        {
            this.id = id;
            GlobalName = "";
        }

        /// <summary>
        /// This must be a unique name across all mods to be used for cross mod sprite usage.
        /// </summary>
        public string GlobalName { get; init; }

        /// <summary>
        /// The Spr value assigned to this Sprite. By the time any other external ressource needs this value, it will already be assigned aka not null
        /// </summary>
        public int? Id
        {
            get => id; set
            {
                if (id != null)
                    throw new InvalidOperationException("This ExternalSprite was already registered");
                id = value;
            }
        }

        /// <summary>
        /// When overriden an external sprite, this flag can be used to indicate that a GetTexture()
        /// function intends to change its output and thus need to be called everytime.
        /// </summary>
        public virtual bool IsCaching => true;

        /// <summary>
        /// This function is used by mod loader to create an art asset with an id of an cobalt core spr value.
        /// mods should not use this unless they feed it direct Spr values.
        /// cannot be registered in the art registry but otherwise be used.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ExternalSprite GetRaw(int id)
        {
            return new ExternalSprite(id);
        }

        /// <summary>
        /// for mods way out there they are free to create their own texture2d object and feed it here. will only be used if both location entries are null.
        /// </summary>
        /// <returns>A texture2d object</returns>
        public virtual object? GetTexture()
        {
            return null;
        }
    }
}