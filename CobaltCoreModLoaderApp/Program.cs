using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using System.IO;
using System.Windows.Forms;

namespace CobaltCoreModLoaderApp
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Directory.CreateDirectory("Outputs");
            // var path = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "LastLog.txt");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "LastLog.txt");
            var path_fallback = Path.Combine(Directory.GetCurrentDirectory(), "Outputs", "Crash.txt");
      
            try
            {
                Log.Logger = new LoggerConfiguration().WriteTo.File(path, rollOnFileSizeLimit: true, retainedFileCountLimit: 2).CreateLogger();
                //build cobalt core
                var modded_cobalt_core_builder = new HostApplicationBuilder();

                modded_cobalt_core_builder.Services.AddSerilog();

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

                _ = modded_cobalt_core_app.RunAsync();
     
                // Run Mod loader form
                var form = new MainForm(modded_cobalt_core_app);
                Application.Run(form);
                // Wait for game to close
                form.CobaltCoreGameTask?.Wait();
            }
            catch (Exception err)
            {
                File.WriteAllLines(path_fallback, new string[] { err.ToString(), err.StackTrace ?? "", err.InnerException?.Message ?? "", err.InnerException?.StackTrace ?? "" });
            }

        }
    }
}