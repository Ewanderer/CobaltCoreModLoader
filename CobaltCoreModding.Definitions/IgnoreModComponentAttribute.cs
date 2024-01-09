namespace CobaltCoreModding.Definitions
{
    /// <summary>
    /// The native loading functions of the mod loader will pass any class with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreModComponentAttribute : Attribute
    {
    }
}