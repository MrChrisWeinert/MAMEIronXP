namespace MAMEIronXP
{
    public class Game
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int PlayCount { get; set; }
        public string Screenshot { get; set; }
        public string Year { get; set; }
        public bool IsExcluded { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsClone { get; set; }
        public Game()
        { }
        public Game(string name, string description, string screenshot, string year, int playcount, bool isfavorite, bool isexcluded, string category, string subcategory, bool isclone)
        {
            Name = name;
            Description = description;
            Screenshot = screenshot;
            Year = year;
            PlayCount = playcount;
            IsFavorite = isfavorite;
            IsExcluded = isexcluded;
            IsClone = isclone;
            Category = category;
            SubCategory = subcategory;
        }
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
