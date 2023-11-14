using CobaltCoreModding.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using System.IO;
using System.Windows.Forms;
using CobaltCoreModdding.Components.Utils;

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
                var modded_cobalt_core_builder = LaunchHelper.CreateBuilder();
                //add custom services
                modded_cobalt_core_builder.Services.AddSerilog();


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