namespace CobaltCoreModding.Definitions.ModManifests
{
    public interface IApiProviderManifest : IManifest
    {
        /// <summary>
        /// Provide an API that other mods can use. All public methods will be exposed to these mods.
        /// </summary>
        /// <param name="requestingMod">The mod that requested the API.</param>
        /// <returns>The object whose public methods will be exposed to other mods.</returns>
        object? GetApi(IManifest requestingMod) => this;
    }
}
