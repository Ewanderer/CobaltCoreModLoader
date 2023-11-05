using CobaltCoreModLoader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CobaltCoreModLoaderApp
{
    public class LauncherUI
    {
        /// <summary>
        /// The horizontal stakc in the main window. tabs are packed at bottom
        /// top
        /// </summary>
        private Gtk.Box? window_content_panel;
        /// <summary>
        /// The main panel
        /// </summary>
        private Gtk.Box? main_panel;
        /// <summary>
        /// The main window
        /// </summary>
        private Gtk.Window? main_window;
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

        private Gtk.Separator? separator;


        public LauncherUI(IHost moddedCobaltCoreApp)
        {
            ModdedCobaltCoreApp = moddedCobaltCoreApp;
        }

        public IHost ModdedCobaltCoreApp { get; init; }

        public void Launch()
        {
            if (uiTask != null)
                return;

            uiTask = new Task(() =>
            {
                Setup();
                CreateMainPanel();
                //insert modded tabs.
                FinishSetup();
                //Run
                Gtk.Application.Run();
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

        private Gtk.Entry? CobaltCorePathEntry;



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
                CobaltCorePathEntry.Expand = true;
                root_path_box.PackStart(CobaltCorePathEntry, true, true, 5);


                root_path_box.ShowAll();
            }
            {
                // display list of mods.
                var mod_list = new Gtk.ListBox();
            }

            {
                //buttons row for new mod loading
            }

            {
                //prewarm and close on launch checkmark
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

        bool mods_loaded = false;

        private bool LoadMods()
        {

            if (mods_loaded)
                return true;
            //feed all listed manifest to assembly handler
            var svc = ModdedCobaltCoreApp.Services.GetRequiredService<ModAssemblyHandler>();



            //launch preload manifests

            //launch modify loader ui manifests

            mods_loaded = true;
            return true;
        }

        bool launched_once = false;

        private void LaunchBtn_Clicked(object? sender, EventArgs evt)
        {
            if (!Warmup() || cobalt_core_launched)
                return;
            if (!LoadMods())
                return;

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
            var task = new Task(() =>
             {
                 svc.RunCobaltCore(new string[] { "--debug" }, false);
             }, TaskCreationOptions.LongRunning);
            task.Start();
            cobalt_core_launched = true;
            // Close();
        }

        bool warmup_done = false;

        public bool Warmup()
        {
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
                    dg.Show();
                    return false;
                }
                cc_exe_path = System.IO.Path.Combine(CobaltCorePathEntry.Text, System.IO.Path.GetFileName("CobaltCore.exe"));
                if (!File.Exists(cc_exe_path))
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "Game directory doesn't contain cobalt core.exe");
                    dg.Show();
                    return false;
                }
            }
            else
            {
                if (string.Compare("CobaltCore.exe", new FileInfo(CobaltCorePathEntry.Text).Name) != 0)
                {
                    var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, "File given is not CobaltCore.exe");
                    dg.Show();
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
                warmup_done = true;
            }
            catch (Exception err)
            {
                var dg = new Gtk.MessageDialog(main_window, Gtk.DialogFlags.DestroyWithParent, Gtk.MessageType.Warning, Gtk.ButtonsType.Ok, err.Message);
                dg.Show();
            }
            return warmup_done;
        }

        private void HotfixMissingDll(string cobalt_exe_path)
        {

            var directory = new FileInfo(cobalt_exe_path)?.Directory??throw new Exception("Broken get directory.");

            foreach (var lib_file in directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), lib_file.Name);
                lib_file.CopyTo(path, true);
            }

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

        bool cobalt_core_launched;

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