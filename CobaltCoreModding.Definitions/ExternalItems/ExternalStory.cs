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

        public List<object> instructions;
        public object? storyNode;

        public string GlobalName { get; init; }

        public ExternalStory(string globalName, object? node = null, List<object>? instructions = null)
        {
            GlobalName = globalName;
            this.storyNode = node;
            this.instructions = instructions ?? new List<object>();

            foreach (var instruction in instructions)
            {
                if (instruction is ExternalSay say)
                {
                    say.hash = QuickHash(say.who + ":" + say.what);
                    AddLocalisation(say.hash, say.what);
                }
            }
        }

        public class ExternalSay
        {
            public string? hash;
            public string what = "";
            public string who = "";
            public bool flipped;
            public string? loopTag;
            public string? choiceFunc;
            public bool ifCrew;
            public double delay;
        }

        private static string QuickHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var inputHash = SHA256.HashData(inputBytes);
            return Convert.ToHexString(inputHash).Substring(0,8);
        }

        public void AddLocalisation(string lineHash, string text, string localisation = "en")
        {
            localisations.TryAdd(localisation, new Dictionary<string, string>());

            string key = String.Join(":", GlobalName, lineHash);
            if (!localisations[localisation].TryAdd(key, text))
                localisations[localisation][key] = text;
        }

        public void GetLocalisation(string localisation, out Dictionary<string, string> lines)
        {
            if (!localisations.TryGetValue(localisation, out lines))
                if (!localisations.TryGetValue("en", out lines))
                    lines = new Dictionary<string, string>();
        }
    }
}
