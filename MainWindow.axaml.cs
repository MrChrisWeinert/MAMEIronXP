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
        private int JUMPDISTANCE = 10;

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
            GamesListBox.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            GamesListBox.AddHandler(InputElement.KeyUpEvent, OnPreviewKeyUp, RoutingStrategies.Tunnel);
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
                else if (windowSize.Width == 1600 && windowSize.Height == 900)
                {
                    GameSnapshotPanel.Margin = new Thickness(345, 220, 0, 0);
                    GameSnapshot.Width = 250;
                    GameMetadataPanel.Margin = new Thickness(285, 500, 0, 0);
                    GameMetadata.Width = 365;
                    GameMetadata.FontSize = 14;
                    GameMetadata.Padding = new Thickness(10, 10, 6, 5);
                    GamesListBox.CornerRadius = new CornerRadius(15);
                    GamesListBox.Margin = new Thickness(0,0,0,windowSize.Height/20);
                }
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
                e.Handled = true;
                ExitWindow exitWindow = new ExitWindow();
                exitWindow.Show();
            }
        }

        private void GamesListBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    //If the timer is set to zero, that means the user has *just* initiated the keydown press. Start the timer...
                    if (_startTimeDownPress == new DateTime(0))
                    {
                        _startTimeDownPress = DateTime.Now;
                    }
                    //If the user is already on the last game in the GamesListBox, don't do anything at all.
                    if (GamesListBox.SelectedIndex == GamesListBox.Items.Count - 1)
                    {
                        e.Handled = true;
                        break;
                    }
                    //if the user has been holding the "DOWN" key for over 3 seconds [LONGPRESSMILLISECONDS]...

                    //******KNOWN BUG******
                    //  There is a known bug that I don't want to chase down. If the last game in the list (in my testing, this is "zzyzzyxx") is marked as a favorite, and if we happen to land on that item, the SelectedIndex will not be the last item in the list (in my testing, this was "1994").
                    //  Instead, the SelectedIndex will be it's position at the top of the Favorites list, in my testing, this was "17". So if the user is still holding down, and happens to land on the last item in the list, and that item is marked as a Favorite, the scrolling will scroll past the last item and restart at the top of the list.
                    //  It's irritating, but not super important to me because no one is going to favorite that game. Cue the Bill Gates quote "no one will ever need more than 640K of memory!"
                    //******END KNOWN BUG******
                    else if (_startTimeDownPress.AddMilliseconds(LONGPRESSMILLISECONDS) < DateTime.Now)
                    {
                        Game selectedGame = (Game)GamesListBox.Items[GamesListBox.SelectedIndex];
                        //We have to go through some shenanigans beacuse when a game is marked as a Favorite, we have two copies of the game in our list...
                        // Assume that MsPacMan is marked as a Favorite; we might find it at Index 3 (in our Favorites) and also at Index 1200. When scrolling through the list and encounter the second one, the "SelectedIndex" will appear as 3, which causes our listbox to jump around.
                        // We need to manually force the issue and set the Index to 1200 so it can continue scrolling as intended.
                        if (selectedGame.IsFavorite)
                        {
                            int i = 0;
                            bool isFirstInstanceFound = false;
                            foreach (Game g in GamesListBox.Items)
                            {
                                if (g.Name == selectedGame.Name)
                                {
                                    //We found the first one...
                                    if (!isFirstInstanceFound)
                                    {
                                        isFirstInstanceFound = true;
                                    }
                                    //We found the second one. This is the Index we want to use.
                                    else
                                    {
                                        GamesListBox.SelectedIndex = i;
                                        break;
                                    }
                                }
                                i++;
                            }
                        }
                        //Prevent us from jumping past the end of the list.
                        if (GamesListBox.SelectedIndex + JUMPDISTANCE > GamesListBox.Items.Count)
                        {
                            GamesListBox.SelectedIndex = GamesListBox.Items.Count - 1;
                            GamesListBox.Focus();
                            e.Handled = true;
                            break;
                        }
                        // Jump 10 [JUMPDISTANCE] games in the list.
                        else
                        {
                            GamesListBox.SelectedIndex += JUMPDISTANCE;
                        }
                        GamesListBox.SelectedItem = GamesListBox.Items[GamesListBox.SelectedIndex];
                        GamesListBox.ScrollIntoView(GamesListBox.SelectedItem);
                        GamesListBox.Focus();
                    }
                    break;
                case Key.Up:
                    if (_startTimeUpPress == new DateTime(0))
                    {
                        _startTimeUpPress = DateTime.Now;
                    }
                    else if (_startTimeUpPress.AddMilliseconds(LONGPRESSMILLISECONDS) < DateTime.Now && _startTimeUpPress != new DateTime(0))
                    {
                        if (GamesListBox.SelectedIndex - JUMPDISTANCE < 0)
                        {
                            GamesListBox.SelectedIndex = 0;
                        }
                        else
                        {
                            GamesListBox.SelectedIndex -= JUMPDISTANCE;
                        }
                        GamesListBox.SelectedItem = GamesListBox.Items[GamesListBox.SelectedIndex];
                        GamesListBox.ScrollIntoView(GamesListBox.SelectedItem);
                        GamesListBox.Focus();
                    }
                    break;
            }
        }
        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //When someone releases the up/down joystick, reset the timer to zero.
            switch (e.Key)
            {
                case Key.Down:
                    _startTimeDownPress = new DateTime(0);
                    break;
                case Key.Up:
                    _startTimeUpPress = new DateTime(0);
                    break;
            }
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
                //Update the metadata textbox with the updated playcount
                DisplayMetadata(game);
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
            //Now that we've cleared the list, re-add all the favorites at the top of the list...
            foreach (var game in favorites)
            {
                _games.Add(game);
            }
            //...and then add all games
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