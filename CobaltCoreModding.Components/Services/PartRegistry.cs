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
    /// <summary>
    /// Part registry hooks up parts into dictinoary and offers tools to actualize parts from templates.
    /// </summary>
    public class PartRegistry : IShipPartRegistry
    {
        private static readonly Dictionary<string, ExternalPart> registeredParts = new();
        private static MethodInfo CopyPart = TypesAndEnums.MutilType.GetMethod("DeepCopy", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(new Type[] { TypesAndEnums.PartType }) ?? throw new Exception("Mutil.DeepCopy<Part> couldn't be created!");
        private static ILogger<PartRegistry>? logger;
        private static Dictionary<string, Tuple<int, int?>> raw_parts = new Dictionary<string, Tuple<int, int?>>();
        private static FieldInfo SkinField = TypesAndEnums.PartType.GetField("skin") ?? throw new Exception("Part.skin field not found.");
        private readonly ModAssemblyHandler modAssemblyHandler;

        public PartRegistry(ILogger<PartRegistry> logger, ModAssemblyHandler mah)
        {
            PartRegistry.logger = logger;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        /// <summary>
        /// Creates a copy of a part template in ExternalPart and fixes its skin value.
        /// </summary>
        /// <param name="globalName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object ActualizePart(string globalName)
        {
            if (!registeredParts.ContainsKey(globalName))
                throw new Exception($"No ExternalPart with global name '{globalName}' exist.");
            var ext_part = registeredParts[globalName];

            string skin_str = ext_part.Key;

            var copy = CopyPart.Invoke(null, new object[] { ext_part.GetPartObject() }) ?? throw new Exception("DeepCopy of Part failed.");

            SkinField.SetValue(copy, skin_str);

            return copy;
        }

        public static object ActualizePart(ExternalPart part)
        {
            return ActualizePart(part.GlobalName);
        }

        public static ExternalPart? LookupPart(string globalName)
        {
            if (!registeredParts.TryGetValue(globalName, out var part))
                logger?.LogWarning("ExternalPart {0} not found", globalName);
            return part;
        }

        public static void PatchPartSprites()
        {
            var part_dict = TypesAndEnums.DbType.GetField("parts")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot get DB.parts dictionary.");
            var partOff_dict = TypesAndEnums.DbType.GetField("partsOff")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot get DB.partsOff dictionary.");
            //patch external parts
            foreach (var externalPart in registeredParts.Values)
            {
                if (externalPart.PartSprite.Id == null)
                {
                    logger?.LogCritical("ExternalPart {0} Sprite {1} has no id value", externalPart.GlobalName, externalPart.PartSprite.GlobalName);
                    continue;
                }
                var key = externalPart.Key;
                if (part_dict.Contains(key))
                {
                    logger?.LogCritical("Couldn't register {0} to Part Sprite Lookup because already there somehow, what did you do?", key);
                    continue;
                }

                var spr = TypesAndEnums.IntToSpr(externalPart.PartSprite.Id);
                part_dict.Add(key, spr);

                //put off part sprite into dictionary.
                if (externalPart.PartOffSprite == null)
                    continue;
                spr = TypesAndEnums.IntToSpr(externalPart.PartOffSprite.Id);
                if (partOff_dict.Contains(key))
                {
                    logger?.LogCritical("Couldn't register {0} to PartOff Sprite Lookup because already there somehow, what did you do?", key);
                    continue;
                }
                partOff_dict.Add(key, spr);
            }
            //patch raw part sprites

            foreach (var entry in raw_parts)
            {
                var key = "@mod_extra_part:" + entry.Key;

                if (part_dict.Contains(key))
                {
                    logger?.LogCritical("Couldn't register {0} to Part Sprite Lookup because already there somehow, what did you do?", key);
                }
                else
                {
                    var spr = TypesAndEnums.IntToSpr(entry.Value.Item1);
                    part_dict.Add(key, spr);
                }

                if (entry.Value.Item2 != null)
                {
                    if (partOff_dict.Contains(key))
                    {
                        logger?.LogCritical("Couldn't register {0} to Part Sprite Lookup because already there somehow, what did you do?", key);
                    }
                    else
                    {
                        var spr = TypesAndEnums.IntToSpr(entry.Value.Item2);
                        partOff_dict.Add(key, spr);
                    }
                }
            }
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.ShipPartsManifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by PartRegistry");
                }
            }
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalPart IPartLookup.LookupPart(string globalName)
        {
            return LookupPart(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalPartType IPartTypeLookup.LookupPartType(string globalName)
        {
            return PartTypeRegistry.LookupPartType(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        public bool RegisterPart(ExternalPart externalPart)
        {
            if (string.IsNullOrWhiteSpace(externalPart.GlobalName))
            {
                logger?.LogCritical("Attempted to register without a global name");
            }

            if (externalPart.PartSprite.Id == null)
            {
                logger?.LogWarning("ExternalPart {0} Active Sprite {1} has no id.", externalPart.GlobalName, externalPart.PartSprite.GlobalName);
                return false;
            }

            if (externalPart.PartOffSprite != null && externalPart.PartOffSprite.Id == null)
            {
                logger?.LogWarning("ExternalPart {0} Inactive Sprite {1} has no id.", externalPart.GlobalName, externalPart.PartOffSprite.GlobalName);
                return false;
            }

            if (!externalPart.GetPartObject().GetType().IsAssignableTo(TypesAndEnums.PartType))
            {
                logger?.LogWarning("ExternalPart {0} GetPartObject doesn't return an object of type CobaltCore.Part or a child type.", externalPart.GlobalName);
                return false;
            }

            //Mabye add check for external PartType enum extenension later.

            if (!registeredParts.TryAdd(externalPart.GlobalName, externalPart))
            {
                logger?.LogWarning("ExternalPart with global name {0} was already registered.", externalPart.GlobalName);
                return false;
            }

            return true;
        }

        public bool RegisterRawPart(string global_name, int spr_value, int? off_spr_value = null)
        {
            if (string.IsNullOrWhiteSpace(global_name))
            {
                logger?.LogCritical("Attempted to register raw part with no global name");
                return false;
            }

            if (!SpriteExtender.ValidateSprValue(spr_value))
            {
                logger?.LogCritical("RawPart {0} attempted to register unkown spr value:" + spr_value, global_name);
                return false;
            }

            if (off_spr_value != null && !SpriteExtender.ValidateSprValue(off_spr_value.Value))
            {
                logger?.LogCritical("RawPart {0} attempted to register unkown spr value:" + spr_value, global_name);
                return false;
            }

            if (!raw_parts.TryAdd(global_name, new(spr_value, off_spr_value)))
            {
                logger?.LogCritical("RawPart with global name {0} already exits. skipping...", global_name);
                return false;
            }
            return true;
        }

        internal bool ValidatePart(ExternalPart part)
        {
            return registeredParts.TryGetValue(part.GlobalName, out var reg_part) && reg_part == part;
        }
    }
}