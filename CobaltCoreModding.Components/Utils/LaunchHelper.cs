using CobaltCoreModding.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CobaltCoreModding.Components.Utils
{
    public static class LaunchHelper
    {
        public static HostApplicationBuilder CreateBuilder()
        {
            HostApplicationBuilder builder = new HostApplicationBuilder();
            builder.Services.AddLogging();
            builder.Logging.AddConsole();

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
            builder.Services.AddSingleton<PartRegistry>();
            builder.Services.AddSingleton<ShipRegistry>();
            builder.Services.AddSingleton<StarterShipRegistry>();
            builder.Services.AddSingleton<PartTypeRegistry>();
            builder.Services.AddSingleton<StoryRegistry>();
            return builder;
        }

        public static void PreLaunch(IHost host)
        {
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
            //patch parts.
            host.Services.GetRequiredService<PartTypeRegistry>().LoadManifests();
            //patch ship parts
            host.Services.GetRequiredService<PartRegistry>().LoadManifests();
            //load ship manifests.
            host.Services.GetRequiredService<ShipRegistry>().LoadManifests();
            //load starter ship manifests
            host.Services.GetRequiredService<StarterShipRegistry>().RunLogic();
            //load story manifests
            host.Services.GetRequiredService<StoryRegistry>().RunLogic();
            //patch db
            host.Services.GetRequiredService<DBExtender>().PatchDB();
            //load events
            host.Services.GetRequiredService<CustomEventHub>().LoadManifest();
            //run remaining mod logic
            host.Services.GetRequiredService<ModAssemblyHandler>().FinalizeModLoading();
        }
    }
}