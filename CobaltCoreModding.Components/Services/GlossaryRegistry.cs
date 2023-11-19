using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    public class GlossaryRegistry : IGlossaryRegisty
    {
        private static readonly Dictionary<string, ExternalGlossary> registered_glossary = new Dictionary<string, ExternalGlossary>();
        private static ILogger<IGlossaryRegisty>? Logger;
        private readonly ModAssemblyHandler modAssemblyHandler;

        public GlossaryRegistry(ILogger<IGlossaryRegisty> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            modAssemblyHandler = mah;
            Logger = logger;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalGlossary? LookupGlossary(string globalName)
        {
            if (!registered_glossary.TryGetValue(globalName, out var glossary))
                Logger?.LogWarning("ExternalGlossary {0} not found.", globalName);
            return glossary;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.GlossaryManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by GlossaryRegistry");
                }
            }
        }

        ExternalGlossary IGlossaryLookup.LookupGlossary(string globalName)
        {
            return LookupGlossary(globalName) ?? throw new KeyNotFoundException();
        }

        public IManifest LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        public ExternalSprite LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
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
    }
}