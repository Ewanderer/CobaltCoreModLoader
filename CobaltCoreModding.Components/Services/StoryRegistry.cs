using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using Microsoft.Extensions.Logging;
using Mono.Cecil.Cil;
using System.Collections;
using System.Net.NetworkInformation;
using System.Reflection;

namespace CobaltCoreModding.Components.Services
{
    public class StoryRegistry : IStoryRegistry
    {
        private static readonly Dictionary<string, Tuple<MethodInfo, bool>> registeredChoices = new Dictionary<string, Tuple<MethodInfo, bool>>();
        private static readonly Dictionary<string, Tuple<MethodInfo, bool>> registeredCommands = new Dictionary<string, Tuple<MethodInfo, bool>>();
        private static readonly Dictionary<string, ExternalStory> registeredStories = new Dictionary<string, ExternalStory>();
        private static readonly List<ExternalStoryInjector> registeredInjectors = new List<ExternalStoryInjector>();
        private static ILogger<StoryRegistry>? logger;
        private readonly ModAssemblyHandler modAssemblyHandler;

        private static FieldInfo? _saySwitch_field_info;

        private static FieldInfo SaySwitchLinesFielInfo
        {  
            get
            {
                if (_saySwitch_field_info == null)
                {
                    _saySwitch_field_info = TypesAndEnums.SaySwitchType.GetField("lines") ?? throw new Exception("Cannot find SaySwitch.lines");
                }

                return _saySwitch_field_info;
            }
        }

        public StoryRegistry(ILogger<StoryRegistry>? logger, ModAssemblyHandler mah)
        {
            StoryRegistry.logger = logger;
            modAssemblyHandler = mah;
        }

        public static void PatchChoicesAndCommands()
        {
            var commands_dict = TypesAndEnums.DbType.GetField("storyCommands")?.GetValue(null) as IDictionary<string, MethodInfo> ?? throw new Exception("Cannot find DB.storyCommands");
            var choices_dict = TypesAndEnums.DbType.GetField("eventChoiceFns")?.GetValue(null) as IDictionary<string, MethodInfo> ?? throw new Exception("Cannot find DB.eventChoiceFns");

            foreach (KeyValuePair<string, Tuple<MethodInfo, bool>> choice in registeredChoices)
            {
                if (!choices_dict.TryAdd(choice.Key, choice.Value.Item1))
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
            var stories_dict = TypesAndEnums.StoryType.GetField("all")?.GetValue(story) as IDictionary ?? throw new Exception("Cannot find Story.all");
            var node_lines_field_info = TypesAndEnums.StoryNodeType.GetField("lines") ?? throw new Exception("Cannot find StoryNode.lines");
            

            foreach (ExternalStory s in registeredStories.Values)
            {
                //overwrite instructions.
                if (s.Instructions != null)
                {
                    var node_lines_field = node_lines_field_info.GetValue(s.StoryNode) as IList ?? throw new Exception("Cannot find value of StoryNode.lines");
                    node_lines_field.Clear();

                    foreach (var instruction in s.Instructions)
                    {
                        if (instruction is ExternalStory.ExternalSay extSay)
                        {
                            node_lines_field.Add(ExternalToNativeSay(extSay));
                        }
                        else if (instruction is ExternalStory.ExternalSaySwitch extSaySwitch)
                        {
                            node_lines_field.Add(ExternalToNativeSaySwitch(extSaySwitch));
                        }
                        else
                        {
                            if (!instruction.GetType().IsSubclassOf(TypesAndEnums.InstructionType))
                            {
                                throw new Exception(String.Format("Cannot add instance of class {0} to Story Node {1} as it does not inherit from class Instruction", instruction.GetType().Name, s.GlobalName));
                            }
                            node_lines_field.Add(instruction);
                        }
                    }
                }

                stories_dict.Add(s.GlobalName, s.StoryNode);
            }

            foreach (ExternalStoryInjector injector in registeredInjectors)
            {
                if (!stories_dict.Contains(injector.StoryName))
                {
                    throw new Exception(String.Format("Cannot find story node {0} for injection", injector.StoryName));
                }

                var injectedNode = stories_dict[injector.StoryName];

                if (injector.advancedInjector != null)
                {
                    injector.advancedInjector.Invoke(injectedNode);
                }
                else if (injector.Instructions != null)
                {
                    var injected_node_lines = node_lines_field_info.GetValue(injectedNode) as IList ?? throw new Exception("Cannot find value of StoryNode.lines");
                    dynamic injected_instructions = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypesAndEnums.InstructionType), new object[] { });

                    foreach (var instruction in injector.Instructions)
                    {
                        if (instruction is ExternalStory.ExternalSay extSay)
                        {
                            injected_instructions.Add(ExternalToNativeSay(extSay));
                        }
                        else if (instruction is ExternalStory.ExternalSaySwitch extSaySwitch)
                        {
                            injected_instructions.Add(ExternalToNativeSaySwitch(extSaySwitch));
                        }
                        else
                        {
                            if (!instruction.GetType().IsSubclassOf(TypesAndEnums.InstructionType))
                            {
                                throw new Exception(String.Format("Cannot add instance of class {0} to Story Node {1} as it does not inherit from class Instruction", instruction.GetType().Name, injector.StoryName));
                            }
                            injected_instructions.Add(instruction);
                        }
                    }

                    int counter = 0;
                    switch (injector.quickInjection)
                    {
                        case ExternalStoryInjector.QuickInjection.SaySwitch:
                            for (int i = 0; i < injected_node_lines.Count; i++)
                            {
                                if (injected_node_lines[i].GetType() == TypesAndEnums.SaySwitchType)
                                {
                                    if (counter == injector.targetIndex)
                                    {
                                        var saySwitchLines = SaySwitchLinesFielInfo.GetValue(injected_node_lines[i]) as IList ?? throw new Exception("Cannot find value of field SaySwitch.lines");
                                        InsertRange(0, saySwitchLines, injected_instructions);
                                        break;
                                    }
                                    counter++;
                                }
                            }
                            break;
                        case ExternalStoryInjector.QuickInjection.Beginning:
                            InsertRange(Math.Min(injector.targetIndex, injected_node_lines.Count), injected_node_lines, injected_instructions);
                            break;
                        case ExternalStoryInjector.QuickInjection.End:
                            InsertRange(Math.Max(injected_node_lines.Count - injector.targetIndex, 0), injected_node_lines, injected_instructions);
                            break;
                    }
                }
            }
        }

