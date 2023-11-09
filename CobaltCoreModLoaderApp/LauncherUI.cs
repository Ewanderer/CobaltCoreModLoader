using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace CobaltCoreModLoaderApp
{
    public class LauncherUI
    {
        public string launch_path = "";

        public Settings settings = new Settings();

        private Gtk.CheckButton? close_launcher_on_start_checkbox;
        private bool cobalt_core_launched;

        private Gtk.Entry? CobaltCorePathEntry;

        private bool launched_once = false;

        /// <summary>
        /// The main panel
        /// </summary>
        private Gtk.Box? main_panel;

        /// <summary>
        /// The main window
        /// </summary>
        private Gtk.Window? main_window;

        private Gtk.ListBox? mod_list_box;

        private Gtk.Box? ModifyModListButtonsBox;

        private bool mods_loaded = false;

        private Gtk.Separator? separator;

        /// <summary>
        /// the lookup of tab buttons and tab containers.
        /// </summary>
        private Dictionary<Gtk.Button, Gtk.Box> Tabs = new();

        /// <summary>
        /// The container for buttons to store
        /// </summary>
        private Gtk.Box? TabSelection;

        /// <summary>
        /// The task which holds the app.
        /// </summary>
        private Task? uiTask;

        private bool warmup_done = false;

        /// <summary>
        /// The horizontal stakc in the main window. tabs are packed at bottom
        /// top
        /// </summary>
        private Gtk.Box? window_content_panel;
        private Gtk.Button? load_mod_button;

        public LauncherUI(IHost moddedCobaltCoreApp)
        {
            ModdedCobaltCoreApp = moddedCobaltCoreApp;
        }

        public IHost ModdedCobaltCoreApp { get; init; }

        public void Launch()
        {
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<LauncherUI>>();
            if (uiTask != null)
                return;
            logger?.LogInformation("booting ui...");

            launch_path = Directory.GetCurrentDirectory();

            //load settings
            try
            {
                var settings_path = Path.Combine(launch_path, "LauncherSettings.json");
                if (File.Exists(settings_path))
                {
                    using (var file = File.OpenRead(settings_path))
                    {
                        settings = JsonSerializer.Deserialize<Settings>(file) ?? new Settings();
                        //remove all mod files no longer existing
                        settings.ModAssemblyPaths.RemoveAll(modpath => !File.Exists(modpath));
                    }
                }
            }
            catch
            {
                //Ignore we already have setting.
            }

            uiTask = new Task(() =>
            {
                try
                {
                    logger?.LogInformation("Running launcher ui task...");
                    Setup();
                    CreateMainPanel();
                    //insert modded tabs.
                    FinishSetup();
                    //Run
                    Gtk.Application.Run();
                }
                catch (Exception err)
                {
                    logger?.LogError(err, "launcher had error");
                }
            }, TaskCreationOptions.LongRunning);
            uiTask.Start();
        }

        public async Task WaitTillClosed()
        {
            if (uiTask == null)
            {
                return;
            }
            await uiTask;
        }

        public bool Warmup()
        {
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<LauncherUI>>();
            if (CobaltCorePathEntry == null)
                return false;
            if (warmup_done)
                return true;
            string cc_exe_path = "";
            if (!File.Exists(CobaltCorePathEntry.Text))
            {
                if (!Directory.Exists(CobaltCorePathEntry.Text))
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "No Game path give!");
                    dg.Run();
                    dg.Destroy();

                    return false;
                }
                cc_exe_path = System.IO.Path.Combine(CobaltCorePathEntry.Text, System.IO.Path.GetFileName("CobaltCore.exe"));
                if (!File.Exists(cc_exe_path))
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "Game directory doesn't contain cobalt core.exe");
                    dg.Run();
                    dg.Destroy();
                    return false;
                }
            }
            else
            {
                if (string.Compare("CobaltCore.exe", new FileInfo(CobaltCorePathEntry.Text).Name) != 0)
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "File given is not CobaltCore.exe");
                    dg.Run();
                    dg.Destroy();
                    return false;
                }
                cc_exe_path = CobaltCorePathEntry.Text;
            }
            
            //load cobalt core exe, which is required.
            try
            {
                var svc = ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreModLoader.Services.CobaltCoreHandler>();

                svc.LoadupCobaltCore(new System.IO.FileInfo(cc_exe_path));

                //copy over other dlls, because gtk messed
                HotfixMissingDll(cc_exe_path);
                settings.CobaltCorePath = cc_exe_path;
                warmup_done = true;
                logger?.LogInformation("Warmup of cobalt core complete");
            }
            catch (Exception err)
            {
                logger?.LogError(err,"error during loading cobalt core executable");
                var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, err.Message);
                dg.Run();
                dg.Destroy();
            }
            return warmup_done;
        }

        private void Add_Assembly_btn_Pressed(object? sender, EventArgs e)
        {
            if (main_window == null || mod_list_box == null)
                return;
            var fcd = new Gtk.FileChooserDialog("Pick Mod Assembly File", main_window, Gtk.FileChooserAction.Open, Gtk.FileChooserAction.Open);
            fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
            fcd.AddButton(Gtk.Stock.Open, Gtk.ResponseType.Ok);
            fcd.SetCurrentFolder(launch_path);
            var response = (Gtk.ResponseType)fcd.Run();

            if (response == Gtk.ResponseType.Ok)
            {
                try
                {
                    var assembly_name = AssemblyName.GetAssemblyName(fcd.Filename);
                    if (!settings.ModAssemblyPaths.Contains(fcd.Filename))
                    {
                        settings.ModAssemblyPaths.Add(fcd.Filename);
                        MakeModRow(fcd.Filename);
                    }
                }
                catch (Exception err)
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, err.Message);
                    dg.Run();
                    dg.Destroy();
                }
            }
            fcd.Destroy();
        }

        private void AddPanel(string tab_name, Gtk.Box box)
        {
            if (window_content_panel == null || TabSelection == null)
                throw new Exception();
            window_content_panel.PackEnd(box, true, true, 5);
            var btn = new Gtk.Button();
            btn.Label = tab_name;
            btn.Pressed += TabBtn_Pressed;

            TabSelection.PackEnd(btn, true, true, 0);
            Tabs.Add(btn, box);
            btn.Show();
            box.Show();
        }

        private void CreateMainPanel()
        {
            main_panel = new Gtk.Box(Gtk.Orientation.Vertical, 5);

            //fill stuff
            {
                //step one the cobalt core game path.
                var root_path_box = new Gtk.Box(Gtk.Orientation.Horizontal, 0);
                main_panel.PackStart(root_path_box, false, false, 5);

                var label = new Gtk.Label();
                label.Text = "CC Game Path:";
                root_path_box.PackStart(label, false, false, 5);
                label.Expand = false;

                CobaltCorePathEntry = new Gtk.Entry();

                root_path_box.PackStart(CobaltCorePathEntry, true, true, 5);
                CobaltCorePathEntry.Text = settings.CobaltCorePath ?? "";

                root_path_box.ShowAll();
            }
            {
                // display list of mods.
                var scroll_container = new Gtk.ScrolledWindow();
                scroll_container.SetPolicy(Gtk.PolicyType.Automatic, Gtk.PolicyType.Always);
                mod_list_box = new Gtk.ListBox();
                scroll_container.HScrollbar.WidthRequest = 25;
                scroll_container.VScrollbar.WidthRequest = 25;

                mod_list_box.SelectionMode = Gtk.SelectionMode.Single;
                foreach (var p in settings.ModAssemblyPaths)
                {
                    MakeModRow(p);
                }

                scroll_container.Add(mod_list_box);

                main_panel.PackStart(scroll_container, true, true, 5);


                scroll_container.ShowAll();
            }

            {
                //buttons row for new mod loading
                ModifyModListButtonsBox = new Gtk.Box(Gtk.Orientation.Horizontal, 5);
                main_panel.PackStart(ModifyModListButtonsBox, false, true, 5);

                //pack buttons into the box

                var Add_assembly_btn = new Gtk.Button();
                Add_assembly_btn.Label = "Add Assembly";
                Add_assembly_btn.Pressed += Add_Assembly_btn_Pressed;
                ModifyModListButtonsBox.PackStart(Add_assembly_btn, false, false, 5);

                var scan_folder_for_mod_btn = new Gtk.Button();
                scan_folder_for_mod_btn.Label = "Scan Folder for Mods";
                scan_folder_for_mod_btn.Pressed += Scan_folder_for_mod_btn_Pressed;
                ModifyModListButtonsBox.PackStart(scan_folder_for_mod_btn, true, true, 5);

                var remove_assembly_btn = new Gtk.Button();
                remove_assembly_btn.Label = "Remove Assembly";
                remove_assembly_btn.Pressed += Remove_assembly_btn_Pressed;

                ModifyModListButtonsBox.PackStart(remove_assembly_btn, false, false, 5);
                ModifyModListButtonsBox.ShowAll();
            }

            {
                //prewarm and close on launch checkmark

                var LoadModBox = new Gtk.Box(Gtk.Orientation.Horizontal, 5);
                main_panel.PackStart(LoadModBox, false, true, 5);

                //pack buttons into the box

                load_mod_button = new Gtk.Button();
                load_mod_button.Label = "Preload Mods";
                load_mod_button.Pressed += (sender, evt) => { LoadMods(); load_mod_button.Sensitive = false; };
                LoadModBox.PackStart(load_mod_button, true, true, 5);

                close_launcher_on_start_checkbox = new Gtk.CheckButton();
                close_launcher_on_start_checkbox.Label = "Close Launcher after Start";
                close_launcher_on_start_checkbox.Active = settings.CloseLauncherAfterLaunch;

                LoadModBox.PackStart(close_launcher_on_start_checkbox, true, true, 5);

                LoadModBox.ShowAll();
            }

            {
                //launch button
                var launch_btn = new Gtk.Button();
                launch_btn.Label = "Launch";
                launch_btn.Clicked += LaunchBtn_Clicked;

                main_panel.PackEnd(launch_btn, false, false, 5);
                launch_btn.Show();
            }

            AddPanel("Main", main_panel);
        }

        private void FinishSetup()
        {
            if (main_panel == null || TabSelection == null)
                throw new Exception();

            main_window?.ShowAll();

            //hide tabs if only one tab there.

            if (Tabs.Count == 1)
            {
                TabSelection.Hide();
                separator?.Hide();
            }

            foreach (var tab in Tabs.Values)
            {
                if (tab != main_panel)
                    tab.Hide();
            }
        }

        /// <summary>
        /// Until someone figures out a better solution, this is a hotpatch to avoid dll missing despite working directory being adjusted to cc path.
        /// </summary>
        /// <param name="cobalt_exe_path"></param>
        /// <exception cref="Exception"></exception>
        private void HotfixMissingDll(string cobalt_exe_path)
        {
            var directory = new FileInfo(cobalt_exe_path)?.Directory ?? throw new Exception("Broken get directory.");

            foreach (var lib_file in directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), lib_file.Name);
                lib_file.CopyTo(path, true);
            }
        }

        public Task? CobaltCoreGameTask { get; private set; }

        private void LaunchBtn_Clicked(object? sender, EventArgs evt)
        {
            if (!Warmup() || cobalt_core_launched)
                return;
            if (!LoadMods())
                return;
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<LauncherUI>>();

            //run all remaining services.

            //patch art.
            ModdedCobaltCoreApp.Services.GetRequiredService<SpriteExtender>().PatchSpriteSystem();
            //patch glossary
            ModdedCobaltCoreApp.Services.GetRequiredService<GlossaryRegistry>().LoadManifests();
            //patch deck
            ModdedCobaltCoreApp.Services.GetRequiredService<DeckRegistry>().LoadManifests();
            //patch status
            ModdedCobaltCoreApp.Services.GetRequiredService<StatusRegistry>().LoadManifests();
            //patch cards
            ModdedCobaltCoreApp.Services.GetRequiredService<CardRegistry>().LoadManifests();
            //card overwrites
            ModdedCobaltCoreApp.Services.GetRequiredService<CardOverwriteRegistry>().LoadManifests();
            //patch artifacts
            ModdedCobaltCoreApp.Services.GetRequiredService<ArtifactRegistry>().LoadManifests();
            //patch animation
            ModdedCobaltCoreApp.Services.GetRequiredService<AnimationRegistry>().LoadManifests();
            //patch characters
            ModdedCobaltCoreApp.Services.GetRequiredService<CharacterRegistry>().LoadManifests();
            //patch ship parts
            ModdedCobaltCoreApp.Services.GetRequiredService<PartRegistry>().LoadManifests();
            //load ship manifests.
            ModdedCobaltCoreApp.Services.GetRequiredService<ShipRegistry>().LoadManifests();
            //load starter ship manifests
            ModdedCobaltCoreApp.Services.GetRequiredService<StarterShipRegistry>().RunLogic();
            //patch db
            ModdedCobaltCoreApp.Services.GetRequiredService<DBExtender>().PatchDB();
            //load events
            ModdedCobaltCoreApp.Services.GetRequiredService<CustomEventHub>().LoadManifest();
            //run remaining mod logic
            ModdedCobaltCoreApp.Services.GetRequiredService<ModAssemblyHandler>().RunModLogics();

            var svc = ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreHandler>();

            if (close_launcher_on_start_checkbox != null)
            {
                settings.CloseLauncherAfterLaunch = close_launcher_on_start_checkbox.Active;
            }

            //write all settings
            try
            {
                var settings_path = Path.Combine(launch_path, "LauncherSettings.json");
                using (var file = File.Create(settings_path))
                {
                    JsonSerializer.Serialize<Settings>(file, settings);
                }
            }
            catch
            {
            }
            //launch game
            CobaltCoreGameTask = new Task(() =>
             {
                 logger?.LogInformation("launching cobalt core");
                 svc.RunCobaltCore(new string[] { "--debug" });
                 Gtk.Application.Quit();
             }, TaskCreationOptions.LongRunning);
            CobaltCoreGameTask.Start();
            cobalt_core_launched = true;
            if (settings.CloseLauncherAfterLaunch)
                main_window?.Close();
        }

        private bool LoadMods()
        {
            if (mods_loaded)
                return true;
            //feed all listed manifest to assembly handler
            var svc = ModdedCobaltCoreApp.Services.GetRequiredService<ModAssemblyHandler>();
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<LauncherUI>>();
            logger?.LogInformation("loading mods...");
            foreach (var assembly_file in settings.ModAssemblyPaths)
            {
                svc.LoadModAssembly(new FileInfo(assembly_file));
            }

            if (ModifyModListButtonsBox != null)
                ModifyModListButtonsBox.Sensitive = false;

            //launch preload manifests
#warning todo

            //launch modify loader ui manifests
#warning todo

            if (load_mod_button != null)
                load_mod_button.Sensitive = false;
            mods_loaded = true;

            return true;
        }

        private void MakeModRow(string file_name)
        {
            if (mod_list_box == null)
                return;
            var row = new Gtk.ListBoxRow();
            var label = new Gtk.Label();
            label.Text = file_name;
            row.Add(label);
            row.ShowAll();
            mod_list_box.Add(row);
        }

        private void Remove_assembly_btn_Pressed(object? sender, EventArgs e)
        {
            if (mod_list_box == null)
                return;
            var row = mod_list_box.SelectedRow;
            if (row == null)
                return;
            if (row.Child is not Gtk.Label lbl)
                return;
            var text = lbl.Text;
            settings.ModAssemblyPaths.Remove(text);
            mod_list_box.Remove(row);
        }

        private void Scan_folder_for_mod_btn_Pressed(object? sender, EventArgs e)
        {
            if (main_window == null || mod_list_box == null)
                return;
            var fcd = new Gtk.FileChooserDialog("Pick Mod Library Folder", main_window, Gtk.FileChooserAction.SelectFolder, Gtk.FileChooserAction.SelectFolder);
            fcd.AddButton(Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
            fcd.AddButton(Gtk.Stock.Open, Gtk.ResponseType.Ok);
            fcd.SetCurrentFolder(launch_path);
            var response = (Gtk.ResponseType)fcd.Run();

            if (response == Gtk.ResponseType.Ok)
            {
                //check all directories in directoy.

                if (!Directory.Exists(fcd.Filename))
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "Directory not existing!");
                    dg.Run();
                    dg.Destroy();
                }
                else
                {
                    foreach (var dir in Directory.GetDirectories(fcd.Filename))
                    {
                        var directory = new DirectoryInfo(dir);
                        var manifest_file = directory.GetFiles().FirstOrDefault(f => string.Compare(f.Name, directory.Name + ".dll", true) == 0);
                        if (manifest_file != null)
                        {
                            try
                            {
                                var assembly_name = AssemblyName.GetAssemblyName(manifest_file.FullName);
                                if (!settings.ModAssemblyPaths.Contains(manifest_file.FullName))
                                {
                                    settings.ModAssemblyPaths.Add(manifest_file.FullName);
                                    MakeModRow(manifest_file.FullName);
                                }
                            }
                            catch (Exception err)
                            {
                                var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, err.Message);
                                dg.Run();
                                dg.Destroy();
                            }
                        }
                    }
                }
            }
            fcd.Destroy();
        }

        private void Setup()
        {
            Gtk.Application.Init();

            main_window = new Gtk.Window("Cobalt Core Mod Loader");
            main_window.DeleteEvent += (s, e) =>
            {
                Gtk.Application.Quit();
                if (!cobalt_core_launched)
                    ModdedCobaltCoreApp.Services.GetRequiredService<IHostApplicationLifetime>().StopApplication();
            };
            window_content_panel = new Gtk.Box(Gtk.Orientation.Vertical, 5);
            main_window.Add(window_content_panel);

            TabSelection = new Gtk.Box(Gtk.Orientation.Horizontal, 10);

            window_content_panel.PackStart(TabSelection, true, true, 0);
            TabSelection.Show();
            separator = new Gtk.Separator(Gtk.Orientation.Horizontal);
            separator.Expand = false;
            window_content_panel.PackStart(separator, true, true, 0);
            separator.Show();
            window_content_panel.ShowAll();
            TabSelection.Show();
        }

        private void TabBtn_Pressed(object? sender, EventArgs e)
        {
            if (sender is not Gtk.Button button)
            {
                return;
            }
            if (Tabs.TryGetValue(button, out var target_box))
            {
                foreach (var box in Tabs.Values)
                {
                    if (target_box == box)
                        box.Show();
                    else
                        box.Hide();
                }
            }
        }
    }
}