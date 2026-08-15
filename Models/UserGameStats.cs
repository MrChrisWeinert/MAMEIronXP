namespace MAMEIronXP.Models
{
    /// <summary>
    /// User-owned data for a game (favorite status, play count), keyed by the game's Name in user-data.json.
    /// Kept separate from games.json so games.json can be regenerated whenever MAME is updated without losing this data.
    /// </summary>
    public class UserGameStats
    {
        public int PlayCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}
