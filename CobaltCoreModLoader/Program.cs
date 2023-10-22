// See https://aka.ms/new-console-template for more information
using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public static class Program
{
    private static IHost? host;
    private static Stopwatch mod_boot_timer = new Stopwatch();

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            mod_boot_timer.Start();
            HostApplicationBuilder builder = new HostApplicationBuilder();
            builder.Services.AddLogging();
            builder.Logging.AddConsole();
            builder.Services.AddSingleton<SettingService>();
            builder.Services.AddSingleton<CobaltCoreHandler>();
            builder.Services.AddSingleton<DBExtender>();
            builder.Services.AddSingleton<ModAssemblyHandler>();
            builder.Services.AddSingleton<SpriteExtender>();
            builder.Services.AddSingleton<AnimationRegistry>();
            builder.Services.AddSingleton<DeckRegistry>();
            builder.Services.AddSingleton<CardRegistry>();
            builder.Services.AddSingleton<CardOverwriteRegistry>();
            builder.Services.AddSingleton<CharacterRegistry>();
            builder.Services.AddSingleton<GlossaryRegistry>();
            builder.Services.AddSingleton<ArtifactRegistry>();
            builder.Services.AddSingleton<StatusRegistry>();
            builder.Services.AddSingleton<CustomEventHub>();

            host = builder.Build();
            host.Start();
            //load /setup settings
            SetupPaths();
            var setting_service = host.Services.GetRequiredService<SettingService>();
            //load cobalt core assembly
            var cobalt_core = host.Services.GetRequiredService<CobaltCoreHandler>();
            cobalt_core.LoadupCobaltCore(new FileInfo(Path.Combine(setting_service.CobaltCoreGamePath?.FullName ?? throw new Exception("Missing path"), Path.GetFileName("CobaltCore.exe"))));
            //load mods and their manifests.
            PickupModsFromLib();
            //patch cobalt core and load various mod components in order of dependency.

            //patch art.
            host.Services.GetRequiredService<SpriteExtender>().PatchSpriteSystem();
            //patch glossary
            host.Services.GetRequiredService<GlossaryRegistry>().LoadManifests();
            //patch deck
            host.Services.GetRequiredService<DeckRegistry>().LoadManifests();
            //patch status
            host.Services.GetRequiredService<StatusRegistry>().LoadManifests();
            //patch cards
            host.Services.GetRequiredService<CardRegistry>().LoadManifests();
            //card overwrites
            host.Services.GetRequiredService<CardOverwriteRegistry>().LoadManifests();
            //patch artifacts
            host.Services.GetRequiredService<ArtifactRegistry>().LoadManifests();
            //patch animation
            host.Services.GetRequiredService<AnimationRegistry>().LoadManifests();
            //patch characters
            host.Services.GetRequiredService<CharacterRegistry>().LoadManifests();
            //patch db
            host.Services.GetRequiredService<DBExtender>().PatchDB();
            //load events
            host.Services.GetRequiredService<CustomEventHub>().LoadManifest();
            //run remaining mod logic
            host.Services.GetRequiredService<ModAssemblyHandler>().RunModLogics();
            mod_boot_timer.Stop();
            host.Services.GetService<ILogger<Stopwatch>>()?.LogInformation("Mod loader booted in:" + mod_boot_timer.Elapsed.TotalSeconds.ToString());
            //run cobalt core.
            cobalt_core.RunCobaltCore(new string[] { "--debug" });
            host.StopAsync().Wait();
        }
        catch (Exception ex)
        {
            if (host != null)
            {
                host.Services.GetService<ILogger<CobaltCoreHandler>>()?.LogError(ex, null);
                HostingAbstractionsHostExtensions.WaitForShutdown(host);
            }
            else
            {
                Console.WriteLine("Exception during host creation:" + ex);
            }
        }
        return 0;
    }

    private static void PickupModsFromLib()
    {
        if (host == null)
            throw new Exception("No host.");
        var mod_loader = host.Services.GetRequiredService<ModAssemblyHandler>();
        var setting_service = host.Services.GetRequiredService<SettingService>();
        var logger = host.Services.GetRequiredService<ILogger<SettingService>>();

        var directory = setting_service.CobaltCoreModLibPath ?? throw new Exception("missing path");
        if (!directory.Exists)
            return;
        foreach (var folder in directory.EnumerateDirectories())
        {
            //check for a dll with the same name as the folder and load it.

            var mod_lib_file = folder.EnumerateFiles().FirstOrDefault(e => string.Compare(e.Extension, ".dll", true) == 0 && string.Compare(Path.GetFileNameWithoutExtension(e.Name), folder.Name, true) == 0);
            if (mod_lib_file != null)
            {
                mod_loader.LoadModAssembly(mod_lib_file);
            }
            else
            {
                logger.LogWarning($"Folder {folder.Name} in Mod Library doesn't contain a dll with matching name. Skipping...");
            }
        }
    }

    private static void SetupCobaltCorePath(SettingService setting_service, ILogger logger)
    {
        if (setting_service.CobaltCoreGamePath == null || !setting_service.CobaltCoreGamePath.Exists || !File.Exists(Path.Combine(setting_service.CobaltCoreGamePath.FullName, Path.GetFileName("CobaltCore.exe"))))
        {
            mod_boot_timer.Stop();
            logger.LogInformation("Please enter CobaltCore game path:");
            //loop until setting is nailed down...
            while (true)
            {
                //ask user for cobalt core exe path.
                var path = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(path))
                {
                    logger.LogWarning("Empty input. Try again:");
                    continue;
                }

                var executable = new FileInfo(path);
                var directory = new DirectoryInfo(path);
                if (!executable.Exists && !directory.Exists)
                {
                    logger.LogWarning("Input not a valid path or doesn't exist. Try again:");
                    continue;
                }
                if (executable.Exists)
                {
                    if (string.Compare(executable.Name, "CobaltCore.exe", true) == 0)
                    {
                        setting_service.CobaltCoreGamePath = executable.Directory ?? throw new Exception("Executable has no parent directory");
                        break;
                    }
                    else
                    {
                        logger.LogWarning("Executable is not CobaltCore.exe. Try again:");
                    }
                }
                else if (directory.Exists)
                {
                    //check if contains cobalt core executable
                    if (File.Exists(Path.Combine(directory.FullName, Path.GetFileName("CobaltCore.exe"))))
                    {
                        setting_service.CobaltCoreGamePath = directory;
                        break;
                    }
                    else
                    {
                        logger.LogWarning("Directory doesn't contain \"CobaltCore.exe\". Try again:");
                    }
                }
            }
            mod_boot_timer.Start();
            logger.LogInformation("Cobalt Core Game path successfully set and saved to settings.");
        }
        else
        {
            logger.LogInformation("Using Cobalt Core in: " + setting_service.CobaltCoreGamePath.FullName);
            logger.LogInformation("If you wish to change this, edit or delete setting file.");
        }
    }

    private static void SetupModLibPath(SettingService setting_service, ILogger logger, IHostEnvironment host_env)
    {
        if (setting_service.CobaltCoreModLibPath != null && setting_service.CobaltCoreModLibPath.Exists)
        {
            logger.LogInformation("Loading mods from: " + setting_service.CobaltCoreModLibPath.FullName);
            logger.LogInformation("If you wish to change path, edit or delete settings file!");
            return;
        }
        mod_boot_timer.Stop();
        if (setting_service.CobaltCoreModLibPath != null)
        {
            logger.LogWarning("Configured Mod Lib folder doesn't exist! New Configuration required.");
        }

        while (true)
        {
            logger.LogInformation("Do you want to use default directory \"ModLibrary\" nested in the mod loader folder? yes/no");
            var answer = Console.ReadLine()?.Trim() ?? "";
            if (string.Compare(answer, "yes", true) == 0)
            {
                //Create default folder
                var default_location = new DirectoryInfo(host_env.ContentRootPath).CreateSubdirectory("ModLibrary");
                setting_service.CobaltCoreModLibPath = default_location;
                return;
            }
            else if (string.Compare(answer, "no", true) == 0)
                break;
        }
        //ask for directory to use instead.
        while (true)
        {
            logger.LogInformation("Please enter mod library folder path:");
            var path = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(path))
            {
                logger.LogWarning("Empty input.");

                continue;
            }
            if (Directory.Exists(path))
            {
                setting_service.CobaltCoreModLibPath = new DirectoryInfo(path);
                logger.LogInformation("Path accepted.");
                mod_boot_timer.Start();
                return;
            }
            else
            {
                logger.LogWarning("Directory does not exist.");
            }
        }
    }

    private static void SetupPaths()
    {
        if (host == null)
            throw new Exception("No host.");
        var setting_service = host.Services.GetRequiredService<SettingService>();
        var logger = host.Services.GetRequiredService<ILogger<SettingService>>();
        var host_env = host.Services.GetRequiredService<IHostEnvironment>();
        SetupCobaltCorePath(setting_service, logger);
        SetupModLibPath(setting_service, logger, host_env);
    }
}