using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod.Artifacts
{
    [ArtifactMeta(owner = Deck.peri)]
    internal class PortableBlackHole : Artifact
    {
        public override Spr GetSprite()
        {
            return Spr.artifacts_AresCannon;
        }

        public override string Name()
        {
            return "Portable Black Hole";
        }

        public override string Description()
        {
            return "Creates black hole wherever you go.";
        }

        public override void OnCombatStart(State state, Combat combat)
        {
            combat.bg = new BGBlackHole();
            counter = 0;
        }

        int counter = 0;

        public override void OnTurnEnd(State state, Combat combat)
        {
            state.AddShake(1);
            combat.otherShip.NormalDamage(state, combat, counter, 0, false, true);
            state.ship.NormalDamage(state, combat, counter++, 0, false, true);
            state.AddShake(-1);
        }
    }
}
