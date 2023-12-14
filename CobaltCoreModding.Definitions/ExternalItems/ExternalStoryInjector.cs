using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CobaltCoreModding.Definitions.ExternalItems.ExternalStory;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalStoryInjector
    {
        private readonly Dictionary<string, Dictionary<string, string>> localisations = new Dictionary<string, Dictionary<string, string>>();
        public string StoryName { get; init; }
        public int targetIndex;
        public QuickInjection quickInjection;
        public IEnumerable<object>? Instructions;

        public delegate void InjectionMethod(object node);
        public InjectionMethod? advancedInjector;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyName">the global name of the existing story in which you want to inject instructions.</param>
        /// <param name="quickInjection">Define the relative place where the injection will take place.</param>
        /// <param name="targetIndex">A general parameter that serves as offset for the injection. offset toward the end for QuickInjection.Beginning, offset toward the beginning for QuickInjection.End, or skips Sayswitches for QuickInjection.SaySwitch </param>
        /// <param name="instructions">instructions which will be injected into the story. mix of native instruction objects and externalstory instructions like externalsay. QuickInjection.SaySwitch only accepts Say and ExternalSay instructions !</param>
        public ExternalStoryInjector(string storyName, QuickInjection quickInjection, int targetIndex, IEnumerable<object> instructions)
        {
            StoryName = storyName;
            this.targetIndex = targetIndex;
            this.quickInjection = quickInjection;

            this.Instructions = instructions.ToArray();
            if (Instructions != null)
            {
                //have instructions based translation registered.
                foreach (var instruction in this.Instructions)
                {
                    if (instruction is ExternalSay say)
                    {
                        AddLocalisation(say.Hash, say.What);
                    }
                    if (instruction is ExternalSaySwitch sswitch)
                    {
                        foreach (ExternalSay extSay in sswitch.lines)
                        {
                            AddLocalisation(extSay.Hash, extSay.What);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyName">the global name of the existing story in which you want to inject instructions.</param>
        public ExternalStoryInjector(string storyName, InjectionMethod injector) 
        {
            StoryName = storyName;
            this.advancedInjector = injector;
        }

        public enum QuickInjection
        {
            Beginning,
            End,
            SaySwitch
        }

        /// <summary>
        /// Use to add non native localisations for an instruction.
        /// </summary>
        /// <param name="lineHash">the hash of the line</param>
        /// <param name="text"></param>
        /// <param name="localisation"></param>
        public void AddLocalisation(string lineHash, string text, string localisation = "en")
        {
            localisations.TryAdd(localisation, new Dictionary<string, string>());
            string key = String.Join(":", StoryName, lineHash);
            if (!localisations[localisation].TryAdd(key, text))
                localisations[localisation][key] = text;
        }

        public void GetLocalisation(string localisation, out Dictionary<string, string> lines)
        {
            Dictionary<string, string>? maybe_lines;
            if (!localisations.TryGetValue(localisation, out maybe_lines))
                if (!localisations.TryGetValue("en", out maybe_lines))
                {
                    lines = new Dictionary<string, string>();
                    return;
                }
            lines = maybe_lines;
        }
    }
}
