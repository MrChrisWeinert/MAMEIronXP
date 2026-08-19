using MAMEIronXP.Models;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace MAMEIronXP
{
    /// <summary>
    /// Regenerates games.json from scratch by re-running MAME's -listxml against the current GameFilter
    /// settings. Used by the ExitWindow "Regenerate games.json" menu option (e.g. after a MAME update
    /// brings in new/changed games). Safe to run any time since games.json holds no user data
    /// (PlayCount/IsFavorite live in user-data.json - see MainWindow.LoadGamesFromJSON).
    /// </summary>
    public static class GameCatalogService
    {
        public static void Regenerate(AppConfig config)
        {
            if (File.Exists(config.GamesJson))
            {
                File.Delete(config.GamesJson);
            }

            var gameListInitializer = new GameListInitializer(config.MAMEDirectory, config.MameExe, config.SnapDirectory, config.GameFilter);
            var games = gameListInitializer.GenerateGameList().OrderBy(g => g.Description).ToList();

            using StreamWriter sw = new StreamWriter(config.GamesJson, false);
            string json = JsonConvert.SerializeObject(games);
            sw.WriteLine(json);
        }
    }
}
