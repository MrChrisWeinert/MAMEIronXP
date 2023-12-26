using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using MAMEIronXP.Models;
using Avalonia.Utilities;

namespace MAMEIronXP
{
    public class GameListInitializer
    {
        private string _MAMEDirectory;
        private string _mameExe;
        private string _listFull;
        private string _snapsDir;
        private string _catver;
        private List<Game> _games = new List<Game>();
        private Dictionary<string, string> _categories = new Dictionary<string, string>();
        private List<string> _killList = new List<string>();

        public List<Game> GenerateGameList(string MAMEDirectory, string mameExe, string snapDir, string catver)
        {
            _MAMEDirectory = MAMEDirectory;
            _mameExe = mameExe;
            _snapsDir = snapDir;
            _catver = catver;
            _listFull = Path.Combine(_MAMEDirectory, "list.xml");
            if (!File.Exists(_listFull))
            {
                GenerateGamesXML();
            }
            LoadCategories();
            ParseXMLAndFilter();

            //Cleanup
            File.Delete(_listFull);
            return _games;
        }

        /// <summary>
        /// Use the MAME executable along with the -listxml parameter to generate a complete description of the drivers in MAME in XML format. 
        /// This XML list contains all the current driver information including driver status, required devices, rom information and other dependencies.
        /// </summary>
        private void GenerateGamesXML()
        {
            string st = _mameExe;
            Process process = new Process();
            process.StartInfo.FileName = st;
            process.StartInfo.WorkingDirectory = _MAMEDirectory;
            process.StartInfo.Arguments = " -listxml";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            //Perf note: It will take MAME a few minutes on low-end hardware to generate the list.xml file. It's roughly 263MB in size (version .244).
            process.Start();

            using (StreamReader sr = process.StandardOutput)
            {
                using (StreamWriter sw = new StreamWriter(_listFull))
                {
                    sw.Write(sr.ReadToEnd());
                    sw.Close();
                    sw.Dispose();
                }
                sr.Close();
                sr.Dispose();
                sr.DiscardBufferedData();
            }
        }

