using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoMod.Artifacts
{
    internal class DemoShipArtifact : Artifact
    {

        public override void OnReceiveArtifact(State state)
        {
            state.GetCurrentQueue().Add(new AModifyShip() { parts = state.ship.parts.Where(p => (int)p.type != DemoShipManifest.DemoPartType?.Id).ToList() });
        }

    }
}
