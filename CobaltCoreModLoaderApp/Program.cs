using CobaltCoreModLoader.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CobaltCoreModLoaderApp
{
    public static class Program
    {
        [STAThread]
        private async static Task<int> Main(string[] args)
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


            //actualize
            var modded_cobalt_core_app = modded_cobalt_core_builder.Build() ?? throw new Exception();

            var loader_app_builder = WpfApplication<LoaderApp, LoaderMainWindow>.CreateBuilder(args);
            //cross reference the mooded cobalt core app in the loader.
            loader_app_builder.Services.AddSingleton(svc => new HeartService(modded_cobalt_core_app, modded_cobalt_core_app.Services.GetRequiredService<IHostApplicationLifetime>()));
            var loader_app = loader_app_builder.Build();
            //boot modded cobalt core app to make services available. 
            _ = modded_cobalt_core_app.RunAsync();
            //boot loader app for the user to interact.
            _ = loader_app.RunAsync();
            // wait for both the loader and the modded cobalt core to shutdown
            try
            {
                modded_cobalt_core_app.WaitForShutdown();
            }
            catch
            {

            }
            return 0;
        }
    }
}
