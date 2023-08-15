// See https://aka.ms/new-console-template for more information
using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            Stopwatch mod_boot_timer = new Stopwatch();
            mod_boot_timer.Start();
            HostApplicationBuilder builder = new HostApplicationBuilder(); ;

            builder.Services.AddLogging();
            builder.Logging.AddConsole();

            builder.Services.AddSingleton<CobaltCoreHandler>();
            builder.Services.AddSingleton<DBExtender>();
            builder.Services.AddSingleton<ModAssemblyHandler>();
            builder.Services.AddSingleton<SpriteExtender>();
            builder.Services.AddSingleton<AnimationRegistry>();

            var host = builder.Build();

            host.Start();

            //load cobalt core assembly

            var cobalt_core = host.Services.GetRequiredService<CobaltCoreHandler>();
            cobalt_core.LoadupCobaltCore(new FileInfo("O:\\SteamLibrary\\steamapps\\common\\Cobalt Core\\CobaltCore.exe"));
            //load mods and their manifests.
            var mod_loader = host.Services.GetRequiredService<ModAssemblyHandler>();
            mod_loader.LoadModAssembly(new FileInfo("X:\\PROGRAMMING\\CobaltCoreModLoader\\DemoMod\\bin\\Debug\\net6.0\\DemoMod.dll"));
            //patch cobalt core and load various mod components in order of dependency.

            //patch art.
            host.Services.GetRequiredService<SpriteExtender>().PatchSpriteSystem();
            //patch animation
            host.Services.GetRequiredService<AnimationRegistry>().LoadManifests();
            //patch db
            host.Services.GetRequiredService<DBExtender>().PatchDB();

            //run remaining mod logic
            mod_loader.RunModLogics();
            mod_boot_timer.Stop();
            Console.WriteLine(mod_boot_timer.Elapsed.TotalSeconds.ToString());
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