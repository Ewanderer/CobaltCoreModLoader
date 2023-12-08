using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod.StoryStuff
{
    public static class DemoStoryChoices
    {
        public static List<Choice> DemoStoryChoice(State s)
        {
            List<Choice> choiceList = new List<Choice>();

            Choice choiceRandom = new Choice();

            choiceRandom.chance = .5; // equal chance to do the chance or normal actions;
            choiceRandom.actions.Add(new AHurt() { targetPlayer = true, hurtAmount = 1 });
            choiceRandom.actionsChance.Add(new AHeal() { targetPlayer = true, healAmount = 1 });
            choiceRandom.keyChance = "EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_0";
            choiceRandom.key = "EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_1";
            choiceRandom.label = "<c=choiceBold>50% to lose or gain 1 hull</c>";
            choiceList.Add(choiceRandom);

            choiceList.Add(new Choice() 
            { 
                key = "EWanderer.Demomod.DemoStory.ChoiceEvent_Outcome_2",
                label = "Nope !",
            });

            return choiceList;
        }
    }
}
