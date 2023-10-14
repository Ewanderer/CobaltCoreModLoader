using System.ComponentModel;
using System.Drawing;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalStatus
    {

        private int? id;

        public int? Id
        {
            get => id; set
            {
                if (id != null)
                    throw new InvalidOperationException("Object has already id assigned!");

                id = value ?? throw new ArgumentException("Cannot assign null to this field!");
            }
        }

        private readonly Dictionary<string, string> name_localisations = new Dictionary<string, string>();

        private readonly Dictionary<string, string> desc_localisations = new Dictionary<string, string>();

        public void AddLocalisation(string name, string desc, string localisation = "en")
        {
            if (!name_localisations.TryAdd(localisation, name))
                name_localisations[localisation] = name;
            if (!desc_localisations.TryAdd(localisation, desc))
                desc_localisations[localisation] = desc;
        }

        public void GetLocalisation(string localisation, out string? name, out string? desc) {
            if (!name_localisations.TryGetValue(localisation, out name))
                name_localisations.TryGetValue("en", out name);
            if (!desc_localisations.TryGetValue(localisation, out desc))
                desc_localisations.TryGetValue("en", out desc);
        }

        public string GlobalName { get; init; }

        public bool IsGood { get; init; }

        public Color MainColor { get; init; }

        public Color? BorderColor { get; init; }

        private ExternalSprite? icon { get; init; }

        public ExternalSprite Icon => icon ?? throw new InvalidEnumArgumentException("Status was never setup with a sprite.");

        public bool AffectedByTimestop { get; init; }

        public ExternalStatus(string globalName, bool isGood, Color mainColor, Color? borderColor, ExternalSprite icon, bool affectedByTimestop)
        {
            GlobalName = globalName;
            IsGood = isGood;
            MainColor = mainColor;
            BorderColor = borderColor;
            this.icon = icon;
            AffectedByTimestop = affectedByTimestop;
        }

        private ExternalStatus(int id)
        {
            this.id = id;
            GlobalName = "";
        }

        public static ExternalStatus GetRaw(int id)
        {
            return new ExternalStatus(id);
        }

    }
}