using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CobaltCoreModLoaderApp
{
    public static class Program
    {
        // [STAThread]
        private static void Main(string[] args)
        {
            //build cobalt core
            var modded_cobalt_core_builder = new HostApplicationBuilder();

            modded_cobalt_core_builder.Services.AddSingleton<SettingService>();
            modded_cobalt_core_builder.Services.AddSingleton<CobaltCoreHandler>();
            modded_cobalt_core_builder.Services.AddSingleton<DBExtender>();
            modded_cobalt_core_builder.Services.AddSingleton<ModAssemblyHandler>();
            modded_cobalt_core_builder.Services.AddSingleton<SpriteExtender>();
            modded_cobalt_core_builder.Services.AddSingleton<AnimationRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<DeckRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<CardRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<CardOverwriteRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<CharacterRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<GlossaryRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<ArtifactRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<StatusRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<CustomEventHub>();
            modded_cobalt_core_builder.Services.AddSingleton<PartRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<ShipRegistry>();
            modded_cobalt_core_builder.Services.AddSingleton<StarterShipRegistry>();

            //actualize cobalt core modded app.
            var modded_cobalt_core_app = modded_cobalt_core_builder.Build() ?? throw new Exception();

            var loader_ui = new LauncherUI(modded_cobalt_core_app);

            var mcca = modded_cobalt_core_app.RunAsync();

            loader_ui.Launch();

            loader_ui.WaitTillClosed().Wait();
            mcca.Wait();
        }
    }
}