// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Reflection;
using System.Resources;
using SingleFileExtractor.Core;

public static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        List<Assembly> cobalt_core_exe_assemblies = new List<Assembly>();
        Console.WriteLine("Booting Cobalt Core Mod Loader...");
        var exe_file = new FileInfo("CobaltCore.exe");

        if (exe_file.Exists)
        {
            try
            {
                Console.WriteLine("Extracting CobaltCore.exe...");
                //Extract everything from cobalt core application to use up
                using (ExecutableReader reader = new(exe_file.FullName))
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
                                Assembly asm = AppDomain.CurrentDomain.Load(buffer);
                                //Assembly asm = Assembly.Load(buffer);
                                cobalt_core_exe_assemblies.Add(asm);
                              //  Console.WriteLine("Loaded Assembly:" + asm.FullName);
                            }
                        }
                        catch
                        {
                        }
                    }

                }
                Console.WriteLine("Cobalt Core Executable content loaded.");
                Console.WriteLine("Booting Cobalt Core from loaded assemblies...");

                var cobalt_core_asm = cobalt_core_exe_assemblies.FirstOrDefault(e => e.FullName?.ToLower().Contains("cobaltcore") ?? false);

                if (cobalt_core_asm != null && cobalt_core_asm.EntryPoint != null)
                {
                    var entry_point = cobalt_core_asm.EntryPoint;
                    //Overwrite assembly resolve to use whatever we have grabbed from the cobalt core exe.
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, evt) => { return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.FullName == evt.Name); };
                    //run main entry point...
                    entry_point.Invoke(null, new object[] { args });
                }
                else
                {
                    Console.WriteLine("Cannot Find CobaltCore Entry Point");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex);
            }

        }
        else
        {

            Console.WriteLine("No Cobalt Core Exe found. Shutting down.");


        }
        Console.ReadLine();
        return 0;
    }

}