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
    /// Ship registry allows for ship templates to be registered.
    /// it will take care of adding playable ships to the selection.
    /// but can also directly provide copies on demand, so that for example an AI can retrieve a ship.
    /// </summary>
    public class ShipRegistry : IShipRegistry, IRawShipRegistry
    {
        /// <summary>
        /// This dictionary holds both external ships and raw ship object
        /// </summary>
        private static readonly Dictionary<string, object> registeredShips = new Dictionary<string, object>();

        private static MethodInfo CopyShip = TypesAndEnums.MutilType.GetMethod("DeepCopy", BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(new Type[] { TypesAndEnums.ShipType }) ?? throw new Exception("Mutil.DeepCopy<Ship> couldn't be created!");
        private static ShipRegistry? instance;
        private static ILogger<ShipRegistry>? logger;
        private static ModAssemblyHandler? modAssemblyHandler;
        private static FieldInfo part_skin_field = TypesAndEnums.PartType.GetField("skin") ?? throw new Exception("Part.skin field not found");
        private static FieldInfo parts_field = TypesAndEnums.ShipType.GetField("parts") ?? throw new Exception("Ship.parts field not found");
        private static FieldInfo ship_chassisOver_field = TypesAndEnums.ShipType.GetField("chassisOver") ?? throw new Exception("Ship.chassisOver field not found");
        private static FieldInfo ship_chassisUnder_field = TypesAndEnums.ShipType.GetField("chassisUnder") ?? throw new Exception("Ship.chassisUnder field not found");
        private readonly PartRegistry partRegistry;

        public ShipRegistry(ILogger<ShipRegistry> logger, PartRegistry partRegistry, ModAssemblyHandler mah)
        {
            ShipRegistry.logger = logger;
            this.partRegistry = partRegistry;
            ShipRegistry.instance = this;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        /// <summary>
        /// Creates copy of a ship object registered under a global name.
        /// </summary>
        /// <param name="global_name">globalName of the ship</param>
        /// <returns></returns>
        public static object ActualizeShip(string global_name)
        {
            if (!registeredShips.TryGetValue(global_name, out var ship_entry))
            {
                throw new Exception($"No Ship under global name {global_name} registered!");
            }
            if (ship_entry is ExternalShip externalShip)
            {
                return ActualizeExternalShip(externalShip);
            }
            return CopyShip.Invoke(null, new object[] { ship_entry }) ?? throw new Exception($"Copy of raw ship {global_name} failed.");
        }

        public static void LoadRawManifests()
        {
            if (instance == null)
            {
                logger?.LogCritical("Instance is null. Cannot load raw ships.");
                return;
            }
            foreach (var manifest in modAssemblyHandler?.LoadOrderly(ModAssemblyHandler.RawShipManifests, logger) ?? ModAssemblyHandler.RawShipManifests)
            {
                try
                {
                    manifest.LoadManifest(instance);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by RawShipRegistry");
                }
            }
        }

        public static object? LookupShip(string globalName)
        {
            if (!registeredShips.TryGetValue(globalName, out var ship))
                logger?.LogWarning("ExternalShip {0} cannot be found", globalName);
            return ship;
        }

        /// <summary>
        /// Put the chassis in the db.
        /// </summary>
        public static void PatchChassisArt()
        {
            var part_dict = TypesAndEnums.DbType.GetField("parts")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot get DB.parts dictionary.");
            foreach (var obj in registeredShips.Values)
            {
                if (obj is not ExternalShip ship)
                    continue;
                if (ship.ChassisUnderSprite != null)
                {
                    var spr = TypesAndEnums.IntToSpr(ship.ChassisUnderSprite.Id);
                    if (part_dict.Contains(ship.underChassisKey))
                    {
                        logger?.LogCritical("DB.Parts already contains key {0} somehow. what did you do?", ship.underChassisKey);
                    }
                    else
                    {
                        part_dict.Add(ship.underChassisKey, spr);
                    }
                }
                if (ship.ChassisOverSprite != null)
                {
                    var spr = TypesAndEnums.IntToSpr(ship.ChassisOverSprite.Id);
                    if (part_dict.Contains(ship.overChassisKey))
                    {
                        logger?.LogCritical("DB.Parts already contains key {0} somehow. what did you do?", ship.overChassisKey);
                    }
                    else
                    {
                        part_dict.Add(ship.overChassisKey, spr);
                    }
                }
            }
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler?.LoadOrderly(ModAssemblyHandler.ShipManifests, logger) ?? ModAssemblyHandler.ShipManifests)
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by ShipRegistry");
                }
            }
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalPart IPartLookup.LookupPart(string globalName)
        {
            return PartRegistry.LookupPart(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalPartType IPartTypeLookup.LookupPartType(string globalName)
        {
            return PartTypeRegistry.LookupPartType(globalName) ?? throw new KeyNotFoundException();
        }

        object IShipLookup.LookupShip(string globalName)
        {
            return LookupShip(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        public bool RegisterShip(ExternalShip ship)
        {
            //check global name
            if (string.IsNullOrWhiteSpace(ship.GlobalName))
            {
                logger?.LogCritical("Ship has no global name!");
                return false;
            }
            //validate ship object
            if (!ship.GetShipObject().GetType().IsAssignableTo(TypesAndEnums.ShipType))
            {
                logger?.LogCritical("ExternalShip {0} doesn't hold a CobaltCore.Ship object.", ship.GlobalName);
                return false;
            }

            //validate chassis

            if (ship.ChassisUnderSprite != null && ship.ChassisUnderSprite.Id == null)
            {
                logger?.LogCritical("ExternalShip {0} ChassisUnderSprite {1} has no id", ship.GlobalName, ship.ChassisUnderSprite.GlobalName);
                return false;
            }

            if (ship.ChassisOverSprite != null && ship.ChassisOverSprite.Id == null)
            {
                logger?.LogCritical("ExternalShip {0} ChassisOverSprite {1} has no id", ship.GlobalName, ship.ChassisOverSprite.GlobalName);
                return false;
            }

            //validate all parts
            var invalid_parts = ship.Parts.Where(p => !partRegistry.ValidatePart(p));
            if (invalid_parts.Any())
            {
                logger?.LogWarning("ExternalShip {0} has not registered external parts: {1}", ship.GlobalName, string.Join(", ", invalid_parts.Select(p => p.GlobalName)));
                return false;
            }

            //attempt to register
            if (!registeredShips.TryAdd(ship.GlobalName, ship))
            {
                logger?.LogWarning("ExternalShip with global name {0} already registered. Skipping this entry...", ship.GlobalName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Used to register a raw ship.
        /// </summary>
        /// <param name="ship">a cobaltcore.ship object.</param>
        /// <param name="global_name">the global name, must be unique</param>
        /// <param name="playable">should the ship being for players</param>
        /// <param name="overwrite_starter_deck">forces normal starter deck to not be generated for player.</param>
        public bool RegisterShip(object ship, string global_name)
        {
            if (string.IsNullOrWhiteSpace(global_name))
            {
                logger?.LogCritical("Attempted to register ship with no global name.");
                return false;
            }

            if (!ship.GetType().IsAssignableTo(TypesAndEnums.ShipType))
            {
                logger?.LogCritical("Attempted to register a raw ship under global name {0} that isn't a CobaltCore.Ship object.", global_name);
                return false;
            }

            var parts = parts_field.GetValue(ship) as IEnumerable;
            if (parts == null)
            {
                logger?.LogWarning("Raw ship {0} couldn't retrieve part list", global_name);
                return false;
            }

            IDictionary part_dict = TypesAndEnums.DbType.GetField("parts")?.GetValue(null) as IDictionary ?? throw new Exception("Cannot get DB.parts dictionary.");
            List<string> missing_parts = new();
            //validate all skins exist.
            foreach (var part in parts)
            {
                var skin = part_skin_field.GetValue(part) as string;
                if (skin == null)
                    continue;
                //check part dictionary.
                if (!part_dict.Contains(skin))
                {
                    missing_parts.Add(skin);
                }
            }
            if (missing_parts.Any())
            {
                logger?.LogWarning("Raw ship {0} missing part skins: {1}", global_name, string.Join(", ", missing_parts));
                return false;
            }
            //check if chassis skin exist in parts...
            var under_chassis = ship_chassisUnder_field.GetValue(ship) as string;
            if (under_chassis != null && !part_dict.Contains(under_chassis))
            {
                logger?.LogWarning("Raw ship {0} chassisUnder {1} not found in DB.parts", global_name, under_chassis);
            }
            var over_chassis = ship_chassisOver_field.GetValue(ship) as string;
            if (over_chassis != null && !part_dict.Contains(over_chassis))
            {
                logger?.LogWarning("Raw ship {0} chassisOver {1} not found in DB.parts", global_name, over_chassis);
            }
            //attempt to feed to registy.
            if (!registeredShips.TryAdd(global_name, ship))
            {
                logger?.LogWarning("A ship was already registered under the global name '{0}'. skipping entry...", global_name);
                return false;
            }
            return true;
        }

        internal static bool CheckShip(string global_name)
        {
            return registeredShips.ContainsKey(global_name);
        }

        private static object ActualizeExternalShip(ExternalShip ship)
        {
            var result = CopyShip.Invoke(null, new object[] { ship.GetShipObject() }) ?? throw new Exception($"Copy of external ship {ship.GlobalName} template failed.");
            //fill chassis
            if (ship.ChassisUnderSprite != null)
                ship_chassisUnder_field.SetValue(result, ship.underChassisKey);
            if (ship.ChassisOverSprite != null)
                ship_chassisOver_field.SetValue(result, ship.overChassisKey);
            //fill parts
            var part_list = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypesAndEnums.PartType));
            if (part_list is not IList pl)
                throw new Exception("Failed to make List<Part>");
            foreach (var ext_part in ship.Parts)
            {
                var part = PartRegistry.ActualizePart(ext_part);
                if (pl.Add(part) == -1)
                    throw new Exception("Failed to add part to List<Part>");
            }
            parts_field.SetValue(result, part_list);
            return result;
        }
    }
}