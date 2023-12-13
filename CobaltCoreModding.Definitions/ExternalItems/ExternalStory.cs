using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CobaltCoreModding.Definitions.ExternalItems
{

    public class ExternalStory
    {
        private readonly Dictionary<string, Dictionary<string, string>> localisations = new Dictionary<string, Dictionary<string, string>>();

        public IEnumerable<object>? Instructions;
        /// <summary>
        /// a native cobalt core story object
        /// </summary>
        public object StoryNode;

        public string GlobalName { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalName">the global name of the story, must be unique amongst all external stories.</param>
        /// <param name="node">A CC StoryNode object.</param>
        /// <param name="instructions">instructions which will overwrite node.instructions. mix of native instruction objects and externalstory instructions like externalsay.</param>
        public ExternalStory(string globalName, object node, IEnumerable<object>? instructions = null)
        {
            GlobalName = globalName;
            this.StoryNode = node;

            this.Instructions = instructions?.ToArray();
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

        public class ExternalSaySwitch
        {
            public List<ExternalSay> lines;

            public ExternalSaySwitch(List<ExternalSay> lines)
            {
                this.lines = lines;
            }
        }

        public class ExternalSay
        {
            /// <summary>
            /// cache of hash to speedup use.
            /// </summary>
            private string? hash;

            /// <summary>
            /// The hash to link it up with localisations.
            /// </summary>
            public string Hash => hash ??= QuickHash($"{Who}:{What}");
            /// <summary>
            /// English localisation. 
            /// for other localisations, use externalstory.addlocalisation(hash,text,loc)
            /// </summary>
            public string What = "";
            /// <summary>
            /// the identifaction of the speaker. use a native characters use their name, for modded characters use externaldeck.globalname.
            /// </summary>
            public string Who = "";
            /// <summary>
            ///  if true, the character is on the right side of the dialogue while talking
            /// </summary>
            public bool Flipped;
            /// <summary>
            /// name of the animation used to for this line.
            /// </summary>
            public string? LoopTag;
            /// <summary>
            /// name of the choice func to be used at the end of dialouge.
            /// </summary>
            public string? ChoiceFunc;
            /// <summary>
            /// 
            /// </summary>
            public bool IfCrew;
            /// <summary>
            /// 
            /// </summary>
            public double Delay;

            /// <summary>
            /// Self generating the hash.
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            private static string QuickHash(string input)
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var inputHash = SHA256.HashData(inputBytes);
                return Convert.ToHexString(inputHash).Substring(0, 8);
            }
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
            string key = String.Join(":", GlobalName, lineHash);
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
