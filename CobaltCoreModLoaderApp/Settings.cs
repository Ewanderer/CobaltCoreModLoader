using System.Collections.Generic;

namespace CobaltCoreModLoaderApp
{
    public class Settings
    {
        public bool CloseLauncherAfterLaunch { get; set; }
        public string? CobaltCorePath { get; set; }

        public List<string> ModAssemblyPaths { get; set; } = new List<string>();
    }
}