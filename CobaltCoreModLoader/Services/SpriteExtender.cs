using CobaltCoreModding.Definitions.ExternalResourceHelper;
using CobaltCoreModding.Definitions.ModContactPoints;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// Sprites are handled for the most part by a SpriteMapper and Sprite loader and a Sprite Enum.
    /// This services hooks into their guts and offers help function to mods making the whole loading process much smoother.
    /// </summary>
    public class SpriteExtender : IArtRegistry
    {
        private static CobaltCoreHandler? cobalt_core_handler;

        private static ILogger<SpriteExtender>? logger;

        private Assembly cobalt_core_assembly { get; init; }

        public SpriteExtender(ILogger<SpriteExtender> logger, CobaltCoreHandler cobaltCoreHandler)
        {
            SpriteExtender.cobalt_core_handler = cobaltCoreHandler;
            SpriteExtender.logger = logger;
            cobalt_core_assembly = cobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Cobalt Core Assembly not loaded.");
        }

        private const int sprite_id_counter_start = 100000;
        private static int sprite_id_counter = sprite_id_counter_start;

        /// <summary>
        /// central
        /// </summary>
        private static Dictionary<int, ExternalSprite> registered_sprites = new Dictionary<int, ExternalSprite>();

        public void PatchSpriteSystem()
        {
            //load manifest


            //"patch" mapping
            PatchMapping();
            //patch sprite loader
            PatchSpriteLoader();
        }

        private void PatchMapping()
        {
            //forcefully inject all registered sprites into the mapper.

            var assembly = cobalt_core_handler?.CobaltCoreAssembly;
            if (assembly != null)
            {
                var sprite_mapping = assembly.GetType("SpriteMapping") ?? throw new Exception("sprite mapping type not found");

                var mapping_field = sprite_mapping.GetField("mapping") ?? throw new Exception("mapping dictonary field not found.");

                var rvs_mapping_field = sprite_mapping.GetField("strToId") ?? throw new Exception("strToId dictonary field not found.");

                var spr_to_str_dictionary = (mapping_field.GetValue(null) as IDictionary) ?? throw new Exception("couldn't retrieve mapping dictonary");

                var str_to_spr_dictionary = (rvs_mapping_field.GetValue(null) as IDictionary) ?? throw new Exception("couldn't retrieve strToId dictonary");

                var spr_type = assembly.GetType("Spr") ?? throw new Exception("spr type not found");

                //add all registed values to dictionary.
                foreach (var sprite in registered_sprites.Values)
                {
                    if (sprite.Id == null)
                        continue;
                    var spr_val = Convert.ChangeType(sprite.Id, spr_type) ?? throw new Exception("Cast failed");
                    var str = $"@mod{sprite.Id}";
                    //update or add registed sprite data.
                    if (spr_to_str_dictionary.Contains(spr_val))
                        spr_to_str_dictionary[spr_val] = str;
                    else
                        spr_to_str_dictionary.Add(spr_val, str);

                    if (str_to_spr_dictionary.Contains(str))
                        str_to_spr_dictionary[str] = spr_val;
                    else
                        str_to_spr_dictionary.Add(str, spr_val);
                }
            }
        }

        private void PatchSpriteLoader()
        {
            Harmony harmony = new("modloader.spriteextender");

            var sprite_loader_type = cobalt_core_handler?.CobaltCoreAssembly?.GetType("SpriteLoader") ?? throw new Exception("Cannot find Sprite Loader in CobaltCoreAssembly.");
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
                        throw new NotImplementedException($"ExternalSprite with Id {sprite.Id?.ToString() ?? "null"} cannot be resolved!");
                }
                return texture;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "Exception during Sprite Loading!");
                return null;
            }
        }

        private static bool LoadFileToTexPrefix(string fullPath, ref Texture2D? __result)
        {
            if (!fullPath.StartsWith("@mod"))
                return true;
            if (!int.TryParse(fullPath.Substring(4), out int key))
            {
                __result = null;
                logger?.LogCritical("ill formed sprite id:" + fullPath);
                return false;
            }
            if (!registered_sprites.ContainsKey(key))
            {
                __result = null;
                logger?.LogCritical("unkown sprite id:" + fullPath);
                return false;
            }

            var sprite = registered_sprites[key];

            __result = LoadTexture(sprite);

            return false;
        }

        private static GraphicsDevice GetGraphicsDevice()
        {
            if (graphics_device != null)
                return graphics_device;

            //try load graphics device...

            var mg_type = cobalt_core_handler?.CobaltCoreAssembly?.GetType("MG") ?? throw new Exception("MG type not found in assembly");

            var mg_inst = mg_type.GetField("inst")?.GetValue(null) ?? throw new Exception("MG instance not found. has game not started?");

            graphics_device = (mg_type.GetMethod("GraphicsDevice", System.Reflection.BindingFlags.Public)?.Invoke(mg_inst, new object[0]) as GraphicsDevice);

            if (graphics_device == null)
            {
                throw new Exception("Cannot load Graphics device");
            }
            return graphics_device;
        }

        private static GraphicsDevice? graphics_device;

        private static IDictionary? CachedTextures;

        Assembly ICobaltCoreContact.CobaltCoreAssembly => throw new NotImplementedException();

#pragma warning disable IDE0051 // Remove unused private members

        private static bool GetSprPrefix(object id, ref Texture2D? __result)
#pragma warning restore IDE0051 // Remove unused private members
        {
            //if this cast breaks, we deserve a crash.
            int id_val = (int)id;
            //check if value is from base game.
            if (0 < id_val || id_val < sprite_id_counter_start)
                return true;

            //check if known
            if (!registered_sprites.ContainsKey(id_val))
            {
                __result = null;
                logger?.LogCritical($"Unregistered mod sprite with id '{id_val}' requested!");
                return false;
            }

            //look into cache

            if (CachedTextures == null)
            {
                CachedTextures = cobalt_core_handler?.CobaltCoreAssembly?.GetType("SpriteLoader")?.GetField("textures")?.GetValue(null) as IDictionary;
            }

            if (CachedTextures?.Contains(id) ?? false)
            {
                // chaced textures will contain a texture2d
                __result = CachedTextures[id] as Texture2D;
                return false;
            }

            //try load from dictionary
            var sprite = registered_sprites[id_val];

            __result = LoadTexture(sprite);
            //cache for future attempts.
            if (__result != null && !(CachedTextures?.Contains(id) ?? true))
            {
                CachedTextures.Add(id, __result);
            }
            return false;
        }

        int IArtRegistry.RegisterArt(ExternalSprite sprite_data, int? overwrite_value)
        {
            if (sprite_data.Id != 0)
                throw new ArgumentException("sprite data was already assigned id.");
            if (overwrite_value == null)
            {
                registered_sprites.Add(sprite_id_counter, sprite_data);
                sprite_data.Id = sprite_id_counter;
                return sprite_id_counter++;
            }
            else
            {
                var target_id = overwrite_value.Value;
                if (0 <= target_id && target_id < sprite_id_counter_start)
                    throw new Exception("Attempted overwrite of modded content detected!");

                if (registered_sprites.ContainsKey(overwrite_value.Value))
                {
                    logger?.LogWarning($"Collision of sprite overwrite with key {target_id} detected.");
                    registered_sprites[target_id] = sprite_data;
                }
                registered_sprites.Add(target_id, sprite_data);
                sprite_data.Id = target_id;
                return target_id;
            }
        }
    }
}