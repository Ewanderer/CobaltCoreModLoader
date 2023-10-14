using System.Text.RegularExpressions;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalGlossary
    {
        private static readonly Regex regex = new Regex("^[a-zA-Z0-9]+$");

        private readonly Dictionary<string, Tuple<string, string, string?>> localisations = new Dictionary<string, Tuple<string, string, string?>>();

        public ExternalGlossary(string globalName, string itemName, bool intendedOverwrite, GlossayType type, ExternalSprite icon)
        {
            if (icon.Id == null)
                throw new ArgumentException("Icon sprite has no id.");

            GlobalName = globalName;
            Icon = icon;

            //ensure item name doesn't break anything.
            if (!regex.IsMatch(itemName))
            {
                throw new ArgumentException("itemName must be non empgty and alphanumerical to not break parsing!");
            }

            ItemName = itemName;
            IntendedOverwrite = intendedOverwrite;
            Type = type;
        }

        public enum GlossayType
        {
            midrow,

            //     status,
            cardtrait,

            action,
            parttrait,
            destination,
            actionMisc,
            part,
            env,
        }

        public string GlobalName { get; init; }

        public string Head => Enum.GetName<GlossayType>(Type) + "." + ItemName;
        public ExternalSprite Icon { get; init; }
        public bool IntendedOverwrite { get; init; }
        public string ItemName { get; init; }
        public GlossayType Type { get; init; }

        public void AddLocalisation(string locale, string name, string desc, string? altDesc = null)
        {
            Tuple<string, string, string?> tuple = new(name, desc, altDesc);
            if (!localisations.TryAdd(locale, tuple))
                localisations[locale] = tuple;
        }

        public bool GetLocalisation(string locale, out string name, out string desc, out string? altDesc)
        {
            name = "";
            desc = "";
            altDesc = null;
            if (localisations.TryGetValue(locale, out var texts))
            {
                name = texts.Item1;
                desc = texts.Item2;
                altDesc = texts.Item3;
                return true;
            }
            return false;
        }
    }
}