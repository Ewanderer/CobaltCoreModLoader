using CobaltCoreModding.Definitions.ExternalItems;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface IDbRegistry : ICobaltCoreContact
    {
        ExternalSprite? GetModSprite(string globalName);

       
    }
}