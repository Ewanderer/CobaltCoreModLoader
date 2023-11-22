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
    public class AnimationRegistry : IAnimationRegistry
    {
        private static ILogger<IAnimationRegistry>? Logger;

        private static Dictionary<string, ExternalAnimation> registered_animations = new Dictionary<string, ExternalAnimation>();

        private readonly ModAssemblyHandler modAssemblyHandler;

        public AnimationRegistry(ILogger<IAnimationRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
            modAssemblyHandler = mah;
        }

        Assembly ICobaltCoreLookup.CobaltCoreAssembly => CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("CobaltCoreAssemblyMissing");

        public static ExternalAnimation? LookupAnimation(string globalName)
        {
            registered_animations.TryGetValue(globalName, out var animation);
            return animation;
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.AnimationManifests, Logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by AnimationRegistry");
                }
            }
        }

        ExternalAnimation IAnimationLookup.LookupAnimation(string globalName)
        {
            return LookupAnimation(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalDeck IDeckLookup.LookupDeck(string globalName)
        {
            return DeckRegistry.LookupDeck(globalName) ?? throw new KeyNotFoundException();
        }

        IManifest IManifestLookup.LookupManifest(string globalName)
        {
            return ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();
        }

        ExternalSprite ISpriteLookup.LookupSprite(string globalName)
        {
            return SpriteExtender.LookupSprite(globalName) ?? throw new KeyNotFoundException();
        }

        bool IAnimationRegistry.RegisterAnimation(ExternalAnimation animation)
        {
            //
            if (string.IsNullOrEmpty(animation.GlobalName))
            {
                Logger?.LogWarning("animation without global name");
                return false;
            }

            if (string.IsNullOrEmpty(animation.Tag))
            {
                Logger?.LogWarning("ExternalAnimation {0} has not tag value", animation.GlobalName);
                return false;
            }

            if (!registered_animations.TryAdd(animation.GlobalName, animation))
            {
                Logger?.LogWarning("ExternalAnimation {0} already has an entry in registry. possible global name collision!", animation.GlobalName);
                return false;
            }

            return true;
        }

        internal static void PatchAnimations()
        {
            // animations

            IDictionary char_animation_dictionary = TypesAndEnums.DbType.GetField("charAnimations")?.GetValue(null) as IDictionary ?? throw new Exception();
            var spr_list_type = typeof(List<>).MakeGenericType(TypesAndEnums.SprType);
            var new_char_anim_dict_type = typeof(Dictionary<,>).MakeGenericType(typeof(string), spr_list_type);

            foreach (var character_animation_group in registered_animations.Values.GroupBy(e => e.Deck.GlobalName))
            {
                IDictionary? animation_lookup = null;
                if (!char_animation_dictionary.Contains(character_animation_group.Key))
                {
                    animation_lookup = Activator.CreateInstance(new_char_anim_dict_type) as IDictionary ?? throw new Exception();
                    char_animation_dictionary.Add(character_animation_group.Key, animation_lookup);
                }
                else
                {
                    animation_lookup = char_animation_dictionary[character_animation_group.Key] as IDictionary ?? throw new Exception();
                }

                //iterate over all animations in group

                foreach (var animation in character_animation_group)
                {
                    //create sprite list
                    var spr_list = Activator.CreateInstance(spr_list_type) as IList ?? throw new Exception();
                    //populate it
                    bool valid = true;

                    foreach (var frame in animation.Frames)
                    {
                        var spr_val = TypesAndEnums.IntToSpr(frame?.Id);
                        if (spr_val == null)
                        {
                            Logger?.LogWarning("ExternalAnimation {0} Frame Sprite {1} was not resolved to Spr object. skipping animation entirely", animation.GlobalName, frame?.GlobalName ?? "NULL");
                            valid = false;
                            continue;
                        }

                        spr_list.Add(spr_val);
                    }
                    if (!valid) { continue; }

                    //either overwrite or add list.

                    if (!animation_lookup.Contains(animation.Tag))
                    {
                        animation_lookup.Add(animation.Tag, spr_list);
                    }
                    else if (animation.IntendedOverwrite)
                    {
                        animation_lookup[animation.Tag] = spr_list;
                    }
                    else
                    {
                        Logger?.LogWarning("Collision of external animation {0} detected for character {1} with tag {2}. if you inteded to overwrite, set ExternalAnimation.intendedoverwrite during construction", animation.GlobalName, character_animation_group.Key, animation.Tag);
                    }
                }
            }
        }
    }
}