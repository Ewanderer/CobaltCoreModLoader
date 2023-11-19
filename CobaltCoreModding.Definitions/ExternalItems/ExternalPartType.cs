namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalPartType
    {
        private readonly Dictionary<string, string> desc_localisations = new Dictionary<string, string>();
        private readonly Dictionary<string, string> name_localisations = new Dictionary<string, string>();
        private int? id;

        public ExternalPartType(string globalName)
        {
            GlobalName = globalName;
        }

        public string GlobalName { get; init; }

        public int? Id
        { get => id; set { if (id != null) throw new Exception("Id already assigned!"); id = value; } }

        public void AddLocalisation(string name, string description, string locale = "en")
        {
            if (!name_localisations.TryAdd(locale, name))
            {
                name_localisations[locale] = name;
            }
            if (!desc_localisations.TryAdd(locale, description))
            {
                desc_localisations[locale] = description;
            }
        }

        public void GetLocalisation(string locale, out string? name, out string? description)
        {
            if (!name_localisations.TryGetValue(locale, out name))
                name_localisations.TryGetValue("en", out name);
            if (!desc_localisations.TryGetValue(locale, out description))
                desc_localisations.TryGetValue("en", out description);
        }
    }
}