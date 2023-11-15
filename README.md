# Cobalt Core Modloader
A simple mod loader for the game [Cobalt Core](https://store.steampowered.com/app/2179850/Cobalt_Core/) by [Rocket Rat Games](https://rocketrat.games/). 

It provides a pipeline to add sprites, cards, ships, characters and much more. While it is our goal to cover any and all aspects of the game, we just haven't reached that point.

## Project Structure

This repo features the following components:

* **CobaltCoreModding.Definitions** These are the interfaces/descriptions for a mod to advertise it's content to the loader. See "About Writing Mods" for more details.
* **CobaltCoreModding.Components** The original loader that used a simple command line interface to do its job. Now it mostly serves to hold the logic of the mod loader in the form of many services that are fed into a generic host.
* **CobaltCoreModLoaderApp** The UI of the mod loader written with GTK sharp of all things. *(because making an UI was that cursed)*
* **DemoMod** This project is a demonstration of the mod loaders capabilities. It requires a copy of the cobaltcore.dll extracted from CobaltCore.exe, which is why it's not part of the default solution. But along with the comments in Definitions, it should help understanding the mod process.
* **[SingleFileExtractor](https://github.com/Droppers/SingleFileExtractor)** A submodule by Joery Droppers used to extract assemblies from CobaltCore.exe during runtime.

# Using the Modloader App

![LauncherUI](https://github.com/Ewanderer/CobaltCoreModLoader/blob/master/images/mod_loader_image.png)

The loader is at its core extremely minimalistic. It only needs the Path to the Cobalt Core Executable (Directory or File are fine). And then a list of mod assembly files (*.dll). Either single files or a complete directory are permissible with the letter scanning all first level subdirectories for libraries of the same name within.

Example:

> ./ModLibrary
>
> ./ModLibrary/DemoMod
>
> ./ModLibrary/DemoMod.dll

If ModLibrary folder was scanned, the loader would add DemoMod.dll to the list. This should make loading entire mod libraries pretty fast provided they are distributed with the right folder and file name schema.

Once all mods have been selected, they can either be preloaded or cobalt core can be launched immediately. Preloading is intended for mods that touch the Launcher UI itself, to run their code while the game is still out. This hasn't been implemented fully, but for the sake of future use it will be documented here.

If you are worried about saves, don't be, as the loader will create a new set of save files within it's own directory. Should you wish to port your files over, you just have to copy them there. Otherwise try to not tinker to much with mods after getting a savefile up, as certain dynamic properties can end up in the save file and confuse the game on future launches, if mods have changed.

# Writing Mods

The process of making a mod is relatively simple:

* Create a new c# library project with your favourite tool set.
* Reference CobaltCoreModding.Definitions either with a copy of the DLL or the entire project directly.
* Create a class that implements any of the IManifest interfaces.
* Build your project and ship the dll with any required files and you are done. Just make sure to use the name schema of Modname/Modname.dll for the sake of allowing quick loading from a mod library folder.

It is recommend to check CobaltCore.dll for more references on how the game works. The loader itself uses [Harmony](https://github.com/pardeike/Harmony) to patch the game and is thus recommend for any further needs.

# Credits
* Many thanks to the wonderful people at Rocket Rat Games for creating Cobalt Core. This would not have been possible with their help and approval to expand their game this way.

* Original Code and Concept for the Loader was done by EWanderer.
* Single File Extractor by Joery Droppers
* Loader was created using [Harmony](https://github.com/pardeike/Harmony) by Andreas Pardeike.
* Sprites in the DemoMod by EWanderer
