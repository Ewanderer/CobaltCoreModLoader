using CobaltCoreModding.Definitions;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using DemoMod.StoryStuff;
using Microsoft.Extensions.Logging;

namespace DemoMod
{
    internal class DemoStoryManifest : IStoryManifest
    {
        public static ExternalPartType? DemoPartType { get; private set; }
        public IEnumerable<DependencyEntry> Dependencies => new DependencyEntry[0];
        public DirectoryInfo? GameRootFolder { get; set; }
        public ILogger? Logger { get; set; }
        public DirectoryInfo? ModRootFolder { get; set; }
        public string Name => "EWanderer.Demomod.DemoStoryManifest";

        public void LoadManifest(IStoryRegistry storyRegistry)
        {
            // A combat shout
            var exampleShout = new ExternalStory("EWanderer.Demomod.DemoStory.CombatShout",
                new StoryNode() // Native CobaltCore class, containing numerous options regarding the shout's trigger. Listed are only the most common, but feel free to explore
                {
                    type = NodeType.combat, // Mark the story as a combat shout
                    priority = true, // Forces this story to be selected before other valid ones when the database is queried, useful for debugging.

                    once = false,
                    oncePerCombat = false,
                    oncePerRun = false, // Self explanatory

                    lookup = new HashSet<string>() // This is a list of tags that queries look for in various situations, very useful for triggering shouts in specific situations !
                    {
                        "demoCardShout" // We'll feed this string to a CardAction's dialogueSelector field in EWandererDemoCard, so that this shout triggers when we play the upgrade B of the card
                    },
                    
                    allPresent = new HashSet<string>() // this checks for the presence of a list of characters.
                    {
                        "riggs"
                    }
                },
                new List<object>() /* this is the actual dialogue. You can feed this list :
                                    * classes inheriting from Instruction (natively Command, Say, or Sayswitch)
                                    * ExternalStory.ExternalSay, which act as a native Say, but automating the more tedious parts,
                                    * such as localizing and hashing*/
                {
                    new ExternalStory.ExternalSay()
                    {
                        Who = "riggs", /* the character that talks. For modded characters, use CharacterDeck.GlobalName
                                        * attempting to make an absent character speak in combat will interrupt the shout !*/
                        What = "Example shout !",
                        LoopTag = "squint" // the specific animation that should play during the shout. "neutral" is default
                    },
                    new Say() // same as above, but native
                    {
                        who = "peri",
                        hash = "0" // a string that must be unique to your story, used to fetch localisation 
                    },
                    new ExternalStory.ExternalSaySwitch( new List<ExternalStory.ExternalSay>() // this is used to randomly pick a valid options among the listed Says.
                    {
                        new ExternalStory.ExternalSay()
                        {
                            Who = "dizzy",
                            What = "A !",
                            LoopTag = "squint" 
                        },
                        new ExternalStory.ExternalSay()
                        {
                            Who = "eunice",
                            What = "B !",
                            LoopTag = "squint"
                        },
                        new ExternalStory.ExternalSay()
                        {
                            Who = "goat",
                            What = "C !",
                            LoopTag = "squint"
                        },
                    }) 
                                     
                }
            );
            exampleShout.AddLocalisation("0", "Example native shout !"); // setting the localisation for peri's shout using the native way

            storyRegistry.RegisterStory(exampleShout);

            var exampleEvent = new ExternalStory("EWanderer.Demomod.DemoStory.ChoiceEvent",
                    node: new StoryNode()
                    {
                        type = NodeType.@event,// Mark the story as an event
                        priority = true,
                        canSpawnOnMap = true, // self explanatory, dictate whether the event can be a [?] node on the map

                        zones = new HashSet<string>() // dictate in which zone of the game the event can trigger.
                        {
                            "zone_first"
                            //"zone_lawless"
                            //"zone_three"
                            //"zone_magic"
                            //"zone_finale"
                        },

                        /*lookup = new HashSet<string>() 
                        {
                            Lookup for event have some interesting functionalities. For exemple, the tag before_[EnemyName] or after_[EnemyName] will
                            make it so the event triggers right before or after said enemy combat, as done with the mouse knight guy for example
                        },*/

                        choiceFunc = "demoChoiceFunc", /* This triggers a registered choice function at the end of the dialogue.
                                                        * You can see vanilla examples in the class Events*/
                    },
                    instructions: new List<object>()
                    {
                        new ExternalStory.ExternalSay()
                        {
                            Who = "walrus", // characters in event dialogues don't need to be actually present !
                            What = "Example event start !",
                            Flipped = true, // if true, the character is on the right side of the dialogue while talking
                        },
                        new Command(){name = "demoDoStuffCommand"}, /* execute a registered method, only works during dialogues.
                                                                     * You can see vanilla examples in the class StoryCommands*/
                        new ExternalStory.ExternalSay()
                        {
                            Who = "comp",
                            What = "Ouch !",
                        },
                    }
                );

            storyRegistry.RegisterChoice("demoChoiceFunc", typeof(DemoStoryChoices).GetMethod(nameof(DemoStoryChoices.DemoStoryChoice))!);
            storyRegistry.RegisterCommand("demoDoStuffCommand", typeof(DemoStoryCommands).GetMethod(nameof(DemoStoryCommands.DemoStoryCommand))!);
            storyRegistry.RegisterStory(exampleEvent);

            var exampleEventOutcome_0 = new ExternalStory("EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_0",
                    node: new StoryNode()
                    {
                        type = NodeType.@event,
                    },
                    instructions: new List<object>()
                    {
                        new ExternalStory.ExternalSay()
                        {
                            Who = "comp",
                            What = "Yay !",
                        },
                    }
                );
            storyRegistry.RegisterStory(exampleEventOutcome_0);

            var exampleEventOutcome_1 = new ExternalStory("EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_1",
                    node: new StoryNode()
                    {
                        type = NodeType.@event,
                    },
                    instructions: new List<object>()
                    {
                        new ExternalStory.ExternalSay()
                        {
                            Who = "comp",
                            What = "That hurts !",
                        },
                    }
                );
            storyRegistry.RegisterStory(exampleEventOutcome_1);

            var exampleEventOutcome_2 = new ExternalStory("EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_2",
                    node: new StoryNode()
                    {
                        type = NodeType.@event
                    },
                    instructions: new List<object>()
                    {
                        new ExternalStory.ExternalSay()
                        {
                            Who = "comp",
                            What = "Let's scoot !",
                        },
                    }
                );
            storyRegistry.RegisterStory(exampleEventOutcome_2);

            // Story injectors below, allows you to actively edit/insert stuff from existing story in a non destructive way !

            var injector = new ExternalStoryInjector("AbandonedShipyard", // the story in which to insert stuff
                                                    ExternalStoryInjector.QuickInjection.Beginning, // from where to insert stuff. SaySwitch will insert your stuff in the nth SaySwitch, where n is targetIndex 
                                                    1, // an index that offset the quick injection location. in this case, we're inserting 1 away from the beginning.
                                                    new List<object>() // the stuff you want to insert
            {
                new ExternalStory.ExternalSay()
                {
                    Who = "comp",
                    What = "Hey, i'm not supposed to say that !"
                }
            });

            storyRegistry.RegisterInjector(injector);


            // Advanced injectors let you hook a method that will receive the StoryNode during DB injection time, allowing you to modify it however you wish !

            var advancedInjector = new ExternalStoryInjector("AbandonedShipyard", (node) =>
            {
                StoryNode s = node as StoryNode;


            });
            storyRegistry.RegisterInjector(advancedInjector);
        }
    }
}
