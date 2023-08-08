using CobaltCoreModding.Definitions;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// This serice can be used to run patches
    /// </summary>
    public class DBPatcher
    {

        private static ModAssemblyHandler? ModStorage;

        private static ILogger<DBPatcher>? Logger;

        public DBPatcher(ModAssemblyHandler modAssemblyHandler, ILogger<DBPatcher> logger)
        {
            //Register ONCE!
            if (ModStorage == null)
            {
                ModStorage = modAssemblyHandler;
            }
            else
                throw new InvalidOperationException("DB patcher already loaded once. cannot have two!");
            Logger = logger;
        }

        private Harmony? harmony;

        public void PatchDB(Assembly cobalt_core_assembly)
        {
            if (harmony != null)
                harmony.UnpatchAll("modloader.dbpatcher");
            harmony = new Harmony("modloader.dbpatcher");

            var db_type = cobalt_core_assembly.GetType("DB") ?? throw new Exception("Cobalt Core Assembly missed DB type.");

            var load_subclass_function = db_type.GetMethod("LoadAllSubclasses", BindingFlags.Static | BindingFlags.NonPublic);

            var load_subclass_postfix = this.GetType().GetMethod("LoadAllSubclasses_PostFix", BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(load_subclass_function, postfix: new HarmonyMethod(load_subclass_postfix));

        }

        public void UnPatchDB()
        {
            if (harmony != null)
                harmony.UnpatchAll("modloader.dbpatcher");
        }


        private static Dictionary<string, Type> LoadAllSubclasses_PostFix<T>(Dictionary<string, Type> __result)
        {
            Logger?.LogDebug("Last found key:" + __result.Keys.LastOrDefault() ?? "");
            if (ModStorage == null)
            {
                Logger?.LogCritical("LoadAllSubclasses was called without a mod assembly storage. No data provided!");
            }
            else
            {
                Logger?.LogInformation($"Loading '{typeof(T).Name}' subclasses");

                //locate all attributes.
                var qualifying_types = ModStorage.ModLookup.Select(e => e.Item1).SelectMany(asembly => asembly.GetTypes().Where(my_type => my_type.IsClass && !my_type.IsAbstract && my_type.IsSubclassOf(typeof(T))));
                //filter those with ignore attribute
                qualifying_types = qualifying_types.Where(type => Attribute.GetCustomAttribute(type, typeof(IgnoreModComponentAttribute)) == null);
                //sort into dictionary
                var counter = 0;
                foreach (var type in qualifying_types)
                {
                    if (__result.TryAdd(type.Name, type))
                    {
                        Logger?.LogDebug("Loaded:" + type.Name);
                        counter++;
                    }
                    else
                        Logger?.LogWarning($"Type {type.Name} was already in dictionary. Naming conflict!");
                }
                Logger?.LogInformation($"Successfully loaded {counter} subclasses of {typeof(T).Name} from {ModStorage.ModLookup.Count()} mod assemblies.");
            }


            return __result;

        }

    }
}