        private static void InsertRange (int index, IList list, IEnumerable items)
        {
            foreach (var item in items)
            {
                list.Insert(index, item);
                index++;
            }
        }

        //convert external says to native say.
        private static dynamic ExternalToNativeSay(ExternalStory.ExternalSay extSay)
        {
            dynamic nativeSay = Activator.CreateInstance(TypesAndEnums.SayType) ?? throw new Exception("Cannot create instance of class Say");
            nativeSay.hash = extSay.Hash;
            nativeSay.who = extSay.Who;
            nativeSay.loopTag = extSay.LoopTag;
            nativeSay.ifCrew = extSay.IfCrew;
            nativeSay.delay = extSay.Delay;
            nativeSay.choiceFunc = extSay.ChoiceFunc;
            nativeSay.flipped = extSay.Flipped;

            return nativeSay;
        }

        private static dynamic ExternalToNativeSaySwitch(ExternalStory.ExternalSaySwitch extSaySwitch)
        {
            dynamic nativeSwitch = Activator.CreateInstance(TypesAndEnums.SaySwitchType) ?? throw new Exception("Cannot create instance of class SaySwitch");
            dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(TypesAndEnums.SayType), new object[] { });

            foreach (var saySwitchInstruction in extSaySwitch.lines)
            {
                if (saySwitchInstruction is ExternalStory.ExternalSay)
                {
                    list.Add(ExternalToNativeSay(saySwitchInstruction));
                }
                else if (saySwitchInstruction.GetType().IsSubclassOf(TypesAndEnums.SayType))
                {
                    list.Add(saySwitchInstruction);
                }
                else
                {
                    throw new Exception(String.Format("Cannot add instance of class {0} to SaySwitch as it does not inherit from class Say", saySwitchInstruction.GetType().Name));
                }
            }

            SaySwitchLinesFielInfo.SetValue(nativeSwitch, list);
            return nativeSwitch;
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

