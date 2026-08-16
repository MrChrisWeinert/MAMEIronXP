using CommunityToolkit.Mvvm.ComponentModel;

namespace MAMEIronXP.Models
{
    /// <summary>
    /// The CommunityToolkit.Mvvm nuget project lets us (easily) mark properties as Observable.
    /// We are binding an ObservableCollection<Game> to the UI controls (ListBox, TextBlock, etc.) so when these Observable properties are updated, the UI will update automatically.
    /// </summary>
    public partial class Game : ObservableObject
    {
        [ObservableProperty]
        private int _playCount;
        [ObservableProperty]
        private bool _isFavorite;
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public string SubCategory { get; }
        public string Screenshot { get; }
        public string Year { get; }

        public Game(string name, string description, string screenshot, string year, int playcount, bool isfavorite, string category, string subcategory)
        {
            Name = name;
            Description = description;
            Screenshot = screenshot;
            Year = year;
            PlayCount = playcount;
            IsFavorite = isfavorite;
            Category = category;
            SubCategory = subcategory;
        }

        public void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
        }
        public void IncrementPlayCount()
        {
            PlayCount++;
        }

        // PlayCount/IsFavorite live in user-data.json now, not games.json (see UserGameStats).
        // These ShouldSerialize* methods are a Newtonsoft.Json naming convention that suppresses
        // them on write while still allowing them to be read back for the legacy migration path
        // in MainWindow.LoadGamesFromJSON.
        public bool ShouldSerializePlayCount() => false;
        public bool ShouldSerializeIsFavorite() => false;
    }
}
