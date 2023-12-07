using DemoMod.Actions;

namespace DemoMod.Cards
{
    [CardMeta(deck = Deck.riggs, rarity = Rarity.common, upgradesTo = new Upgrade[] { Upgrade.A, Upgrade.B })]
    public class EWandererDemoCard : Card
    {
        internal static Spr card_sprite = Spr.cards_GoatDrone;

        public override List<CardAction> GetActions(State s, Combat c)
        {
            var list = new List<CardAction>();
            switch (this.upgrade)
            {
                case Upgrade.None:
                    list.Add(new ADrawCard() { count = 100 });
                    list.Add(new AAttack() { damage = 10, fast = true });
                    list.Add(new EWandererDemoAction());
                    list.Add(new AStatus() { targetPlayer = true, status = (Status)(ModManifest.demo_status?.Id ?? throw new NullReferenceException()), statusAmount = 2 });
                    break;

                case Upgrade.A:
                    list.Add(new AHeal { healAmount = 100, targetPlayer = true });
                    break;

                case Upgrade.B:
                    list.Add(new ACorrodeDamage() { dialogueSelector = ".demoCardShout"}); /* notice the "." It is placed before a lookup tag to notify the game that it is one.
                                                                                            * Without it, the story queries would try to match the string to an exact global name */
                    break;

                case (Upgrade)3:
                    list.Add(new ABubbleField() { });
                    break;
            }

            return list;
        }

        public override CardData GetData(State state) => new CardData
        {
            cost = 0,
            art = new Spr?(card_sprite),
        };

        public override string Name() => "EWDemoCard";
    }
}