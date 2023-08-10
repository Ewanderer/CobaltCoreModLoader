using CobaltCoreModding.Definitions.ModContactPoints;
using Microsoft.Extensions.Logging;
using SingleFileExtractor.Core;
using System.Reflection;

namespace CobaltCoreModLoader.Services
{
    /// <summary>
    /// This class contains the logic to parse a CoboltCore executable and start it up on demand.
    /// After being loaded it will also provide the cobalt core game assembly for any shenanigans
    /// </summary>
    public class CobaltCoreHandler : ICobaltCoreContact
    {

        public static Assembly? CobaltCoreAssembly { get; private set; }
        public DirectoryInfo? CobaltCoreAppPath { get; private set; }

        Assembly ICobaltCoreContact.CobaltCoreAssembly => CobaltCoreAssembly ?? throw new Exception("Cobalt Core Assembly not loaded!");

        private List<Assembly> CobaltCoreExecutableAssemblies = new List<Assembly>();

        public CobaltCoreHandler(ILogger<CobaltCoreHandler> logger)
        {
            this.logger = logger;
        }

        private ILogger<CobaltCoreHandler> logger;

        public void LoadupCobaltCore(FileInfo cobaltCoreExecutable)
        {
            if (!cobaltCoreExecutable.Exists)
                throw new ArgumentException("Specified File Path doesn't exist");
            CobaltCoreAppPath = cobaltCoreExecutable.Directory ?? throw new Exception("Unkown directory!");
            var symbol_file = CobaltCoreAppPath.GetFiles().FirstOrDefault(e => string.Compare(e.Name, "CobaltCore.pdb", true) == 0);
            //Load Cobalt Core Exe
            try
            {
                using (ExecutableReader reader = new(cobaltCoreExecutable.FullName))
                {
                    foreach (var file in reader.Bundle.Files)
                    {
                        if (file.Type != FileType.Assembly)
                            continue;
                        try
                        {
                            using (var stream = file.AsStream())
                            {
                                var buffer = new byte[stream.Length];
                                stream.Read(buffer);
                                Assembly asm;
                                if (file.RelativePath == "CobaltCore.dll" && symbol_file != null)
                                {
                                    using (var symbol_stream = symbol_file.OpenRead())
                                    {
                                        var symbol_buffer = new byte[symbol_stream.Length];
                                        symbol_stream.Read(symbol_buffer);
                                        asm = Assembly.Load(buffer, symbol_buffer);
                                    }
                                }
                                else
                                    asm = Assembly.Load(buffer);
                                CobaltCoreExecutableAssemblies.Add(asm);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                throw new ArgumentException("Couldn't parse given executable as Signgle File. You sure you have selected cobalt core?");
            }

            //   var CobaltCoreAssembly = CobaltCoreExecutableAssemblies.FirstOrDefault(e => e.FullName?.ToLower().Contains("cobaltcore") ?? false);
            CobaltCoreAssembly = CobaltCoreExecutableAssemblies.FirstOrDefault(e => e.ManifestModule.ScopeName == "CobaltCore.dll") ?? throw new ArgumentException("Given Executable doesn't contain CobaltCore.dll.");

            //Setup assembly resolver for anything else.
            AppDomain.CurrentDomain.AssemblyResolve += (sender, evt) => { return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.FullName == evt.Name); };
        }

        /// <summary>
        /// Startsup cobalt core game.
        /// </summary>
        /// <param name="args">arguments to be passed to cobalt core game for starting.</param>
        public void RunCobaltCore(string[] args)
        {
            var entry_point = CobaltCoreAssembly?.EntryPoint ?? throw new Exception("No entry point in cobalt core dll!");
            if (CobaltCoreAppPath == null)
                throw new Exception("No cobalt core assembly name know");
            var current_dir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(CobaltCoreAppPath.FullName);
            try
            {
                entry_point.Invoke(null, new object[] { args });
            }
            finally
            {
                Directory.SetCurrentDirectory(current_dir);
            }
        }
    }
}