        /// <summary>
        /// catver.ini consists of two main sections:
        /// 1) Categories
        /// 2) Versions
        ///
        /// This function reads from that file and build a list of all the games and their corresponding categories.
        /// We use filter out certain games based on their categories.
        /// </summary>
        private void LoadCategories()
        {
            using (StreamReader sr = new StreamReader(_catver))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    //We're looking for Category lines which are structured like this: "mspacman=Maze / Collect"
                    if (line.Contains("=") && line.Contains("/"))
                    {
                        var x = line.Split("=");
                        string name = x[0];
                        string category = x[1];
                        _categories.Add(name, category);
                    }
                    line = sr.ReadLine();
                }
            }
        }

        private void ParseXMLAndFilter()
        {
            XmlDocument doc = new XmlDocument();
            //Perf note: The list.xml file is roughly 263B (version .244). Loading this into memory uses ~2GB of RAM.
            doc.Load(_listFull);
            XmlNode root = doc.SelectSingleNode("mame");
            
            foreach (XmlNode node in root.SelectNodes("machine"))
            {
                string gameName = node.Attributes["name"].Value.ToString();
                if (node.Attributes["cloneof"] != null)
                {
                    //TODO: Make this a configurable parameter via App.config?
                    //Just because *I* don't want clones doesn't mean someone else doesn't.
                    //If it's a clone, we don't want it in our list. Skip it.
                    continue;
                }
                string driverStatus = node.SelectSingleNode("driver")?.Attributes["status"].Value.ToString();
                string driverEmulation = node.SelectSingleNode("driver")?.Attributes["emulation"].Value.ToString();

                //If a game's Status/Emulation aren't both good, we don't want it in the list.
                if (driverStatus == "good" && driverEmulation == "good")
                {
                    string gameDescription = node.SelectSingleNode("description").InnerText;
                    string gameYear = node.SelectSingleNode("year").InnerText;
                    string category = _categories.Where(x => x.Key == gameName).FirstOrDefault().Value;
                    string gameCategory = "";
                    string gameSubCategory = "";

                    //Only add games if they have a category. If the category is null it's likely a "system" or something that we don't otherwise want.
                    if (category != null)
                    {
                        if (!category.Contains("/"))
                        {
                            gameCategory = category;
                        }
                        else
                        {
                            string mainCategory = category.Substring(0, category.IndexOf("/") - 1);
                            int start = category.IndexOf("/") + 2;
                            int end = category.Length - start;
                            string subCategory = category.Substring(start, end);
                            gameCategory = mainCategory;
                            gameSubCategory = subCategory;
                        }
                        if (node.SelectSingleNode("input")?.Attributes["coins"]?.Value == null)
                        {
                            //No coin input so it's probably not a traditional arcade. Skip it.
                            continue;
                        }

                        //TODO: Make this a configurable parameter via App.config?
                        //Filter out games by category/subcategory.
                        //I'm sure there's an easier/better way of handling this.
                        if (gameCategory == "Electromechanical" || 
                            gameCategory.Contains("* Mature *") ||
                            gameSubCategory.Contains("* Mature *") ||
                            gameSubCategory == "Reels" || 
                            gameCategory == "Casino" || 
                            gameSubCategory == "Mahjong" || 
                            (gameCategory == "Rhythm" && (gameSubCategory == "Dance" || gameSubCategory == "Instruments")) || 
                            gameCategory == "Home Systems" || 
                            gameCategory == "Professional Systems" || 
                            gameCategory == "System" || 
                            gameCategory == "Ball & Paddle" ||
                            gameDescription.Contains("DECO Cassette") || 
                            gameCategory == "Multiplay" ||
                            gameDescription.Contains("PlayChoice-10") || 
                            gameCategory == "Quiz" ||
                            gameDescription.Contains("bootleg") || 
                            gameCategory == "Utilities" || 
                            gameCategory == "Handheld" || 
                            gameCategory== "Computer" || 
                            gameCategory== "Game Console" || 
                            gameCategory== "Slot Machine" || 
                            gameCategory== "Misc." || 
                            gameCategory=="Tabletop" || 
                            gameCategory== "Board Game" || 
                            gameCategory=="Calculator")
                        {
                            continue;
                        }

                        string gameScreenshot = gameName + ".png";
                            
                        //Only add games for which we have a valid screenshot
                        //This is purposefully and intentionally mandatory since we want to display game images for 100% of the games in our list. 
                        if (isValidScreenshot(Path.Combine(_snapsDir, gameScreenshot)))
                        {
                            _games.Add(new Game(gameName, gameDescription, gameScreenshot, gameYear, 0, false, gameCategory, gameSubCategory));
                        }
                    }
                }
            }
        }

        private bool isValidScreenshot(string screenshot)
        {
            //TODO: These shouldn't be hard-coded here. Find a better spot like App.Config, or maybe some sort of "killlist/ignore" file.
            //There are several screenshots for games that use a "default" or "image not found" or some generic varation of a blank screen. I don't want those games, so I filter them out manually.
            _killList.Add("a766be38df34c5db61ad5cd559919487");
            _killList.Add("30ab4d58332ef5332affe5f3320c647a");
            _killList.Add("1b7928278186f053777dea680b0a2b2d");
            _killList.Add("e2b8f257fea66b661ee70efc73b6c84a");
            _killList.Add("ab541cffaccbff5f9d2ad2d9031c0c48");
            _killList.Add("6a4ca1ab352df8af4a25c50a65bb8963");
            _killList.Add("26bdf324b11da6190f38886a3b0f7598");
            //Nerd note^ ... This would be a perfect use case for AI. If we trained the model properly, it would be able to tell us if each screenshot is an "image not found", or mostly black, or video poker, etc.

            if (File.Exists(screenshot))
            {

                string md5 = HashFile(screenshot);
                //Don't add it if it's in the kill list
                if (_killList.Contains(md5))
                {
                    return false;
                }
                else
                {
                    //There were a bunch of screenshots that I wanted to kill and they all had similar timestamps (but varying MD5 hashes).
                    //This code allows us to filter out based on timestamp. Again, leaving this code here "just in case" 

                    //DateTime dt = File.tim(screenshot);
                    //DateTime dtStart = new DateTime(2015, 01, 16, 23, 18, 0);
                    //DateTime dtEnd = new DateTime(2015, 01, 16, 23, 20, 0);
                    //if (dt > dtStart && dt < dtEnd )
                    //{
                    //    return false;
                    //}
                    //else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        static string HashFile(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return HashFile(fs);
            }
        }

        static string HashFile(FileStream stream)
        {
            StringBuilder sb = new StringBuilder();

            if (stream != null)
            {
                stream.Seek(0, SeekOrigin.Begin);

                MD5 md5 = MD5CryptoServiceProvider.Create();
                byte[] hash = md5.ComputeHash(stream);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));

                stream.Seek(0, SeekOrigin.Begin);
            }

            return sb.ToString();
        }
    }
}
