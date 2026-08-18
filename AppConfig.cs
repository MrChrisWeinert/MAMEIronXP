using Microsoft.Extensions.Configuration;
using MAMEIronXP.Models;
using System;
using System.IO;

namespace MAMEIronXP
{
    /// <summary>
    /// Loads appsettings.json once and exposes the resolved MAME paths/settings needed by both
    /// MainWindow (normal launch) and ExitWindow (the "Regenerate games.json" menu option).
    /// </summary>
    public class AppConfig
    {
        public string MAMEDirectory { get; private set; } = "";
        public string MameExe { get; private set; } = "";
        public string MameArgs { get; private set; } = "";
        public string LogFile { get; private set; } = "";
        public string SnapDirectory { get; private set; } = "";
        public string GamesJson { get; private set; } = "";
        public string UserDataJson { get; private set; } = "";
        public GameFilterSettings GameFilter { get; private set; } = new GameFilterSettings();

        public static AppConfig Load()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var config = new AppConfig();
            config.MAMEDirectory = ResolvePath(GetSetting(configuration, "MAME:Directory", "."), AppContext.BaseDirectory);
            config.MameExe = ResolvePath(
                GetSetting(configuration, "MAME:Executable", OperatingSystem.IsWindows() ? "mame.exe" : "mame"),
                config.MAMEDirectory);
            config.MameArgs = GetSetting(configuration, "MAME:Args", "-autosave -skip_gameinfo -video bgfx");
            config.LogFile = ResolvePath(
                GetSetting(configuration, "MAME:LogFile", "MAMElogfile.log"),
                config.MAMEDirectory);
            config.SnapDirectory = ResolvePath(
                GetSetting(configuration, "MAME:SnapDirectory", "snap"),
                config.MAMEDirectory);
            config.GamesJson = Path.Combine(config.MAMEDirectory, "games.json");
            config.UserDataJson = Path.Combine(config.MAMEDirectory, "user-data.json");

            IConfigurationSection filterSection = configuration.GetSection("GameFilter");
            //If appsettings.json has no "GameFilter" section at all, fall back to the original hard-coded
            //filter list rather than an empty (i.e. unfiltered) one. If the section exists, the user's
            //config fully replaces the defaults (Bind() appends to existing list contents, so we bind
            //into a fresh empty GameFilterSettings instead of merging onto the defaults).
            config.GameFilter = filterSection.Exists() ? new GameFilterSettings() : GameFilterSettings.Default();
            if (filterSection.Exists())
            {
                filterSection.Bind(config.GameFilter);
            }

            return config;
        }

        private static string GetSetting(IConfiguration configuration, string key, string fallback)
        {
            string? value = configuration[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string ResolvePath(string path, string basePath)
        {
            return Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(basePath, path));
        }
    }
}
