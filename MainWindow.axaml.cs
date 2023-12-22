using Avalonia.Controls;
using MAMEIronXP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;

namespace MAMEIronXP
{
    public partial class MainWindow : Window
    {
        private string _MAMEDirectory;
        private string _snapDirectory;
        private string _mameExe;
        private string _mameArgs;
        private string _gamesJson;
        private string _logFile;
        private string _catver;
        private string _romsDirectory;
        //TODO
        //private Dictionary<string, System.Windows.Media.ImageSource> _snapshots;


        //Note: Why did I use an ObservableCollection here?
        private ObservableCollection<Game> _games = new ObservableCollection<Game>();
        private Logger _logger;
        private DateTime _startTimeUpPress = new DateTime(0);
        private DateTime _startTimeDownPress = new DateTime(0);
        private DateTime _startTimeCPress;
        private DateTime _startTimeVPress;
        private const int LONGPRESSMILLISECONDS = 3000;
        private int JUMPDISTANCE = 25;

        public MainWindow()
        {
            InitializeComponent();
            _MAMEDirectory = ConfigurationManager.AppSettings["MAMEDirectory"];
            _mameExe = Path.Combine(_MAMEDirectory, ConfigurationManager.AppSettings["MAMEExecutable"]);
            _mameArgs = ConfigurationManager.AppSettings["MAME_Args"];
            _logFile = ConfigurationManager.AppSettings["LogFile"];
            _catver = ConfigurationManager.AppSettings["catverFile"];
            _snapDirectory = ConfigurationManager.AppSettings["SnapDirectory"];
            _romsDirectory = ConfigurationManager.AppSettings["RomsDirectory"];

            //TODO
            //_snapshots = new Dictionary<string, System.Windows.Media.ImageSource>();
            _gamesJson = Path.Combine(_MAMEDirectory, "games.json");
            _logger = new Logger(_logFile);

            //Sanity checks
            string errorText;
            if (!File.Exists(_mameExe))
            {
                errorText = $"Error: {_mameExe} was not found.";
                Console.WriteLine(errorText);
                Console.WriteLine("1) Ensure all prerequisite are met (https://github.com/MrChrisWeinert/MAMEIronXP#prerequisites)");
                Console.WriteLine("2) Check the MAMEDirectory setting in the App.config to make sure you're pointed at your MAME executable.");
                _logger.LogException(errorText, new Exception($"MAME executable ({_mameExe}) not found"));
                Environment.Exit(1);
            }
            else if (!File.Exists(_catver))
            {
                errorText = $"Error: {_catver} was not found.";
                Console.WriteLine("1) Ensure all prerequisite are met (https://github.com/MrChrisWeinert/MAMEIronXP#prerequisites)");
                Console.WriteLine("2) Verify that your MAMEDirectory setting in the App.config is correct and that catver.ini exists in that directory.");
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception($"catver.ini {_catver} not found"));
                Environment.Exit(1);
            }
            else if (!File.Exists(_gamesJson))
            {
                GameListCreator glc = new GameListCreator();
                glc.GenerateGameList(_MAMEDirectory, _mameExe, _gamesJson, _snapDirectory, _catver);
            }
            else if (!Directory.Exists(_snapDirectory))
            {
                errorText = $"Error: {_snapDirectory} was not found.";
                Console.WriteLine("1) Ensure all prerequisite are met (https://github.com/MrChrisWeinert/MAMEIronXP#prerequisites)");
                Console.WriteLine("2) Verify that your MAMEDirectory setting in the App.config is correct and that your roms directory exists that directory.");
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception($"Snap directory ({_snapDirectory}) not found"));
                Environment.Exit(1);
            }
            LoadGamesFromJSON();
            if (_games.Count == 0)
            {
                errorText = $"Error: Unable to load games.";
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception("Games did not load"));
                Environment.Exit(1);
            }

            GamesListBox.ItemsSource = _games;
            GamesListBox.SelectedIndex = 0;
            GamesListBox.SelectionMode = SelectionMode.Single;

        }
        private void LoadGamesFromJSON()
        {
            using (StreamReader sr = new StreamReader(_gamesJson))
            {
                string json = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                List<Game> tempGames = JsonConvert.DeserializeObject<List<Game>>(json);
                foreach (Game g in tempGames)
                {
                    if (!g.IsExcluded && !g.IsClone)
                    {
                        _games.Add(g);
                    }
                }
            }
        }
    }
}