using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using Microsoft.Extensions.Logging;
using Nanoray.Pintail;
using System.Reflection;
using System.Reflection.Emit;

namespace CobaltCoreModding.Components.Services
{
    internal class PerModModLoaderContact : IModLoaderContact, IPrelaunchContactPoint
    {
        private record struct ModApiKey(
            string ModName,
            Type ApiType
        );

        private readonly ModAssemblyHandler modAssemblyHandler;
        private readonly ILogger<ModAssemblyHandler> logger;
        private readonly IManifest modManifest;

        private readonly Dictionary<ModApiKey, object?> modApis = new();
        private readonly IProxyManager<string> proxyManager;

        internal PerModModLoaderContact(ModAssemblyHandler modAssemblyHandler, ILogger<ModAssemblyHandler> logger, IManifest modManifest)
        {
            this.modAssemblyHandler = modAssemblyHandler;
            this.logger = logger;
            this.modManifest = modManifest;

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"CobaltCoreModding.Proxies, Version={this.GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("CobaltCoreModding.Proxies");
            proxyManager = new ProxyManager<string>(moduleBuilder, new ProxyManagerConfiguration<string>(
                proxyPrepareBehavior: ProxyManagerProxyPrepareBehavior.Eager,
                proxyObjectInterfaceMarking: ProxyObjectInterfaceMarking.Disabled
            ));
        }

        public IEnumerable<IManifest> LoadedManifests => modAssemblyHandler.LoadedManifests;

        public Assembly CobaltCoreAssembly => modAssemblyHandler.CobaltCoreAssembly;

        public IManifest LookupManifest(string globalName) => ModAssemblyHandler.LookupManifest(globalName) ?? throw new KeyNotFoundException();

        public bool RegisterNewAssembly(Assembly assembly, DirectoryInfo working_directory) => modAssemblyHandler.RegisterNewAssembly(assembly, working_directory);

        public TApi? GetApi<TApi>(string modName) where TApi : class
        {
            ModApiKey key = new(modName, typeof(TApi));
            if (modApis.TryGetValue(key, out var api))
                return api as TApi;

            logger.LogTrace("Mod {Client} is trying to access {Provider} mod's API.", modManifest.Name, modName);
            api = CreateApi<TApi>(modName);
            modApis[key] = api;
            return api as TApi;
        }

        private TApi? CreateApi<TApi>(string modName) where TApi : class
        {
            if (!typeof(TApi).IsInterface)
            {
                logger.LogError("Failed to load {Provider} mod's API with the API interface {ApiType} provided by mod {Client}. The type is not an interface.", modName, typeof(TApi), modManifest.Name);
                return null;
            }

            var apiProviderManifest = ModAssemblyHandler.ApiProviderManifests.FirstOrDefault(manifest => manifest.Name == modName);
            if (apiProviderManifest is null)
            {
                logger.LogError("Failed to load {Provider} mod's API with the API interface {ApiType} provided by mod {Client}. The type is not an interface.", modName, typeof(TApi), modManifest.Name);
                return null;
            }

            var api = apiProviderManifest.GetApi(modManifest);
            if (api is null)
            {
                logger.LogError("Failed to load {Provider} mod's API with the API interface {ApiType} provided by mod {Client}. The type is not an interface.", modName, typeof(TApi), modManifest.Name);
                return null;
            }

            try
            {
                return proxyManager.ObtainProxy<string, TApi>(api, apiProviderManifest.Name, modManifest.Name);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to load {Provider} mod's API with the API interface {ApiType} provided by mod {Client}.\nException: {ex}", modName, typeof(TApi), modManifest.Name, ex);
                return null;
            }
        }
    }
}