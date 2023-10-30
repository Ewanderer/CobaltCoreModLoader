namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalPart
    {
        public ExternalPart(string globalName, object partObjectTemplate, ExternalSprite partSprite, ExternalSprite? partOffSprite = null)
        {
            GlobalName = globalName;
            PartObjectTemplate = partObjectTemplate;
            PartSprite = partSprite;
            PartOffSprite = partOffSprite;
        }

        protected ExternalPart(string globalName, ExternalSprite partSprite)
        {
            GlobalName = globalName;
            PartSprite = partSprite;
        }

        public string GlobalName { get; init; }

        public string Key => "@mod_part:" + GlobalName;
        public ExternalSprite? PartOffSprite { get; init; }
        public ExternalSprite PartSprite { get; init; }
        private object? PartObjectTemplate { get; init; }

        public virtual object GetPartObject() => PartObjectTemplate ?? throw new NotImplementedException("GetPartObject returned null.");
    }
}