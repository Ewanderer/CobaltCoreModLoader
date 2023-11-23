using CobaltCoreModding.Components.Utils;
using CobaltCoreModding.Components.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CobaltCoreModLoaderApp
{
    public partial class MainForm : Form
    {
        public Settings settings = new Settings();
        private bool cobalt_core_launched;
        private bool mods_loaded;
        private bool warmup_done = false;

        public MainForm(IHost moddedCobaltCoreApp)
        {
            InitializeComponent();
            ModdedCobaltCoreApp = moddedCobaltCoreApp;

            //load settings
            try
            {
                var settings_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? throw new Exception(), "LauncherSettings.json");
                if (File.Exists(settings_path))
                {
                    using (var file = File.OpenRead(settings_path))
                    {
                        settings = JsonSerializer.Deserialize<Settings>(file) ?? new Settings();
                        //remove all mod files no longer existing
                        settings.ModEntries.RemoveAll(entry => !File.Exists(entry.AssemblyPath));

                        tbPath.Text = settings.CobaltCorePath;
                        cbCloseOnLaunch.Checked = settings.CloseLauncherAfterLaunch;
                        cbStartDevMode.Checked = settings.LaunchInDeveloperMode;
                        foreach (var entry in settings.ModEntries)
                        {
                            var idx = clbModLibrary.Items.Add(entry.AssemblyPath);
                            clbModLibrary.SetItemChecked(idx, entry.Active);
                        }

                    }
                }
            }
            catch
            {
                //Ignore we already have setting.
            }

        }

        public Task? CobaltCoreGameTask { get; private set; }
        public IHost ModdedCobaltCoreApp { get; init; }

        public bool Warmup()
        {
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<MainForm>>();
            if (warmup_done)
                return true;
            string cc_exe_path = "";
            if (!File.Exists(tbPath.Text))
            {
                if (!Directory.Exists(tbPath.Text))
                {
                    MessageBox.Show("No Game path give!");
                    return false;
                }
                cc_exe_path = System.IO.Path.Combine(tbPath.Text, System.IO.Path.GetFileName("CobaltCore.exe"));
                if (!File.Exists(cc_exe_path))
                {
                    MessageBox.Show("Game directory doesn't contain cobalt core.exe");
                    return false;
                }
            }
            else
            {
                if (string.Compare("CobaltCore.exe", new FileInfo(tbPath.Text).Name) != 0)
                {
                    MessageBox.Show("File given is not CobaltCore.exe");
                    return false;
                }
                cc_exe_path = tbPath.Text;
            }

            //load cobalt core exe, which is required.
            try
            {
                var svc = ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreModding.Components.Services.CobaltCoreHandler>();

                svc.LoadupCobaltCore(new System.IO.FileInfo(cc_exe_path));

                //copy over other dlls, because gtk messed
                //  HotfixMissingDll(cc_exe_path);
                settings.CobaltCorePath = cc_exe_path;
                warmup_done = true;
                logger?.LogInformation("Warmup of cobalt core complete");
            }
            catch (Exception err)
            {
                logger?.LogError(err, "error during loading cobalt core executable");
                MessageBox.Show(err.Message);
            }
            return warmup_done;
        }

        private void btnAddAssembly_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Mod Assembly";
                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? "";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.CheckFileExists)
                    {
                        var path = dialog.FileName;
                        if (settings.ModEntries.Any(e => string.Compare(e.AssemblyPath, path, true) == 0))
                        {
                            MessageBox.Show("Mod already loaded!");
                            return;
                        }
                        settings.ModEntries.Add(new Settings.ModEntry() { AssemblyPath = path, Active = true });
                        var idx = clbModLibrary.Items.Add(path);
                        clbModLibrary.SetItemChecked(idx, true);
                    }
                }
            }
        }

        private void btnLaunchCobaltCore_Click(object sender, EventArgs e)
        {
            if (!Warmup() || cobalt_core_launched)
                return;
            if (!LoadMods())
                return;
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<MainForm>>();

            //run all remaining services.

            LaunchHelper.PreLaunch(ModdedCobaltCoreApp);

            var svc = ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreHandler>();

            settings.CloseLauncherAfterLaunch = cbCloseOnLaunch.Checked;

            settings.LaunchInDeveloperMode = cbStartDevMode.Checked;

            //write all settings
            try
            {
                var settings_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Environment.ProcessPath) ?? throw new Exception(), "LauncherSettings.json");
                using (var file = File.Create(settings_path))
                {
                    JsonSerializer.Serialize<Settings>(file, settings);
                }
            }
            catch
            {
                MessageBox.Show("Failed to save settings!");
            }
            //launch game
            CobaltCoreGameTask = new Task(() =>
            {
                logger?.LogInformation("launching cobalt core");
                if (settings.LaunchInDeveloperMode)
                {
                    svc.RunCobaltCore(new string[] { "--debug" });
                }
                else
                {
                    svc.RunCobaltCore(new string[0]);
                }
            }, TaskCreationOptions.LongRunning);
            CobaltCoreGameTask.Start();
            cobalt_core_launched = true;
            if (settings.CloseLauncherAfterLaunch)
                Close();
        }

        private void btnLoadFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var dir = new DirectoryInfo(dialog.SelectedPath);
                    if (!dir.Exists)
                        return;
                    foreach (var folder in dir.GetDirectories())
                    {
                        var mod_file = folder.GetFiles().FirstOrDefault(e => string.Compare(Path.GetFileNameWithoutExtension(e.Name), folder.Name, true) == 0);
                        if (mod_file == null)
                            continue;
                        if (settings.ModEntries.Any(e => string.Compare(e.AssemblyPath, mod_file.FullName, true) == 0))
                        {
                            return;
                        }
                        settings.ModEntries.Add(new Settings.ModEntry() { AssemblyPath = mod_file.FullName, Active = true });
                        var idx = clbModLibrary.Items.Add(mod_file.FullName);
                        clbModLibrary.SetItemChecked(idx, true);
                    }
                }

            }
        }

        private void btnRemoveMod_Click(object sender, EventArgs e)
        {
            var idx = clbModLibrary.SelectedIndex;
            if (idx != -1)
            {
                clbModLibrary.Items.RemoveAt(idx);
                settings.ModEntries.RemoveAt(idx);
            }
        }

        private void btnWarmupMods_Click(object sender, EventArgs e)
        {
            if (!Warmup() || cobalt_core_launched)
                return;
            if (!LoadMods())
                return;
        }

        private bool LoadMods()
        {
            if (mods_loaded)
                return true;
            //feed all listed manifest to assembly handler
            var svc = ModdedCobaltCoreApp.Services.GetRequiredService<ModAssemblyHandler>();
            var logger = ModdedCobaltCoreApp.Services.GetService<ILogger<MainForm>>();
            logger?.LogInformation("loading mods...");
            foreach (var entry in settings.ModEntries)
            {
                if (!entry.Active)
                    continue;
                svc.LoadModAssembly(new FileInfo(entry.AssemblyPath));
            }

            btnAddAssembly.Enabled = false;
            btnRemoveMod.Enabled = false;
            btnLoadFolder.Enabled = false;
            btnWarmupMods.Enabled = false;
            svc.WarumMods(this);
            mods_loaded = true;

            return true;
        }

        private void clbModLibrary_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            settings.ModEntries[e.Index].Active = e.NewValue == CheckState.Checked;
        }

        private void btnFindGame_Click(object sender, EventArgs e)
        {
            //put code for autodetecting path.
            try
            {
                var path = FindGameFolder.FindGamePath();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    MessageBox.Show("Cobalt Core automatically located. Check text box if you suspect error.");
                    tbPath.Text = path;
                    return;
                }
            }
            catch
            {

            }

            //fallback on just opening cobalt core exe.
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Pick CobaltCore.exe";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!dialog.CheckFileExists)
                        return;
                    var file_name = dialog.FileName;
                    if (string.Compare("CobaltCore.exe", Path.GetFileName(file_name), true) != 0)
                    {
                        MessageBox.Show("Selected File is not CobaltCore.exe");
                        return;
                    }
                    tbPath.Text = file_name;
                }
            }
        }
    }
}