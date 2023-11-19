using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CobaltCoreModding.Components.Services
{
    public class StatusRegistry : IStatusRegistry
    {
        private const int status_id_counter_start = 1000000;
        private static readonly Dictionary<int, object> icon_lookup = new Dictionary<int, object>();
        private static readonly Dictionary<string, ExternalStatus> total_lookup = new Dictionary<string, ExternalStatus>();
        private static Type? buildiconandtext_return_type;
        private static ILogger<StatusRegistry>? logger;
        private static int status_id_counter = status_id_counter_start;
        private static FieldInfo? tt_glossary_key_field;
        private readonly ModAssemblyHandler modAssemblyHandler;

        public StatusRegistry(ILogger<StatusRegistry>? logger, ModAssemblyHandler mah)
        {
            StatusRegistry.logger = logger;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalStatus? LookupStatus(string globalName)
        {
            if (!total_lookup.TryGetValue(globalName, out var status))
                logger?.LogWarning("ExternalStatus {0} not found", globalName);
            return status;
        }

        public static void PatchStatusData()
        {
            var status_dict = TypesAndEnums.DbType.GetField("statuses")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot find DB.statuses");

            var def_good_field = TypesAndEnums.StatusDefType.GetField("isGood") ?? throw new Exception("StatusDef.isGood field not found");
            var def_color_field = TypesAndEnums.StatusDefType.GetField("color") ?? throw new Exception("StatusDef.color field not found");
            var def_border_field = TypesAndEnums.StatusDefType.GetField("border") ?? throw new Exception("StatusDef.border field not found");
            var def_icon_field = TypesAndEnums.StatusDefType.GetField("icon") ?? throw new Exception("StatusDef.icon field not found");
            var def_timestop_field = TypesAndEnums.StatusDefType.GetField("affectedByTimestop") ?? throw new Exception("StatusDef.affectedByTimestop field not found");

            foreach (var s in total_lookup.Values)
            {
                var status_val = TypesAndEnums.IntToStatus(s.Id) ?? throw new Exception($"Status {s.GlobalName} could not create an status enum val.");
                var status_def = Activator.CreateInstance(TypesAndEnums.StatusDefType);
                def_good_field.SetValue(status_def, s.IsGood);
                var color = Activator.CreateInstance(TypesAndEnums.CobaltColorType, (UInt32)s.MainColor.ToArgb());
                def_color_field.SetValue(status_def, color);
                if (s.BorderColor != null)
                {
                    color = Activator.CreateInstance(TypesAndEnums.CobaltColorType, (UInt32)s.BorderColor.Value.ToArgb());
                    def_border_field.SetValue(status_def, color);
                }
                var spr_val = TypesAndEnums.IntToSpr(s.Icon.Id) ?? throw new Exception($"Status {s.GlobalName} couldn't get Spr for Icon {s.Icon.GlobalName}");
                def_icon_field.SetValue(status_def, spr_val);
                def_timestop_field.SetValue(status_def, s.AffectedByTimestop);
                if (status_dict.Contains(status_val))
                {
                    logger?.LogCritical($"Status {s.GlobalName} couldn't be added to status dictionary, because key {s.Id} already exists.");
                    continue;
                }
                status_dict.Add(status_val, status_def);
            }
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.StatusManifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by StatusRegistry");
                }
            }

            //harmony patching TT glossary to replace status lookup.
            var harmony = new Harmony("modloader.statusregistry.ttglossary");

            tt_glossary_key_field = TypesAndEnums.TTGlossaryType.GetField("key") ?? throw new Exception("TTGlossary.key not found!");

            var tt_glossary_build_icon_and_text_method = TypesAndEnums.TTGlossaryType.GetMethod("BuildIconAndText") ?? throw new Exception("TTGlossary.BuildIconAndText method not found");

            //  buildiconandtext_return_type = typeof(ValueTuple<,>).MakeGenericType(TypesAndEnums.SprType, typeof(string)) ?? throw new Exception("Unable to create buildiconandtext_return_type");
            buildiconandtext_return_type = tt_glossary_build_icon_and_text_method.ReturnType ?? throw new Exception("Unable to create buildiconandtext_return_type");

            var tt_glossary_build_icon_and_text_postfix = typeof(StatusRegistry).GetMethod("Glossary_Postfix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("StatusRegistry.Glossary_Postfix method not found");

            harmony.Patch(tt_glossary_build_icon_and_text_method, postfix: new HarmonyMethod(tt_glossary_build_icon_and_text_postfix));

            /*
             *  var load_strings_for_locale_method = TypesAndEnums.DbType.GetMethod("LoadStringsForLocale", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("load_strings_for_locale_method not found");

            var load_strings_for_locale_postfix = typeof(DBExtender).GetMethod("LoadStringsForLocale_PostFix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("make init queue postfix not found");

            harmony.Patch(load_strings_for_locale_method, postfix: new HarmonyMethod(load_strings_for_locale_postfix));
             */
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalStatus IStatusLookup.LookupStatus(string globalName)
        {
            throw new NotImplementedException();
        }

        public bool RegisterStatus(ExternalStatus status)
        {
            if (status.Id != null)
            {
                logger?.LogCritical("Status {0} already has id.", status.GlobalName);
                return false;
            }
            if (status.Icon.Id == null)
            {
                logger?.LogCritical("Status {0} Icon Sprite {1} has no id registered.", status.GlobalName, status.Icon.GlobalName);
                return false;
            }
            if (string.IsNullOrWhiteSpace(status.GlobalName))
            {
                logger?.LogCritical("Status without global name!");
                return false;
            }

            if (total_lookup.ContainsKey(status.GlobalName))
            {
                logger?.LogCritical("Status with global name {0} already registered!", status.GlobalName);
                return false;
            }

            var spr = TypesAndEnums.IntToSpr(status.Icon.Id);
            if (spr == null)
            {
                logger?.LogCritical("Status {0} couldn't make Spr object from icon {1}.", status.GlobalName, status.Icon.GlobalName);
                return false;
            }

            total_lookup.Add(status.GlobalName, status);
            int id = status_id_counter++;
            status.Id = id;
            icon_lookup.Add(id, spr);
            return true;
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> result)
        {
            foreach (var s in total_lookup.Values)
            {
                if (s.Id == null)
                    continue;
                s.GetLocalisation(locale, out var name, out var desc);

                if (name != null)
                {
                    var key = $"status.{s.Id}.name";
                    if (!result.TryAdd(key, name))
                        logger?.LogWarning("Status {0} cannot register name because key {1} already added somehow", s.GlobalName, key);
                }
                else
                {
                    logger?.LogError("Status {0} has no name found in {1} and english", s.GlobalName, locale);
                }

                if (desc != null)
                {
                    var key = $"status.{s.Id}.desc";
                    if (!result.TryAdd(key, desc))
                        logger?.LogWarning("Status {0} cannot register desc because key {1} already added somehow", s.GlobalName, key);
                }
                else
                {
                    logger?.LogError("Status {0} has no description found in {1} and english", s.GlobalName, locale);
                }
            }
        }

        private static void Glossary_Postfix(ref object __result, object __instance)
        {
            if (buildiconandtext_return_type == null)
                return;
            //check if this tt references a status.

            var key = tt_glossary_key_field?.GetValue(__instance) as string;
            if (key == null || !key.StartsWith("status"))
                return;
            //check if status enum value is an integer aka a custom status added by mod loader
            var splits = key.Split('.');
            if (splits.Length < 2)
                return;

            if (!int.TryParse(splits[1], out var key_id))
            {
                return;
            }

            //check if spr value exists
            if (!icon_lookup.TryGetValue(key_id, out var icon_spr))
            {
                return;
            }
            //at this point we must inject.
            //result is a valuetuple
            if (__result is not ITuple tuple)
                return;

            var text = tuple[1] as string;
            if (text == null)
                return;
            var new_result = Activator.CreateInstance(buildiconandtext_return_type, icon_spr, text);
            if (new_result == null)
                return;

            __result = new_result;
        }
    }
}