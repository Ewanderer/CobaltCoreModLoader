﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod.Actions
{
    public class EWandererDemoAction : CardAction
    {

        internal static string glossary_item = "";

        public override void Begin(G g, State s, Combat c)
        {
            c.energy += 99;
        }

        public override Icon? GetIcon(State s) => new Icon(Spr.icons_ace,42,Colors.attackFail);

        public override List<Tooltip> GetTooltips(State s)
        {
            List<Tooltip> tooltips = new List<Tooltip>();

            tooltips.Add(new TTGlossary(glossary_item, Array.Empty<object>()));

            return tooltips;
        }

    }
}
