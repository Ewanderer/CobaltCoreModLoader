using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CobaltCoreModding.Components.Services
{
    public class SettingService
    {
        private const string settings_file_name = "ModLoaderSettings.json";
        private readonly ILogger<SettingService> logger;
        private Settings current_settings;

        /// <summary>
        ///
        /// </summary>
        public SettingService(ILogger<SettingService> logger)
        {
            this.logger = logger;
            current_settings = new Settings();
            try
            {
                //Load first setting from local path.
                var file_info = new FileInfo(settings_file_name);
                if (file_info.Exists)
                {
                    using (var stream = file_info.OpenRead())
                    {
                        current_settings = JsonSerializer.Deserialize<Settings>(stream) ?? throw new Exception();
                    }
                }
                else
                {
                    logger.LogInformation("no setting files found. continuing with default.");
                }
            }
            catch
            {
                //Ignore. default values are assumed.
                logger.LogError("Couldn't parse settings file. Continuing with default.");
            }
        }

        public DirectoryInfo? CobaltCoreGamePath
        {
            get
            {
                try
                {
                    var result = new DirectoryInfo(current_settings.CobaltCoreGamePath);
                    if (result.Exists)
                        return result;
                    return null;
                }
                catch { return null; }
            }
            set
            {
                if (value != null && value.Exists && string.Compare(current_settings.CobaltCoreGamePath, value.FullName, true) != 0)
                {
                    current_settings.CobaltCoreGamePath = value.FullName;
                    WriteChanges();
                }
            }
        }

        public DirectoryInfo? CobaltCoreModLibPath
        {
            get
            {
                try
                {
                    var result = new DirectoryInfo(current_settings.CobaltCoreModLibPath);
                    if (result.Exists)
                        return result;
                    return null;
                }
                catch { return null; }
            }
            set
            {
                if (value != null && value.Exists && string.Compare(current_settings.CobaltCoreModLibPath, value.FullName, true) != 0)
                {
                    current_settings.CobaltCoreModLibPath = value.FullName;
                    WriteChanges();
                }
            }
        }

        private void WriteChanges()
        {
            try
            {
                using (var stream = File.Create(settings_file_name))
                {
                    JsonSerializer.Serialize<Settings>(stream, current_settings);
                }
            }
            catch
            {
                logger.LogCritical("Couldn't save settings!");
            }
        }

        [Serializable]
        private class Settings
        {
            public string CobaltCoreGamePath { get; set; } = "";

            public string CobaltCoreModLibPath { get; set; } = "";
        }
    }
}