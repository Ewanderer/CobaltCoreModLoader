// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Reflection;
using System.Resources;
using CobaltCoreModLoader;
using SingleFileExtractor.Core;

public static class Program
{
    enum BlubberEnum
    {
        a = 1
            , b = 2, c = 3, d = 4
    }

    [STAThread]
    private static int Main(string[] args)
    {



        //  var loader = new CoreLoader("O:\\SteamLibrary\\steamapps\\common\\Cobalt Core\\CobaltCore.exe");

        //   CobaltCorePatching.Patch(loader.CobaltCoreAssembly);
        //   loader.RunCobaltCore(args);
        return 0;
    }

}