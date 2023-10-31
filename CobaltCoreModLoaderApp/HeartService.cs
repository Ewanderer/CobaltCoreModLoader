using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoaderApp
{
    public class HeartService
    {
        public IHost ModdedCobaltCoreApp { get; init; }

        public IHostApplicationLifetime ModdedCobaltCoreAppLifeTime { get; init; }

        public HeartService(IHost moddedCobaltCoreApp, IHostApplicationLifetime moddedCobaltCoreAppLifeTime)
        {
            ModdedCobaltCoreApp = moddedCobaltCoreApp;
            ModdedCobaltCoreAppLifeTime = moddedCobaltCoreAppLifeTime;
        }
    }
}
