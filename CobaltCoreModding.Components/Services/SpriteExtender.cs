using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ItemLookups;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    /// <summary>
    /// Sprites are handled for the most part by a SpriteMapper and Sprite loader and a Sprite Enum.
    /// This services hooks into their guts and offers help function to mods making the whole loading process much smoother.
    /// </summary>
    public class SpriteExtender : ISpriteRegistry
    {
        private const int sprite_id_counter_start = 10000000;
        private static IDictionary? CachedTextures;
        private static GraphicsDevice? graphics_device;
        private static ILogger<SpriteExtender>? logger;
        private static int sprite_id_counter = sprite_id_counter_start;
        private static Dictionary<string, ExternalSprite> sprite_lookup = new Dictionary<string, ExternalSprite>();

        /// <summary>
        /// central
        /// </summary>
        private static Dictionary<int, ExternalSprite> sprite_registry = new Dictionary<int, ExternalSprite>();

        private readonly ModAssemblyHandler modAssemblyHandler;

        public SpriteExtender(ILogger<SpriteExtender> logger, CobaltCoreHandler cobaltCoreHandler, ModAssemblyHandler modAssemblyHandler)
        {
            SpriteExtender.logger = logger;
            this.modAssemblyHandler = modAssemblyHandler;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        Func<object> ISpriteRegistry.GetCobaltCoreGraphicsDeviceFunc => () => { return SpriteExtender.GetGraphicsDevice(); };

        public static GraphicsDevice GetGraphicsDevice()
        {
            if (graphics_device != null)
                return graphics_device;

            //try load graphics device...

            var mg_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("MG") ?? throw new Exception("MG type not found in assembly");

            var mg_inst = mg_type.GetField("inst")?.GetValue(null) ?? throw new Exception("MG instance not found. has game not started?");

            graphics_device = (mg_type.BaseType?.GetMethod("get_GraphicsDevice")?.Invoke(mg_inst, new object[0]) as GraphicsDevice);

            if (graphics_device == null)
            {
                throw new Exception("Cannot load Graphics device");
            }
            return graphics_device;
        }

        public static ExternalSprite? LookupSprite(string globalName)
        {
            if (!sprite_lookup.TryGetValue(globalName, out var sprite))
                logger?.LogWarning($"Requested external sprite {globalName} unkown");
            return sprite;
        }

        public IManifest LookupManifest(string globalName)
        {
            var item = ModAssemblyHandler.LookupManifest(globalName);
            if (item == null)
                throw new KeyNotFoundException();
            return item;
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        public void PatchSpriteSystem()
        {
            //load manifest
            RunArtManifest();
            //"patch" mapping
            PatchMapping();
            //patch sprite loader
            PatchSpriteLoader();
        }

        bool ISpriteRegistry.RegisterArt(ExternalSprite sprite_data, int? overwrite_value)
        {
            if (sprite_data.Id != null)
            {
                logger?.LogCritical("sprite data was already assigned id.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sprite_data.GlobalName))
            {
                logger?.LogCritical("Attempted to register without globalname. registry rejected");
                return false;
            }

            if (!sprite_lookup.TryAdd(sprite_data.GlobalName, sprite_data))
            {
                logger?.LogCritical($"Art with global name {sprite_data.GlobalName} already know");
                return false;
            }

            if (overwrite_value == null)
            {
                sprite_registry.Add(sprite_id_counter, sprite_data);
                sprite_data.Id = sprite_id_counter;
                sprite_id_counter++;
            }
            else
            {
                var target_id = overwrite_value.Value;
                if (target_id < 0 && sprite_id_counter_start <= target_id)
                    throw new Exception("Attempted overwrite of modded content detected!");

                if (sprite_registry.ContainsKey(overwrite_value.Value))
                {
                    logger?.LogWarning($"Collision of sprite overwrite with key {target_id} detected.");
                    sprite_registry[target_id] = sprite_data;
                }
                sprite_registry.Add(target_id, sprite_data);
                sprite_data.Id = target_id;
            }
            return true;
        }

        internal static void BreakAtlas()
        {
            //get db type.
            var db_type = TypesAndEnums.DbType;
            var spr_type = TypesAndEnums.SprType;
            var atlas = db_type.GetField("atlas")?.GetValue(null) as IDictionary;
            if (atlas != null)
            {
                foreach (var overwrite_sprite in sprite_registry.Values.Where(e => e.Id != null && 0 <= e.Id && e.Id < sprite_id_counter_start))
                {
                    if (overwrite_sprite == null || overwrite_sprite.Id == null)
                        continue;
                    var texture = LoadTexture(overwrite_sprite);
                    if (texture == null && overwrite_sprite.IsCaching)
                    {
                        logger?.LogWarning("Failed to load overwrite texture");
                        continue;
                    }

                    object spr_val;
                    try
                    {
                        spr_val = Convert.ChangeType(Enum.ToObject(spr_type, overwrite_sprite.Id), spr_type) ?? throw new Exception("Cast failed");
                    }
                    catch
                    {
                        continue;
                    }
                    if (spr_val == null)
                        continue;
                    if (atlas.Contains(spr_val))
                        atlas.Remove(spr_val);
                }
            }
        }

        internal static bool ValidateSprValue(int spr)
        {
            return Enum.IsDefined(TypesAndEnums.SprType, spr) || sprite_registry.Values.Any(e => e.Id == spr);
        }

        private static bool GetSprPrefix(object id, ref Texture2D? __result)
        {
            //if this cast breaks, we deserve a crash.
            int id_val = (int)id;
            //check if value is from base game.
            if (sprite_registry.TryGetValue(id_val, out var sprite) && !sprite.IsCaching)
            {
                __result = LoadTexture(sprite);
                return false;
            }

            if (0 <= id_val && id_val < sprite_id_counter_start)
                return true;

            //check if known
            if (sprite == null)
            {
                __result = null;
                logger?.LogCritical($"Unregistered mod sprite with id '{id_val}' requested!");
                return false;
            }

            //look into cache

            if (CachedTextures == null)
            {
                CachedTextures = CobaltCoreHandler.CobaltCoreAssembly?.GetType("SpriteLoader")?.GetField("textures")?.GetValue(null) as IDictionary;
            }

            if (sprite.IsCaching && (CachedTextures?.Contains(id) ?? false))
            {
                // chaced textures will contain a texture2d
                __result = CachedTextures[id] as Texture2D;
                return false;
            }

            //try load from dictionary

            __result = LoadTexture(sprite);
            //cache for future attempts.
            if (__result != null && !(CachedTextures?.Contains(id) ?? true) && sprite.IsCaching)
            {
                CachedTextures.Add(id, __result);
            }
            return false;
        }

        private static bool LoadFileToTexPrefix(string path, ref Texture2D? __result)
        {
            if (!path.StartsWith("@mod"))
                return true;
            if (!int.TryParse(path.Substring(4), out int key))
            {
                __result = null;
                logger?.LogCritical("ill formed sprite id:" + path);
                return false;
            }
            if (!sprite_registry.ContainsKey(key))
            {
                __result = null;
                logger?.LogCritical("unkown sprite id:" + path);
                return false;
            }

            var sprite = sprite_registry[key];

            __result = LoadTexture(sprite);

            return false;
        }

        private static Texture2D? LoadTexture(ExternalSprite sprite)
        {
            Texture2D? texture = null;
            try
            {
                if (sprite.virtual_location != null)
                {
                    using var stream = sprite.virtual_location();
                    texture = Texture2D.FromStream(GetGraphicsDevice(), stream);
                }
                else if (sprite.physical_location != null)
                {
                    texture = Texture2D.FromFile(GetGraphicsDevice(), sprite.physical_location.FullName);
                }
                else
                {
                    texture = sprite.GetTexture() as Texture2D;
                    if (texture == null)
                        throw new NotImplementedException($"ExternalSprite with Id {sprite.Id?.ToString() ?? "null"} didn't return a texture!");
                }
                return texture;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "Exception during Sprite Loading!");
                return null;
            }
        }

        private static void PreloadEverythingPostFix()
        {
            var counter = 0;
            //overwriting textures

            if (CachedTextures == null)
            {
                CachedTextures = CobaltCoreHandler.CobaltCoreAssembly?.GetType("SpriteLoader")?.GetField("textures")?.GetValue(null) as IDictionary;
            }
            if (CachedTextures == null)
            {
                logger?.LogCritical("No texture cache found!");
                return;
            }

            foreach (var overwrite_sprite in sprite_registry.Values.Where(e => e.Id != null && 0 <= e.Id && e.Id < sprite_id_counter_start && e.IsCaching))
            {
                if (overwrite_sprite == null || overwrite_sprite.Id == null)
                    continue;
                var texture = LoadTexture(overwrite_sprite);
                if (texture == null)
                {
                    logger?.LogWarning("Failed to load overwrite texture");
                    continue;
                }

                object? spr_val = TypesAndEnums.IntToSpr(overwrite_sprite.Id);
                if (spr_val == null)
                {
                    logger?.LogCritical($"Couldn't convert {overwrite_sprite.Id?.ToString() ?? "null"} not to spr val during overwrite of original sprites.");
                    continue;
                }

                if (CachedTextures.Contains(spr_val))
                {
                    var old = CachedTextures[spr_val];
                    CachedTextures[spr_val] = texture;
                    (old as IDisposable)?.Dispose();
                }
                else
                {
                    CachedTextures.Add(spr_val, texture);
                }

                counter++;
            }
            logger?.LogInformation($"Overwrote {counter} original sprites");
        }

        private void PatchMapping()
        {
            //forcefully inject all registered sprites into the mapper.

            var assembly = CobaltCoreHandler.CobaltCoreAssembly;
            if (assembly != null)
            {
                var sprite_mapping = assembly.GetType("SpriteMapping") ?? throw new Exception("sprite mapping type not found");

                var mapping_field = sprite_mapping.GetField("mapping") ?? throw new Exception("mapping dictonary field not found.");

                var rvs_mapping_field = sprite_mapping.GetField("strToId") ?? throw new Exception("strToId dictonary field not found.");

                var spr_to_path_dictionary = (mapping_field.GetValue(null) as IDictionary) ?? throw new Exception("couldn't retrieve mapping dictonary");

                var str_to_spr_dictionary = (rvs_mapping_field.GetValue(null) as IDictionary) ?? throw new Exception("couldn't retrieve strToId dictonary");

                var spr_type = assembly.GetType("Spr") ?? throw new Exception("spr type not found");

                //add all registed values to dictionary.
                foreach (var sprite in sprite_registry.Values)
                {
                    if (sprite.Id == null)
                        continue;
                    var spr_val = Convert.ChangeType(Enum.ToObject(spr_type, sprite.Id), spr_type) ?? throw new Exception("Cast failed");

                    var str = $"@mod{sprite.Id}";
                    var spr_path = Activator.CreateInstance(TypesAndEnums.SpritePathType, str) ?? throw new Exception("Sprite Path wasn't created");
                    //update or add registed sprite data.

                    if (spr_to_path_dictionary.Contains(spr_val))
                        spr_to_path_dictionary[spr_val] = spr_path;
                    else
                        spr_to_path_dictionary.Add(spr_val, spr_path);
                    if (!Enum.IsDefined(spr_type, spr_val))
                    {
                        if (str_to_spr_dictionary.Contains(str))
                            str_to_spr_dictionary[str] = spr_val;
                        else
                            str_to_spr_dictionary.Add(str, spr_val);
                    }
                }
            }
        }

        private void PatchSpriteLoader()
        {
            Harmony harmony = new("modloader.spriteextender");

            var sprite_loader_type = CobaltCoreHandler.CobaltCoreAssembly?.GetType("SpriteLoader") ?? throw new Exception("Cannot find Sprite Loader in CobaltCoreAssembly.");
            //Patch get sprite method
            {
                var get_sprite_method = sprite_loader_type.GetMethod("Get", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public) ?? throw new Exception("Cannot find Get Method in Sprite Loader");

                var get_sprite_prefix = typeof(SpriteExtender).GetMethod("GetSprPrefix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Cannot find get sprite prefix method!");

                harmony.Patch(get_sprite_method, prefix: new HarmonyMethod(get_sprite_prefix));
            }
            //patch get load method
            {
                var load_file_to_tex_method = sprite_loader_type.GetMethod("LoadFileToTex", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Cannot find load file to tex method in spriter loader");

                var load_file_to_tex_prefix = typeof(SpriteExtender).GetMethod("LoadFileToTexPrefix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Cannot find load_file_to_tex_prefix method!");

                harmony.Patch(load_file_to_tex_method, prefix: new HarmonyMethod(load_file_to_tex_prefix));
            }
            //patch cache for preload everything
            {
                var preload_everything_method = sprite_loader_type.GetMethod("Preload", BindingFlags.Static | BindingFlags.Public) ?? throw new Exception("Cannot find Preload method in spriter loader");
                var preload_everything_postfix = typeof(SpriteExtender).GetMethod("PreloadEverythingPostFix", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Cannot find preload_everything_postfix method!");

                harmony.Patch(preload_everything_method, postfix: new HarmonyMethod(preload_everything_postfix));
            }
        }

        private void RunArtManifest()
        {
            var sprite_manifests = ModAssemblyHandler.SpriteManifests;
            foreach (var manifest in modAssemblyHandler.LoadOrderly(sprite_manifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by SpriteExtender");
                }
            }
        }
    }
}