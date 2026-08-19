using System.Collections.Generic;

namespace MAMEIronXP.Models
{
    /// <summary>
    /// Configurable rules (bound from the "GameFilter" section of appsettings.json) used to exclude
    /// games from the generated catalog based on their catver.ini Category/SubCategory/description.
    /// </summary>
    public class GameFilterSettings
    {
        public List<string> ExcludedCategories { get; set; } = new List<string>();
        public List<string> ExcludedSubCategories { get; set; } = new List<string>();
        public List<string> ExcludedCategoryContains { get; set; } = new List<string>();
        public List<string> ExcludedSubCategoryContains { get; set; } = new List<string>();
        public List<string> ExcludedDescriptionContains { get; set; } = new List<string>();
        public List<CategorySubCategoryFilter> ExcludedCategorySubCategoryPairs { get; set; } = new List<CategorySubCategoryFilter>();

        /// <summary>
        /// Reproduces MAMEIronXP's original hard-coded filter list. Used as a fallback when appsettings.json
        /// has no "GameFilter" section at all, so upgrading doesn't silently let excluded categories back in.
        /// </summary>
        public static GameFilterSettings Default()
        {
            return new GameFilterSettings
            {
                ExcludedCategories = new List<string>
                {
                    "Electromechanical",
                    "Casino",
                    "Home Systems",
                    "Professional Systems",
                    "System",
                    "Ball & Paddle",
                    "Multiplay",
                    "Quiz",
                    "Utilities",
                    "Handheld",
                    "Computer",
                    "Game Console",
                    "Slot Machine",
                    "Misc.",
                    "Tabletop",
                    "Board Game",
                    "Gambling",
                    "Calculator"
                },
                ExcludedSubCategories = new List<string> { "Reels", "Mahjong" },
                ExcludedCategoryContains = new List<string> { "* Mature *" },
                ExcludedSubCategoryContains = new List<string> { "* Mature *" },
                ExcludedDescriptionContains = new List<string> { "DECO Cassette", "PlayChoice-10", "bootleg" },
                ExcludedCategorySubCategoryPairs = new List<CategorySubCategoryFilter>
                {
                    new CategorySubCategoryFilter { Category = "Rhythm", SubCategories = new List<string> { "Dance", "Instruments" } }
                }
            };
        }
    }

    public class CategorySubCategoryFilter
    {
        public string Category { get; set; } = "";
        public List<string> SubCategories { get; set; } = new List<string>();
    }
}
