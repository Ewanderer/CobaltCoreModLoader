using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Input;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Components.Services
{
    public class StoryRegistry : IStoryRegistry
    {
        private readonly ModAssemblyHandler modAssemblyHandler;
        private static ILogger<StoryRegistry>? logger;
        private static readonly Dictionary<string, ExternalStory> registeredStories = new Dictionary<string, ExternalStory>();
        private static readonly Dictionary<string, Tuple<MethodInfo, bool>> registeredCommands = new Dictionary<string, Tuple<MethodInfo, bool>>();
        private static readonly Dictionary<string, Tuple<MethodInfo, bool>> registeredChoices = new Dictionary<string, Tuple<MethodInfo, bool>>();
        public StoryRegistry(ILogger<StoryRegistry>? logger, ModAssemblyHandler mah)
        {
            StoryRegistry.logger = logger;
            modAssemblyHandler = mah;
        }
        
        public void LoadManifests()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.StoryManifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by StoryRegistry");
                }
            }

            //patching
        }
        public void RunLogic()
        {
            LoadManifests();
        }

        public static void PatchChoicesAndCommands()
        {
            var commands_dict = TypesAndEnums.DbType.GetField("storyCommands")?.GetValue(null) as IDictionary<string, MethodInfo> ?? throw new Exception("Cannot find DB.storyCommands");
            var choices_dict = TypesAndEnums.DbType.GetField("eventChoiceFns")?.GetValue(null) as IDictionary<string, MethodInfo> ?? throw new Exception("Cannot find DB.eventChoiceFns");

            foreach (KeyValuePair<string, Tuple<MethodInfo, bool>> choice in registeredChoices)
            {
                if(!choices_dict.TryAdd(choice.Key, choice.Value.Item1))
                {
                    if (!choice.Value.Item2)
                    {
                        logger?.LogCritical("Story Choice with key {0} already exists natively!", choice.Key);
                        continue;
                    }
                    choices_dict[choice.Key] = choice.Value.Item1;
                }
            }

            foreach (KeyValuePair<string, Tuple<MethodInfo, bool>> command in registeredCommands)
            {
                if (!commands_dict.TryAdd(command.Key, command.Value.Item1))
                {
                    if (!command.Value.Item2)
                    {
                        logger?.LogCritical("Story Choice with key {0} already exists natively!", command.Key);
                        continue;
                    }
                    commands_dict[command.Key] = command.Value.Item1;
                }
            }
        }

        public static void PatchStories()
        {
            var story = TypesAndEnums.DbType.GetField("story")?.GetValue(null) ?? throw new Exception("Cannot find DB.story");
            var stories_dict = TypesAndEnums.StoryType.GetField("all").GetValue(story) as IDictionary ?? throw new Exception("Cannot find Story.all");
            var node_lines_field_info = TypesAndEnums.StoryNodeType.GetField("lines") ?? throw new Exception("Cannot find StoryNode.lines");
            var node_type_field_info = TypesAndEnums.StoryNodeType.GetField("type") ?? throw new Exception("Cannot find StoryNode.type");

            foreach (ExternalStory s in registeredStories.Values)
            {
                if (s.storyNode == null)
                {
                    s.storyNode = Activator.CreateInstance(TypesAndEnums.StoryNodeType) ?? throw new Exception("Cannot create instance of class StoryNode");
                    node_type_field_info.SetValue(s.storyNode, TypesAndEnums.IntToNodeType(1));
                }

                if (s.storyNode.GetType() != TypesAndEnums.StoryNodeType)
                {
                    throw new Exception(String.Format("Cannot patch ExternalStory {0}, as ExternalStory.storyNode should be of class StoryNode", s.GlobalName));
                }

                var node_lines_field = node_lines_field_info.GetValue(s.storyNode) as IList ?? throw new Exception("Cannot find value of StoryNode.lines");

                object instruction;
                for (int i = 0; i < s.instructions.Count; i++)
                {
                    instruction = s.instructions[i];

                    if (instruction is ExternalStory.ExternalSay extSay)
                    {
                        dynamic nativeSay = Activator.CreateInstance(TypesAndEnums.SayType) ?? throw new Exception("Cannot create instance of class Say");
                        nativeSay.hash = extSay.hash;
                        nativeSay.who = extSay.who;
                        nativeSay.loopTag = extSay.loopTag;
                        nativeSay.ifCrew = extSay.ifCrew;
                        nativeSay.delay = extSay.delay;
                        nativeSay.choiceFunc = extSay.choiceFunc;
                        nativeSay.flipped = extSay.flipped;

                        instruction = nativeSay;
                    }
                    else
                    {
                        if (!instruction.GetType().IsSubclassOf(TypesAndEnums.InstructionType))
                        {
                            throw new Exception(String.Format("Cannot add instance of class {0} to Story Node {1} as it does not inherit from class Instruction", instruction.GetType().Name, s.GlobalName));
                        }
                    }

                    node_lines_field.Add(instruction);
                }
                
                stories_dict.Add(s.GlobalName, s.storyNode);
            }
        }

        internal static void PatchLocalisations(string locale, ref Dictionary<string, string> result)
        {
            foreach (var s in registeredStories.Values)
            {
                s.GetLocalisation(locale, out Dictionary<string, string> lines);

                foreach (KeyValuePair<string, string> line in lines)
                {
                    if (!result.TryAdd(line.Key, line.Value))
                        logger?.LogWarning("Story {0} cannot register line because key {1} already added somehow", s.GlobalName, line.Key);
                }
            }
        }

        public bool RegisterStory(ExternalStory story)
        {

            if (registeredStories.ContainsKey(story.GlobalName))
            {
                logger?.LogCritical("Story with global name {0} already registered!", story.GlobalName);
                return false;
            }

            registeredStories.Add(story.GlobalName, story);

            return true;
        }

        public bool RegisterChoice(string key, MethodInfo choice, bool intendedOverride = false)
        {
            var value = new Tuple<MethodInfo, bool>(choice, intendedOverride);
            if (!registeredChoices.TryAdd(key, value))
            {
                if (!intendedOverride)
                {
                    logger?.LogCritical("Story Choice with key {0} already registered!", key);
                    return false;
                }

                registeredChoices[key] = value;
            }

            return true;
        }

        public bool RegisterCommand(string key, MethodInfo command, bool intendedOverride = false)
        {
            var value = new Tuple<MethodInfo, bool>(command, intendedOverride);
            if (!registeredCommands.TryAdd(key, value))
            {
                if (!intendedOverride)
                {
                    logger?.LogCritical("Story Command with key {0} already registered!", key);
                    return false;
                }

                registeredChoices[key] = value;
            }

            return true;
        }
    }
}
