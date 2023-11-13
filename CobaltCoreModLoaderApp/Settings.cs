using System.Collections.Generic;

namespace CobaltCoreModLoaderApp
{
    public class Settings
    {
        public bool CloseLauncherAfterLaunch { get; set; }
        public bool LaunchInDeveloperMode { get; set; }
        public string? CobaltCorePath { get; set; }

        public List<ModEntry> ModEntries { get; set; } = new();

        public class ModEntry {
            public string AssemblyPath { get; set; } = "";

            public bool Active { get; set; }
        }
    }
}