using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalShip
    {
        public string GlobalName { get; init; }

        private object? shipObjectTemplate { get; init; }

        public virtual object GetShipObject() => shipObjectTemplate ?? throw new Exception("GetShipObject returned null. Forgot proper overwrite?");

        public string underChassisKey => "@mod_ship_under:" + GlobalName;
        public string overChassisKey => "@mod_ship_over:" + GlobalName;

        public IEnumerable<ExternalPart> Parts { get; init; }

        public ExternalSprite? ChassisUnderSprite { get; init; }
        public ExternalSprite? ChassisOverSprite { get; init; }

        public ExternalShip(string globalName, object shipObjectTemplate, IEnumerable<ExternalPart> parts, ExternalSprite? chassisUnderSprite = null, ExternalSprite? chassisOverSprite = null)
        {
            GlobalName = globalName;
            this.shipObjectTemplate = shipObjectTemplate;
            Parts = parts;
            ChassisUnderSprite = chassisUnderSprite;
            ChassisOverSprite = chassisOverSprite;
        }

        protected ExternalShip(string globalName, IEnumerable<ExternalPart> parts)
        {
            GlobalName = globalName;
            this.shipObjectTemplate = null;
            Parts = parts;

        }
    }
}