        public bool RegisterStory(ExternalStory story)
        {
            if (string.IsNullOrWhiteSpace(story.GlobalName))
            {
                logger?.LogCritical("Attempted to register story with no global namme!");
                return false;
            }

            if (registeredStories.ContainsKey(story.GlobalName))
            {
                logger?.LogCritical("Story with global name {0} already registered!", story.GlobalName);
                return false;
            }

            if (story.StoryNode.GetType() != TypesAndEnums.StoryNodeType)
            {
                logger?.LogCritical("Cannot patch ExternalStory {0}, as ExternalStory.storyNode should be of class StoryNode", story.GlobalName);
                return false;
            }

            //validate lines
            if (story.Instructions != null)
            {
                foreach (var instruction in story.Instructions)
                {
                    if (instruction is ExternalStory.ExternalSay)
                    {
                        continue;
                    }
                    if (instruction is ExternalStory.ExternalSaySwitch saySwitch)
                    {
                        foreach (var switchInstruction in saySwitch.lines)
                        {
                            if (switchInstruction is ExternalStory.ExternalSay)
                            {
                                continue;
                            }
                            if (!switchInstruction.GetType().IsSubclassOf(TypesAndEnums.SayType))
                            {
                                logger?.LogWarning("Cannot add instance of class {0} to ExternalSaySwitch in Story Node {1} as it does not inherit from class Say. It is also not an externalStory.Say instruction type", instruction.GetType().Name, story.GlobalName);
                                return false;
                            }
                        }
                        continue;
                    }
                    if (!instruction.GetType().IsSubclassOf(TypesAndEnums.InstructionType))
                    {
                        logger?.LogWarning("Cannot add instance of class {0} to Story Node {1} as it does not inherit from class Instruction. It is also not an external.story instruction type", instruction.GetType().Name, story.GlobalName);
                        return false;
                    }
                }
            }
            registeredStories.Add(story.GlobalName, story);

            return true;
        }

        public bool RegisterInjector(ExternalStoryInjector injector)
        {
            if (string.IsNullOrWhiteSpace(injector.StoryName))
            {
                logger?.LogCritical("Attempted to register injector with no target!");
                return false;
            }

            //validate lines
            if (injector.Instructions != null)
            {
                if (injector.quickInjection == ExternalStoryInjector.QuickInjection.SaySwitch)
                {
                    foreach (var instruction in injector.Instructions)
                    {
                        if (instruction is ExternalStory.ExternalSay)
                        {
                            continue;
                        }
                        if (!instruction.GetType().IsSubclassOf(TypesAndEnums.SayType))
                        {
                            logger?.LogWarning("Cannot inject instance of class {0} to SaySwitch in StoryNode {1} as it does not inherit from class Say. It is also not an ExternalStory.ExternalSay instruction type", instruction.GetType().Name, injector.StoryName);
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (var instruction in injector.Instructions)
                    {
                        if (instruction is ExternalStory.ExternalSay)
                        {
                            continue;
                        }
                        if (instruction is ExternalStory.ExternalSaySwitch saySwitch)
                        {
                            foreach (var switchInstruction in saySwitch.lines)
                            {
                                if (switchInstruction is ExternalStory.ExternalSay)
                                {
                                    continue;
                                }
                                if (!switchInstruction.GetType().IsSubclassOf(TypesAndEnums.SayType))
                                {
                                    logger?.LogWarning("Cannot add instance of class {0} to ExternalSaySwitch in Story Node {1} as it does not inherit from class Say. It is also not an externalStory.Say instruction type", instruction.GetType().Name, injector.StoryName);
                                    return false;
                                }
                            }
                        }
                        if (!instruction.GetType().IsSubclassOf(TypesAndEnums.InstructionType))
                        {
                            logger?.LogWarning("Cannot inject instance of class {0} to Story Node {1} as it does not inherit from class Instruction. It is also not an external.story instruction type", instruction.GetType().Name, injector.StoryName);
                            return false;
                        }
                    }
                }
            }
            registeredInjectors.Add(injector);

            return true;
        }

        public void RunLogic()
        {
            LoadManifests();
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

            foreach (var i in registeredInjectors)
            {
                i.GetLocalisation(locale, out Dictionary<string, string> lines);

                foreach (KeyValuePair<string, string> line in lines)
                {
                    if (!result.TryAdd(line.Key, line.Value))
                        logger?.LogWarning("Story {0} cannot register line because key {1} already added somehow", i.StoryName, line.Key);
                }
            }
        }
    }
}