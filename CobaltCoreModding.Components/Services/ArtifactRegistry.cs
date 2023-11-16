using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Components.Utils;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace CobaltCoreModding.Components.Services
{
    public class ArtifactRegistry : IArtifactRegistry
    {

        private readonly ModAssemblyHandler modAssemblyHandler;

        /// <summary>
        /// Under what name artifacts are registered.
        /// </summary>
        private static Dictionary<string, Tuple<ExternalArtifact, bool>> artifact_targets = new();

        private static ILogger<IArtifactRegistry>? Logger;

        /// <summary>
        /// global name artifact lookup.
        /// </summary>
        private static Dictionary<string, ExternalArtifact> registered_artifacts = new Dictionary<string, ExternalArtifact>();

        public ArtifactRegistry(ILogger<IArtifactRegistry> logger, ModAssemblyHandler mah, CobaltCoreHandler cch)
        {
            Logger = logger;
            modAssemblyHandler = mah;
        }

        public bool RegisterArtifact(ExternalArtifact artifact, string? overwrite = null)
        {
            //check if already know by global name
            if (!artifact.ArtifactType.IsSubclassOf(TypesAndEnums.ArtifactType) || artifact.ArtifactType.IsAbstract)
            {
                Logger?.LogError("Attempted artifact {0} registry that is not in fact artifact typed!", artifact.GlobalName);
                return false;
            }

            if (artifact.OwnerDeck != null && artifact.OwnerDeck.Id == null)
            {
                Logger?.LogError("Attempted artifact {0} registry that has no deck registry", artifact.GlobalName);
                return false;
            }

            if (artifact.Sprite.Id == null)
            {
                Logger?.LogError("Attempted artifact {0} registry that has no sprite registry", artifact.GlobalName);
                return false;
            }

            if (!registered_artifacts.TryAdd(artifact.GlobalName, artifact))
            {
                Logger?.LogWarning("ExternalArtifact with global name {0} already registered. skipping...", artifact.GlobalName);
                return false;
            }

            //check for overwrite conflict.
            if (overwrite != null)
            {
                if (!artifact_targets.TryAdd(overwrite, new(artifact, true)))
                {
                    if (artifact_targets[overwrite].Item2)
                        Logger?.LogWarning("Overwriting External artifact overwrite {0} for target {1} with artifact {2}", artifact_targets[overwrite].Item1.GlobalName, overwrite, artifact.GlobalName);
                    artifact_targets[overwrite] = new(artifact, true);
                }
            }
            else
            {
                if (!artifact_targets.TryAdd(artifact.ArtifactType.Name, new(artifact, false)))
                {
                    Logger?.LogWarning("Artifact {0} cannot register target. Maybe an overwrite happened before or there is a type name collision.", artifact.GlobalName);
                }
            }
            return true;
        }

        internal static void PatchArtifactData()
        {
            //patch dictionary
            IDictionary artifact_dict = TypesAndEnums.DbType.GetField("artifacts", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new NullReferenceException("Cannot fiend db.artifacts");
            var copy = artifact_targets.ToArray();
            foreach (var artifact_data in copy)
            {
                if (artifact_dict.Contains(artifact_data.Key))
                {
                    if (!artifact_data.Value.Item2)
                    {
                        artifact_targets.Remove(artifact_data.Key);
                        Logger?.LogWarning("Attempted overwrite of existing artifact {0} with external {1} without overwrite indication. external artifact is skipped...", artifact_data.Key, artifact_data.Value.Item1.GlobalName);
                        continue;
                    }

                    artifact_dict[artifact_data.Key] = artifact_data.Value.Item1.ArtifactType;
                }
                else
                {
                    artifact_dict.Add(artifact_data.Key, artifact_data.Value.Item1.ArtifactType);
                }
            }
        }

        internal static void PatchArtifactMetas()
        {
            IDictionary artifact_meta_dict = TypesAndEnums.DbType.GetField("artifactMetas", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new NullReferenceException("Cannot fiend db.artifactMetas");

            var deck_field = TypesAndEnums.ArtifactMetaType.GetField("owner") ?? throw new NullReferenceException("cannot find cardmeta.owner field.");

            foreach (var artifact_data in artifact_targets)
            {
                var artifact = artifact_data.Value.Item1;
                if (artifact.OwnerDeck == null)
                    continue;
                var deck = artifact.OwnerDeck;
                var deck_val = TypesAndEnums.IntToDeck(deck.Id);
                if (deck_val == null)
                {
                    Logger?.LogWarning("Cannot convert deck {0} of artifact {1} into deck enum val", deck.GlobalName, artifact.GlobalName);
                    continue;
                }

                var key = artifact_data.Key;
                var artifact_meta = artifact_meta_dict[key];
                deck_field.SetValue(artifact_meta, deck_val);
            }
        }

        internal static void PatchArtifactSprites()
        {
            IDictionary artifact_sprite_dict = TypesAndEnums.DbType.GetField("artifactSprites", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null) as IDictionary ?? throw new NullReferenceException("Cannot fiend db.artifactSprites");
            foreach (var artifact_data in artifact_targets)
            {
                var artifact = artifact_data.Value.Item1;
                var sprite = artifact.Sprite;
                var key = artifact_data.Key;
                var spr = TypesAndEnums.IntToSpr(sprite.Id);
                if (sprite.Id == null)
                {
                    Logger?.LogError("Sprite {0} Id null for artifact {1}", sprite.GlobalName, artifact.GlobalName);
                    continue;
                }

                if (spr == null)
                {
                    Logger?.LogError("Sprite {0} not convertable to Spr enum val for artifact {1}", sprite.GlobalName, artifact.GlobalName);
                    continue;
                }

                if (artifact_sprite_dict.Contains(key))
                {
                    artifact_sprite_dict[key] = spr;
                }
                else
                {
                    artifact_sprite_dict.Add(key, spr);
                }
            }
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> loc_dictionary)
        {
            foreach (var artifact_data in artifact_targets)
            {
                var key = artifact_data.Key;
                var artifact = artifact_data.Value.Item1;

                if (!artifact.GetLocalisation(locale, out string name, out string description))
                {
                    Logger?.LogWarning("Missing localisation for artifact {0} in {1}", artifact.GlobalName, locale);
                    continue;
                }
                var name_key = $"artifact.{key}.name";
                var desc_key = $"artifact.{key}.desc";
                if (loc_dictionary.TryAdd(name_key, name))
                    loc_dictionary[name_key] = name;
                if (loc_dictionary.TryAdd(desc_key, description))
                    loc_dictionary[desc_key] = description;
            }
        }

        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.ArtifactManifests,Logger))
            {
                manifest.LoadManifest(this);
            }
        }

        internal bool ValidateArtifact(ExternalArtifact artifact)
        {
            return registered_artifacts.TryGetValue(artifact.GlobalName, out var reg_artifact) && reg_artifact == artifact;
        }
    }
}