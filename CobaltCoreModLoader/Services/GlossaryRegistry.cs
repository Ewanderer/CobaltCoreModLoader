using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModLoader.Utils;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CobaltCoreModLoader.Services
{
    public class GlossaryRegistry : IGlossaryRegisty
    {
        private static readonly Dictionary<string, ExternalGlossary> registered_glossary = new Dictionary<string, ExternalGlossary>();
        private static ILogger<IGlossaryRegisty>? Logger;

        public GlossaryRegistry(ILogger<IGlossaryRegisty> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
        }

        bool IGlossaryRegisty.RegisterGlossary(ExternalGlossary glossary)
        {
            if (string.IsNullOrEmpty(glossary.Head) || string.IsNullOrEmpty(glossary.GlobalName) || !Enum.IsDefined<ExternalGlossary.GlossayType>(glossary.Type))
                return false;

            if (!registered_glossary.TryAdd(glossary.ItemName, glossary))
            {
                Logger?.LogWarning("Global Name {0} already known", glossary.GlobalName);
                return false;
            }

            return true;
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> localisation_dictionary)
        {
            foreach (var glossary in registered_glossary.Values)
            {
                if (!glossary.GetLocalisation(locale, out string name, out string desc, out string? altDesc))
                {
                    Logger?.LogWarning("Missing localisation {0} in glosaary {1}", locale, glossary.GlobalName);
                    continue;
                }
                var path_name = glossary.Head + ".name";
                var path_desc = glossary.Head + ".desc";
                var path_altDesc = glossary.Head + ".altDesc";
                if (!glossary.IntendedOverwrite && (localisation_dictionary.ContainsKey(path_name) || localisation_dictionary.ContainsKey(path_desc) || localisation_dictionary.ContainsKey(path_altDesc)))
                {
                    Logger?.LogWarning("Unintended overwrite in localisation directoy for glossary {0} with head {1}, skipping...", glossary.GlobalName, glossary.ItemName);
                    continue;
                }
                localisation_dictionary.Add(path_name, name);
                localisation_dictionary.Add(path_desc, desc);
                if (altDesc != null)
                {
                    localisation_dictionary.Add(path_altDesc, altDesc);
                }
            }
        }

        internal static void PathIconSprites()
        {
            IDictionary icon_dict = TypesAndEnums.DbType.GetField("icons", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new Exception("Icon Dictionary not found in DB.");
            foreach (var glossary in registered_glossary.Values)
            {
                var sprite = TypesAndEnums.IntToSpr(glossary.Icon.Id);
                if (sprite == null)
                {
                    Logger?.LogWarning("Glossary {0} Icon Spr Id {1} cannot be converted to Spr object", glossary.GlobalName, glossary.Icon.Id);
                    continue;
                }

                if (glossary.IntendedOverwrite)
                {
                    if (icon_dict.Contains(glossary.ItemName))
                    {
                        icon_dict[glossary.ItemName] = sprite;
                    }
                    else
                    {
                        icon_dict.Add(glossary.ItemName, sprite);
                    }
                }
                else
                {
                    if (icon_dict.Contains(glossary.ItemName))
                    {
                        Logger?.LogWarning("Glossary {0} unintended overwrite in icon registy, skiping", glossary.GlobalName);
                        continue;
                    }
                    else
                    {
                        icon_dict.Add(glossary.ItemName, sprite);
                    }
                }
            }
        }

        internal void LoadManifests()
        {
            foreach (var manifest in ModAssemblyHandler.GlossaryManifests)
            {
                manifest.LoadManifest(this);
            }
        }
    }
}