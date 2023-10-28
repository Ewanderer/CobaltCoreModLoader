using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModLoader.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// Part registry hooks up parts into dictinoary and offers tools to actualize parts from templates.
    /// </summary>
    public class PartRegistry : IShipPartRegistry
    {

        private static ILogger<PartRegistry>? logger;

        private static readonly Dictionary<string, ExternalPart> registeredParts = new();

        public void LoadManifests()
        {
            foreach (var manifest in ModAssemblyHandler.ShipPartsManifests)
            {
                manifest.LoadManifest(this);
            }
        }

        public PartRegistry(ILogger<PartRegistry> logger)
        {
            PartRegistry.logger = logger;
        }

        public static void PatchPartSprites()
        {
            var part_dict = TypesAndEnums.DbType.GetField("parts")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot get DB.parts dictionary.");
            foreach (var externalPart in registeredParts.Values)
            {
                if (externalPart.PartSprite.Id == null)
                {
                    logger?.LogCritical("ExternalPart {0} Sprite {1} has no id value", externalPart.GlobalName, externalPart.PartSprite.GlobalName);
                    continue;
                }
                var key = externalPart.GlobalName + externalPart.PartSprite.Id.Value.ToString();
                if (part_dict.Contains(key))
                {
                    logger?.LogCritical("Couldn't register {0} to Part Sprite Lookup because already there somehow, what did you do?", key);
                    continue;
                }
                var spr = TypesAndEnums.IntToSpr(externalPart.PartSprite.Id);
                part_dict.Add(key, spr);
            }
        }

        private static MethodInfo CopyPart = TypesAndEnums.MutilType.GetMethod("DeepCopy", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(new Type[] { TypesAndEnums.PartType }) ?? throw new Exception("Mutil.DeepCopy<Part> couldn't be created!");

        private static FieldInfo SkinField = TypesAndEnums.PartType.GetField("skin") ?? throw new Exception("Part.skin field not found.");



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

            string skin_str = globalName + (ext_part.PartSprite.Id?.ToString() ?? throw new Exception("ExternalPart has Sprite with no Id. Something went very wrong!!!"));

            var copy = CopyPart.Invoke(null, new object[] { ext_part.GetPartObject }) ?? throw new Exception("DeepCopy of Part failed.");

            SkinField.SetValue(copy, skin_str);

            return copy;

        }

        public static object ActualizePart(ExternalPart part)
        {
            return ActualizePart(part.GlobalName);
        }

        public bool RegisterPart(ExternalPart externalPart)
        {
            if (string.IsNullOrWhiteSpace(externalPart.GlobalName))
            {
                logger?.LogCritical("Attempted to register without a global name");
            }

            if (externalPart.PartSprite.Id == null)
            {
                logger?.LogWarning("ExternalPart {0} Sprite {1} has no id.", externalPart.GlobalName, externalPart.PartSprite.GlobalName);
                return false;
            }

            if (!externalPart.GetPartObject.GetType().IsAssignableTo(TypesAndEnums.PartType))
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

    }
}
