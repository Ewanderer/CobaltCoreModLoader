using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace CobaltCoreModLoaderApp
{
    /// <summary>
    /// Interaction logic for LoaderMainWindow.xaml
    /// </summary>
    public partial class LoaderMainWindow : Window
    {

        public HeartService heart { get; init; }

        public bool LaunchedCobaltCore { get; set; } = false;

        public LoaderMainWindow(HeartService heart)
        {
            InitializeComponent();
            this.heart = heart;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!LaunchedCobaltCore)
            {
                heart.ModdedCobaltCoreAppLifeTime.StopApplication();
            }
        }

        /// <summary>
        /// Check Cobalt Core Path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnWarmup_Click(object sender, RoutedEventArgs e)
        {
            Warmup();
        }

        bool warmup_done = false;

        public bool Warmup()
        {
            if (warmup_done)
                return true;
            string cc_exe_path = "";
            if (!File.Exists(CobaltCoreGamePathTextField.Text))
            {

                if (!Directory.Exists(CobaltCoreGamePathTextField.Text))
                {
                    MessageBox.Show("No Game path give!");
                    return false;
                }
                cc_exe_path = System.IO.Path.Combine(CobaltCoreGamePathTextField.Text, System.IO.Path.GetFileName("CobaltCore.exe"));
                if (!File.Exists(cc_exe_path))
                {
                    MessageBox.Show("Game directory doesn't contain cobalt core.exe");
                    return false;
                }
            }
            else
            {
                if (string.Compare("CobaltCore.exe", new FileInfo(CobaltCoreGamePathTextField.Text).Name) != 0)
                {
                    MessageBox.Show("File given is not CobaltCore.exe");
                    return false;
                }
                cc_exe_path = CobaltCoreGamePathTextField.Text;
            }

            //load cobalt core exe, which is required.
            try
            {
                var svc = heart.ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreModLoader.Services.CobaltCoreHandler>();
                svc.LoadupCobaltCore(new FileInfo(cc_exe_path));
                warmup_done = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            return warmup_done;
        }

        private void BtnCloseRun_Click(object sender, RoutedEventArgs e)
        {
            if (!Warmup() || LaunchedCobaltCore)
                return;
            var svc = heart.ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreModLoader.Services.CobaltCoreHandler>();
            _ = Task.Run(() => svc.RunCobaltCore(new string[] { "--debug" }));
            LaunchedCobaltCore = true;
            Close();
        }

        private void BtnRunView_Click(object sender, RoutedEventArgs e)
        {
            if (!Warmup() || LaunchedCobaltCore)
                return;

            var svc = heart.ModdedCobaltCoreApp.Services.GetRequiredService<CobaltCoreModLoader.Services.CobaltCoreHandler>();
            _ = Task.Run(() => svc.RunCobaltCore(new string[] { "--debug" }));
            LaunchedCobaltCore = true;
        }

        private void BtnLoadSingleModFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLoadCollection_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
