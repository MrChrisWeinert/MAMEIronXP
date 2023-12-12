using Avalonia.Controls;
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
        private string _rootDirectory;
        private string _snapDirectory;
        private string _mameExe;
        private string _mameArgs;
        private string _gamesJson;
        private string _logFile;
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
        private int JUMPDISTANCE=25;

        public MainWindow()
        {
            InitializeComponent();
            _rootDirectory = ConfigurationManager.AppSettings["rootDirectory"];
            _mameArgs = ConfigurationManager.AppSettings["MAME_Args"];
            _logFile = Path.Combine(_rootDirectory, "log.txt");
            _mameExe = Path.Combine(_rootDirectory, "MAME64.EXE");
            _snapDirectory = Path.Combine(_rootDirectory, "snap");
            //TODO
            //_snapshots = new Dictionary<string, System.Windows.Media.ImageSource>();
            _gamesJson = Path.Combine(_rootDirectory, "games.json");
            _logger = new Logger(_logFile);

            //Sanity checks
            string errorText;
            if (!File.Exists(_mameExe))
            {
                errorText = $"{_mameExe} does not exist.";
                //TODO
                //MessageBox.Show(errorText, "Fatal Error");
                _logger.LogException(errorText, new Exception("MAME executable not found"));
                Environment.Exit(1);
            }
            else if (!File.Exists(_gamesJson))
            {
                GameListCreator glc = new GameListCreator();
                glc.GenerateGameList(_mameExe, _gamesJson, _snapDirectory);
            }
            else if (!Directory.Exists(_snapDirectory))
            {
                errorText = $"{_snapDirectory} does not exist.";
                //TODO
                //MessageBox.Show(errorText, "Fatal Error");
                _logger.LogException(errorText, new Exception("Snap directory not found"));
                Environment.Exit(1);
            }
            LoadGamesFromJSON();
            if (_games.Count == 0)
            {
                errorText = $"_games count is zero.";
                //TODO
                //MessageBox.Show(errorText, "Fatal Error");
                _logger.LogException(errorText, new Exception("Games did not load"));
                Environment.Exit(1);
            }

            //Application.Current.MainWindow.Left = 0;
            //Application.Current.MainWindow.Top = 0;
            //HideMouse();


            //#region Load games from disk and bind to the ListView

            //lvGames.ItemsSource = GetUpdatedGameList();

            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvGames.ItemsSource);
            //lvGames.Focus();
            //lvGames.SelectionMode = SelectionMode.Single;
            //lvGames.SelectedIndex = 0;
            //#endregion

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