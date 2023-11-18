namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalPartType
    {
        private int? id;

        public ExternalPartType(string globalName)
        {
            GlobalName = globalName;
        }

        public string GlobalName { get; init; }
        public int? Id
        { get => id; set { if (id != null) throw new Exception("Id already assigned!"); id = value; } }
    }
}