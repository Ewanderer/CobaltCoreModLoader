namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalShip
    {
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

        public ExternalSprite? ChassisOverSprite { get; init; }
        public ExternalSprite? ChassisUnderSprite { get; init; }
        public string GlobalName { get; init; }

        public string overChassisKey => "@mod_ship_over:" + GlobalName;
        public IEnumerable<ExternalPart> Parts { get; init; }
        public string underChassisKey => "@mod_ship_under:" + GlobalName;
        private object? shipObjectTemplate { get; init; }

        public virtual object GetShipObject() => shipObjectTemplate ?? throw new Exception("GetShipObject returned null. Forgot proper overwrite?");
    }
}