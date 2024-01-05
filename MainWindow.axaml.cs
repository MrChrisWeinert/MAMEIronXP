using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using MAMEIronXP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
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

        //TODO: Should have ways of quickly navigating through the list(jump to end, jump to beginning, "accelerate" through the list using the JUMPDISTANCE that was in MAMEIron, etc)
        private int JUMPDISTANCE = 25;

        public MainWindow()
        {
            //Initialize all our private variables
            _MAMEDirectory = ConfigurationManager.AppSettings["MAMEDirectory"];
            _mameExe = Path.Combine(_MAMEDirectory, ConfigurationManager.AppSettings["MAMEExecutable"]);
            _mameArgs = ConfigurationManager.AppSettings["MAME_Args"];
            _logFile = ConfigurationManager.AppSettings["LogFile"];
            _snapDirectory = ConfigurationManager.AppSettings["SnapDirectory"];
            _gamesJson = Path.Combine(_MAMEDirectory, "games.json");
            _logger = new Logger(_logFile);

            InitializeComponent();
            PrepareForLaunch();
            GamesListBox.ItemsSource = _games;
            GamesListBox.SelectedIndex = 0;
            GamesListBox.SelectionMode = SelectionMode.Single;
            GamesListBox.SelectionChanged += GamesListBox_SelectionChanged;
            GamesListBox.KeyDown += GamesListBox_KeyDown;
            GamesListBox.KeyUp += GamesListBox_KeyUp;
            PointerPressed += MainWindow_PointerPressed;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GamesListBox.Focus();
            DisplaySnapshot((Game)GamesListBox.SelectedItem);
            DisplayMetadata((Game)GamesListBox.SelectedItem);

            //1) Get screen resolution and move objects where they need to be accordingly
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                PixelSize windowSize = desktop.MainWindow.Screens.Primary.Bounds.Size;
                if (windowSize.Width == 2560 && windowSize.Height == 1440)
                {
                    //1440p
                    GameSnapshotPanel.Margin = new Thickness(550, 350, 0, 0);
                    GameSnapshot.Width = 400;
                    GameMetadataPanel.Margin = new Thickness(460, 810, 0, 0);
                    GameMetadata.Width = 580;
                    GameMetadata.FontSize = 22;
                    GameMetadata.Padding = new Thickness(10, 10, 6, 5);
                    GamesListBox.CornerRadius = new CornerRadius(25);
                    //ImageIsFavorite.Width = 50;
                    //ImageIsFavorite.Height = 50;
                    //ImageIsNotFavorite.Width = 50;
                    //ImageIsNotFavorite.Height = 50;
                    GamesListBox.Margin = new Thickness(0,0,0,windowSize.Height/20);                    
                }
                else if (windowSize.Width == 1920 && windowSize.Height == 1080)
                {
                    //1080p
                    GameSnapshotPanel.Margin = new Thickness(415, 275, 0, 0);
                    GameSnapshot.Width = 300;
                    GameMetadataPanel.Margin = new Thickness(345, 605, 0, 0);
                    GameMetadata.Width = 433;
                    GameMetadata.FontSize = 16.5;
                    GameMetadata.Padding = new Thickness(10, 10, 6, 5);
                    GamesListBox.CornerRadius = new CornerRadius(15);
                    GamesListBox.Margin = new Thickness(0,0,0,windowSize.Height/20);
                }
                    GamesListBox.Margin = new Thickness(0,0,0,windowSize.Height/20);
            }
        }
        /// <summary>
        /// We always want to "handle" the event so the mouse click doesn't actually mouse-click in our app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var x = e.GetCurrentPoint(this).Properties;

            //The Tankstick's "exit" button at the top right is mapped to a Middle Mouse-click.
            if (x.IsMiddleButtonPressed)
            {
                ExitWindow exitWindow = new ExitWindow();
                exitWindow.Show();
            }
        }

        private void GamesListBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void GamesListBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C:
                    e.Handled = true;
                    ToggleFavorite();
                    break;
                case Key.D1:
                    e.Handled = true;
                    var listBox = (ListBox)sender;
                    if (listBox.SelectedItem is Game game)
                    {
                        StartGame(game);
                    }                        
                    break;
                case Key.D3:
                    //volume Down
                    break;
                case Key.D4:
                    //volume Up
                    break;
                case Key.Escape:
                    ExitWindow exitWindow = new ExitWindow();
                    exitWindow.Show();
                    break;
            }
        }

        private void PrepareForLaunch()
        {
            //Check prerequisites and generate games.json if it doesn't exist
            string errorText;
            if (!File.Exists(_logFile))
            {
                //attempt to create the log file and bail out if it can't be created.
                try
                {
                    File.Create(_logFile);
                }
                catch
                {
                    errorText = $"Error: Unable to create log file here: {_logFile}";
                    Console.WriteLine(errorText);
                    Console.WriteLine("1) Check the LogFile setting in the App.config to make sure you're pointed at a location suitable for logging.");
                    Environment.Exit(1);
                }
            }
            if (!File.Exists(_mameExe))
            {
                errorText = $"Error: {_mameExe} was not found.";
                Console.WriteLine(errorText);
                Console.WriteLine("1) Check out the Getting Started section of the README (https://github.com/MrChrisWeinert/MAMEIronXP#getting-started)");
                Console.WriteLine("2) Check the MAMEDirectory and MAMEExecutable settings in the App.config to make sure you're pointed at your MAME executable.");
                _logger.LogInfo(errorText);
                Environment.Exit(1);
            }
            else if (!Directory.Exists(_snapDirectory))
            {
                errorText = $"Error: {_snapDirectory} was not found.";
                Console.WriteLine("1) Check out the Getting Started section of the README (https://github.com/MrChrisWeinert/MAMEIronXP#getting-started)");
                Console.WriteLine("2) Verify that your SnapDirectory setting in the App.config is correct.");
                Console.WriteLine(errorText);
                _logger.LogInfo(errorText);
                Environment.Exit(1);
            }
            else if (!File.Exists(_gamesJson))
            {
                //INFO: games.json is a file that MAMEIronXP generates once (and only once). It is the main working file that MAMEIronXP subsequently uses to load games and is also where a game's PlayCount is tracked as well as its "Favorite" status.
                //      This file is periodically persisted back to disk if there are changes (i.e. a game is marked as a Favorite or it's Play Count is incremented).
                GameListInitializer gameListInitializer = new GameListInitializer(_MAMEDirectory, _mameExe, _snapDirectory);
                foreach (Game game in gameListInitializer.GenerateGameList().OrderBy(x => x.Description))
                {
                    _games.Add(game);
                }
                PersistGamesFile();
            }
            LoadGamesFromJSON();
            LoadImagesIntoDictionary();
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
            var listBox = (ListBox)sender;
            if (listBox.SelectedItem is Game game)
            {
                DisplaySnapshot(game);
                DisplayMetadata(game);
            }
        }
        private void DisplaySnapshot(Game game)
        {
            if (_snapshots.TryGetValue(game.Name, out var image))
            {
                GameSnapshot.Source = image;
            }
        }
        private void DisplayMetadata(Game game)
        {
            string subCategory = "";
            if (!string.IsNullOrEmpty(game.SubCategory))
            {
                subCategory = $"/{game.SubCategory}";
            }
            GameMetadata.Text = $"Year: {game.Year} Plays: {game.PlayCount} Category: {game.Category}{subCategory}";
        }

        private void LoadGamesFromJSON()
        {
            //TODO: Add error handling in here in case someone hand-edits the games.json file and makes a mistake.
            using (StreamReader sr = new StreamReader(_gamesJson))
            {
                string json = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                var tempListOfGames = JsonConvert.DeserializeObject<ObservableCollection<Game>>(json);
                //Ensure favorites show up at the top of the list
                foreach (Game game in tempListOfGames.Where(y => y.IsFavorite == true).OrderBy(y => y.Description))
                {
                    _games.Add(game);
                }
                //Note: While we have favorites listed at the top, we *also* want those games to show up in their normal spot.
                // So, yes, we will have duplicates in our list, and this is intentional.
                foreach (Game game in tempListOfGames.OrderBy(y => y.Description))
                {
                    _games.Add(game);
                }
            }
            if (_games.Count == 0)
            {
                string errorText = $"Error: Unable to load games.";
                Console.WriteLine(errorText);
                _logger.LogInfo(errorText);
                Environment.Exit(1);
            }
        }
        
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
            //Our "ObservableCollection<Game> _games" will contain duplicates for any favorited game.
            //We need to ensure we only persist each game once, so that's what this chunk of code does.
            List<Game> games = new List<Game>();
            foreach (Game game in _games)
            {
                if (!games.Contains(game))
                {
                    games.Add(game);
                }
            }

            using (StreamWriter sw = new StreamWriter(_gamesJson, false))
            {
                string json = JsonConvert.SerializeObject(games);
                sw.WriteLine(json);
                sw.Close();
            }
        }
        private void StartGame(Game game)
        {
            string st = _mameExe;
            Process process = new Process();
            process.StartInfo.FileName = st;
            process.StartInfo.WorkingDirectory = _MAMEDirectory;
            process.StartInfo.Arguments = game.Name + " " + _mameArgs;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                game.IncrementPlayCount();
                PersistGamesFile();
            }
            else
            {
                _logger.LogInfo($"Couldn't start game: {game.Name} via {st}.");
            }
            process.Close();
        }
        private void ToggleFavorite()
        {
            //Need to use First() not Single() here because a favorited game will exist in the list twice and .Single() will throw an exception
            _games.First(x => x.Name == ((Game)GamesListBox.SelectedItem).Name).ToggleFavorite();

            //PlaySound("pacman_cherry.wav");
            PersistGamesFile();
            RefreshGamesListBox();
        }
        /// <summary>
        /// Probably not the most efficient way to do this but it's fast enough.
        /// </summary>
        private void RefreshGamesListBox()
        {
            Game selectedGame = (Game)GamesListBox.SelectedItem;

            //We use a Hashset here because our "_games" collection has duplicates, and using a HashSet will ensure each game only gets added once
            HashSet<Game> favorites = _games.Where(g=> g.IsFavorite).OrderBy(g => g.Description).ToHashSet<Game>();
            HashSet<Game> allTheGames = _games.OrderBy(g => g.Description).ToHashSet<Game>();
            _games.Clear();
            foreach (var game in favorites)
            {
                _games.Add(game);
            }
            foreach (var game in allTheGames)
            {
                _games.Add(game);
            }
            //Restore the SelectedItem back to whatever was selected before we refreshed the list.
            GamesListBox.SelectedItem = selectedGame;
            GamesListBox.Focus();
        }
    }
}