// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Reflection.PortableExecutable;

public static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {

        Console.WriteLine("Booting Cobalt Core Mod Loader...");
        var exe_file = new FileInfo("CobaltCore.exe");
      
        if (exe_file.Exists)
        {

         

            try
            {
                //open cobal core exe.
                using(var exe_stream=exe_file.OpenRead())
                {
                    //get data from exe.
                    var exe_headers = new PEHeaders(exe_stream);


                    //find all assemblies and load them.
                    var header = exe_headers.PEHeader;
                    if (header != null)
                    {
                        //start cobalt core assembly.

                        var stuff = header.ResourceTableDirectory;
                        Console.WriteLine("Stuffing?");
                    }

                   

                    foreach(var section in exe_headers.SectionHeaders){
                        Console.WriteLine(section.Name);
                    }
                }
                
                var assembly = Assembly.Load("CobaltCore.dll");
                
                // Run Program startup.
                var entry_point = assembly.EntryPoint;
                if (entry_point != null)
                {
                    var result = entry_point.Invoke(null, args);
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Assembly doesn't contain entry point. you sure you are running cobalt core?");
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