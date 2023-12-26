using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MAMEIronXP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;

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
        private Dictionary<string, Bitmap> _snapshots = new Dictionary<string, Bitmap>();


        //Note: We use an ObservableCollection (versus a List) so we can take action when the collection is changed. For example:
        //   When a game is played; the PlayCount is incremented
        //   If a user Favorites/Unfavorites a game, refresh the list, etc.
        private ObservableCollection<Game> _games = new ObservableCollection<Game>();
        private Logger _logger;
        private DateTime _startTimeUpPress = new DateTime(0);
        private DateTime _startTimeDownPress = new DateTime(0);
        private DateTime _startTimeCPress;
        private DateTime _startTimeVPress;
        private const int LONGPRESSMILLISECONDS = 3000;
        private int JUMPDISTANCE = 25;
        private const int MINIMUM_X_RESOLUTION = 640;
        private const int MINIMUM_Y_RESOLUTION = 480;

        public MainWindow()
        {
            InitializeComponent();
            GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;
            _MAMEDirectory = ConfigurationManager.AppSettings["MAMEDirectory"];
            _mameExe = Path.Combine(_MAMEDirectory, ConfigurationManager.AppSettings["MAMEExecutable"]);
            _mameArgs = ConfigurationManager.AppSettings["MAME_Args"];
            _logFile = ConfigurationManager.AppSettings["LogFile"];
            _catver = ConfigurationManager.AppSettings["catverFile"];
            _snapDirectory = ConfigurationManager.AppSettings["SnapDirectory"];
            _romsDirectory = ConfigurationManager.AppSettings["RomsDirectory"];

            //INFO: games.json is a file that MAMEIronXP generates once (and only once). It is the main working file that MAMEIronXP subsequently uses to load games and is also where a game's PlayCount is tracked as well as its "Favorite" status.
            //      This file is periodically persisted back to disk if there are changes (i.e. a game is marked as a Favorite or it's Play Count is incremented).
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
            else if (!Directory.Exists(_snapDirectory))
            {
                errorText = $"Error: {_snapDirectory} was not found.";
                Console.WriteLine("1) Ensure all prerequisite are met (https://github.com/MrChrisWeinert/MAMEIronXP#prerequisites)");
                Console.WriteLine("2) Verify that your MAMEDirectory setting in the App.config is correct and that your roms directory exists that directory.");
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception($"Snap directory ({_snapDirectory}) not found"));
                Environment.Exit(1);
            }
            else if (!File.Exists(_gamesJson))
            {
                GameListInitializer gameListInitializer = new GameListInitializer();
                foreach (Game game in gameListInitializer.GenerateGameList(_MAMEDirectory, _mameExe, _snapDirectory, _catver))
                {
                    _games.Add(game);
                }
                PersistGamesFile();
            }
            LoadGamesFromJSON();
            if (_games.Count == 0)
            {
                errorText = $"Error: Unable to load games.";
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception("Games did not load"));
                Environment.Exit(1);
            }
            LoadImagesIntoDictionary();
            GamesListBox.ItemsSource = _games;
            GamesListBox.SelectedIndex = 0;
            GamesListBox.SelectionMode = SelectionMode.Single;
            
            //TODO: Make everything automatically scale, or perhaps have some pre-defined screen sizes, or maybe just throw values in the App.config
            if (this.ClientSize.Height < MINIMUM_Y_RESOLUTION || this.ClientSize.Width < MINIMUM_X_RESOLUTION)
            {
                errorText = $"Error: This application was designed to work at 640x480 resolution or higher.";
                Console.WriteLine(errorText);
                _logger.LogException(errorText, new Exception($"Screen resolution was too low {this.Width}x{this.Height}."));
                Environment.Exit(1);
            }
            //None of these should be hard-coded values. They should auto-scale. However, the listbox must have a defined height otherwise the scrolling won't work properly.
            GamesListBox.CornerRadius = new Avalonia.CornerRadius(25);
            GamesListBox.Height = 1100;
            GamesListBox.Width = 1100;
            GamesListBox.Margin = new Avalonia.Thickness(150);
            //GamesListTextBox.FontSize = 48;
            //TODO: Hide scrollbar
        }
        void LoadImagesIntoDictionary()
        {
            //We shouldn't run into issues loading files from disk since we already checked for valid images when we built our game list.
            foreach (Game game in _games)
            {
                string filePath = Path.Combine(_snapDirectory, game.Screenshot);                
                var image = new Bitmap(filePath);
                _snapshots[game.Name] = image;
            }
        }
        private void GamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the selection change
            // e.g., var selectedItem = MyListBox.SelectedItem;

            var listBox = (ListBox)sender;
            if (listBox.SelectedItem is Game selectedItem)
            {
                if (_snapshots.TryGetValue(selectedItem.Name, out var image))
                {
                    GameSnapshot.Source = image;
                }
            }
        }
        private void LoadGamesFromJSON()
        {
            //TODO: Add error handling in here in case someone hand-edits the games.json file and makes a mistake.
            using (StreamReader sr = new StreamReader(_gamesJson))
            {
                string json = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                _games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(json);
            }
        }
        //WIP for when we start messing with the Favorites...
        public class FavoriteIconConverter : IValueConverter
        {
            public static readonly FavoriteIconConverter Instance = new();

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (value is Boolean isFavorite // && parameter is string targetCase
                    && targetType.IsAssignableTo(typeof(string)))
                {
                    if (isFavorite)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
                // converter used for the wrong type
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }

            public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
        private void PersistGamesFile()
        {
            using (StreamWriter sw = new StreamWriter(_gamesJson, false))
            {
                string json = JsonConvert.SerializeObject(_games);
                sw.WriteLine(json);
                sw.Close();
            }
        }
    }
}