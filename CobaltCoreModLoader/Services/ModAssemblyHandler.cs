using CobaltCoreModding.Definitions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// A singleton to help store any assembly and its manifest.
    /// Can also run their bootup according to dependency.
    /// </summary>
    public class ModAssemblyHandler
    {
        private ILogger<ModAssemblyHandler> logger { get; init; }

        public ModAssemblyHandler(ILogger<ModAssemblyHandler> logger)
        {
            this.logger = logger;
        }

        private List<Tuple<Assembly, IModManifest?>> mod_lookup_list = new List<Tuple<Assembly, IModManifest?>>();

        public IEnumerable<Tuple<Assembly, IModManifest?>> ModLookup => mod_lookup_list;

        public void LoadModAssembly(FileInfo mod_file)
        {
            try
            {
                logger.LogInformation($"Loading mod from {mod_file.FullName}...");
                var assembly = Assembly.LoadFile(mod_file.FullName);
                var manifest_types = assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces().Contains(typeof(IModManifest)));
                IModManifest? manifest = null;
                if (manifest_types.Count() == 0)
                {
                    logger.LogWarning("Mod assembly contains no manifest.");
                }
                else
                {
                    if (manifest_types.Count() > 1)
                    {
                        logger.LogWarning($"Mod assembly contains more than one manifest. Will use manifest type {manifest_types.First().Name}");
                    }
                    var manifest_instance = (manifest_types.First().GetConstructor(Type.EmptyTypes)?.Invoke(new object[0]));
                    if (manifest_instance == null)
                        throw new Exception("No empty constructor found");
                    manifest = manifest_instance as IModManifest;
                }

                mod_lookup_list.Add(new Tuple<Assembly, IModManifest?>(assembly, manifest));

            }
            catch (Exception err)
            {
                logger.LogCritical(err, $"Error while loading mod assembly from '{mod_file.FullName}':");
            }
        }

    }
}
