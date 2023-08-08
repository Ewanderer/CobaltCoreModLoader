﻿using CobaltCoreModding.Definitions;
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

        private static CobaltCoreHandler? CobaltCoreHandler;

        public DBPatcher(CobaltCoreHandler cobaltCoreHandler, ModAssemblyHandler modAssemblyHandler, ILogger<DBPatcher> logger)
        {
            //Register ONCE!
            if (ModStorage == null)
            {
                ModStorage = modAssemblyHandler;
            }
            else
                throw new InvalidOperationException("DB patcher already loaded once. cannot have two!");
            Logger = logger;
            CobaltCoreHandler = cobaltCoreHandler;
        }

        private Harmony? harmony;

        public void PatchDB()
        {
            Assembly cobalt_core_assembly = CobaltCoreHandler.CobaltCoreAssembly ?? throw new Exception("Cobalt Core not found");
            if (harmony != null)
                harmony.UnpatchAll("modloader.dbpatcher");
            harmony = new Harmony("modloader.dbpatcher");


            //patch DB
            var db_type = cobalt_core_assembly.GetType("DB") ?? throw new Exception("Cobalt Core Assembly missed DB type.");

            var make_init_queue_function = db_type.GetMethod("MakeInitQueue");

            var make_init_queue_postfix = typeof(DBPatcher).GetMethod("MakeInitQueue_Postfix", BindingFlags.Static | BindingFlags.NonPublic);

            harmony.Patch(make_init_queue_function, postfix: new HarmonyMethod(make_init_queue_postfix));

        }

        public void UnPatchDB()
        {
            if (harmony != null)
                harmony.UnpatchAll("modloader.dbpatcher");
        }


        private static Queue<Action> MakeInitQueue_Postfix(Queue<Action> __result)
        {
            //extract each action and splice in custom actions.

            var patched_result = new Queue<Action>();

            patched_result.Enqueue(__result.Dequeue());

            patched_result.Enqueue(() => { Load_1(); });

            while (__result.Count > 0)
                patched_result.Enqueue(__result.Dequeue());

            return patched_result;

        }

        private static void Load_1()
        {
            if (CobaltCoreHandler?.CobaltCoreAssembly != null)
            {
                //patch into all fields
                var db_type = CobaltCoreHandler.CobaltCoreAssembly.GetType("DB") ?? throw new Exception();

                var card_dict = db_type.GetField("cards")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(card_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("Card"));

                var enemies_dict = db_type.GetField("enemies")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(enemies_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("AI"));

                var modifiers_dict = db_type.GetField("modifiers")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(modifiers_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("FightModifier"));

                var artifacts_dict = db_type.GetField("artifacts")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(artifacts_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("Artifact"));

                var midrowStuff_dict = db_type.GetField("midrowStuff")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(midrowStuff_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("StuffBase"));

                var backgrounds_dict = db_type.GetField("backgrounds")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(backgrounds_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("BG"));

                var maps_dict = db_type.GetField("maps")?.GetValue(null) as Dictionary<string, Type>;
                LoadAllSubclasses(maps_dict, CobaltCoreHandler.CobaltCoreAssembly.GetType("MapBase"));
            }
        }

        private static void LoadAllSubclasses(Dictionary<string, Type>? target, Type? lookup_type)
        {
            if (target == null || lookup_type == null)
                return;

            if (ModStorage == null)
            {
                Logger?.LogCritical("LoadAllSubclasses was called without a mod assembly storage. No data provided!");
            }
            else
            {
                Logger?.LogInformation($"Loading '{lookup_type.Name}' subclasses");

                //locate all attributes.
                var qualifying_types = ModStorage.ModLookup.Select(e => e.Item1).SelectMany(asembly => asembly.GetTypes().Where(my_type => my_type.IsClass && !my_type.IsAbstract && my_type.IsSubclassOf(lookup_type)));
                //filter those with ignore attribute
                qualifying_types = qualifying_types.Where(type => Attribute.GetCustomAttribute(type, typeof(IgnoreModComponentAttribute)) == null);
                //sort into dictionary
                var counter = 0;
                foreach (var type in qualifying_types)
                {
                    if (target.TryAdd(type.Name, type))
                    {
                        Logger?.LogDebug("Loaded:" + type.Name);
                        counter++;
                    }
                    else
                        Logger?.LogWarning($"Type {type.Name} was already in dictionary. Naming conflict!");
                }
                Logger?.LogInformation($"Successfully loaded {counter} subclasses of {lookup_type.Name} from {ModStorage.ModLookup.Count()} mod assemblies.");
            }



        }

    }
}
