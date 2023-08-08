// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Reflection;
using System.Resources;
using CobaltCoreModLoader;
using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SingleFileExtractor.Core;

public static class Program
{
    enum BlubberEnum
    {
        a = 1
            , b = 2, c = 3, d = 4
    }

    [STAThread]
    private static int Main(string[] args)
   {
        try
        {
            HostApplicationBuilder builder = new HostApplicationBuilder(); ;


            builder.Services.AddLogging();
            builder.Logging.AddConsole();

            builder.Services.AddSingleton<CobaltCoreHandler>();
            builder.Services.AddSingleton<DBPatcher>();
            builder.Services.AddSingleton<ModAssemblyHandler>();

            var host = builder.Build();

            host.Start();
            //load cobalt core
            var cobalt_core = host.Services.GetRequiredService<CobaltCoreHandler>();
            cobalt_core.LoadupCobaltCore(new FileInfo("O:\\SteamLibrary\\steamapps\\common\\Cobalt Core\\CobaltCore.exe"));
            //patch cobalt core
            host.Services.GetRequiredService<DBPatcher>().PatchDB();

            //load mods
            {
                var mod_loader = host.Services.GetRequiredService<ModAssemblyHandler>();
                mod_loader.LoadModAssembly(new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\bin\\Debug\\net6.0\\DemoMod.dll"));
            }
            //run cobalt core.
            cobalt_core.RunCobaltCore(new string[] { "--debug" });

            host.StopAsync().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        return 0;
    }

}