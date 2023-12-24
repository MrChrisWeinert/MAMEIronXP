using System.Collections.Specialized;
using System.ComponentModel;

namespace MAMEIronXP.Models
{
    public class Game : INotifyCollectionChanged
    {
        private int _playCount;
        private bool _isFavorite;
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int PlayCount
        {
            get => _playCount;
            set
            {
                if (_playCount != value)
                {
                    _playCount = value;
                    OnPropertyChanged(nameof(PlayCount));
                }
            }
        }
        public string Screenshot { get; set; }
        public string Year { get; set; }
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged(nameof(IsFavorite));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Game()
        { }
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

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Toggles the "IsFavorite" flag
        /// </summary>
        /// <returns>current status</returns>
        public bool ToggleFavorite()
        {
            //IsFavorite ^= IsFavorite;
            if (!IsFavorite)
                IsFavorite = true;
            else
                IsFavorite = false;
            return IsFavorite;
        }
        public void IncrementPlayCount()
        {
            PlayCount++;
        }
    }
}
