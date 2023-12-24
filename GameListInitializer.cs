using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Diagnostics;
using System;
using MAMEIronXP.Models;

namespace MAMEIronXP
{
    public class GameListInitializer
    {
        private string _MAMEDirectory;
        private string _mameExe;
        private string _listFull;
        private string _snapsDir;
        private string _gamesJson;
        private string _catver;
        private List<Game> _games = new List<Game>();
        private Dictionary<string, string> _categories = new Dictionary<string, string>();
        private Dictionary<string, float> _versions = new Dictionary<string, float>();
        private List<string> _killList = new List<string>();

        public List<Game> GenerateGameList(string MAMEDirectory, string mameExe, string gamesJson, string snapDir, string catver)
        {
            _MAMEDirectory = MAMEDirectory;
            _mameExe = mameExe;
            _gamesJson = gamesJson;
            _snapsDir = snapDir;
            _catver = catver;
            _listFull = Path.Combine(_MAMEDirectory, "list.xml");
            if (!File.Exists(_listFull))
            {
                GenerateGamesXML();
            }
            LoadCategoriesAndVersions();
            ParseXMLAndFilter();
            //cleanup
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
        /// This function reads from that file and build a list of all the games and their corresponding categories and versions.
        /// This information is uses in further processing to filter out certain categories of games, and TODO: [does something with versions? I don't recall]
        /// </summary>
        private void LoadCategoriesAndVersions()
        {
            using (StreamReader sr = new StreamReader(_catver))
            {
                string line = sr.ReadLine();
                while (line != null) // && !string.IsNullOrEmpty(line)
                {
                    //if the line doesn't contain an equal sign, we don't care about it. Move along...
                    if (!line.Contains("="))
                    {
                        line = sr.ReadLine();
                        continue;
                    }

                    // if there's a "/" we know it's a category, like this:
                    //    mspacman=Maze / Collect
                    if (line.Contains("/"))
                    {
                        var x = line.Split("=");
                        string name = x[0];
                        string category = x[1];
                        _categories.Add(name, category);
                    }
                    // else it just has an equal sign so we know it's a game like this:
                    //   mspacman=0.37b16
                    else
                    {
                        var x = line.Split("=");
                        string name = x[0];
                        string ver = x[1];
                        //TODO: Remember what the heck this is all for, and then comment it approrpriately.
                        if (ver.Contains("b") || ver.Contains("u") || ver.Contains("rc") || ver.Contains("a"))
                        {
                            int letterPos=-1;
                            if (ver.Contains("b"))
                            {
                                letterPos = ver.IndexOf("b");
                            }
                            else if (ver.Contains("u"))
                            {
                                letterPos = ver.IndexOf("u");
                            }
                            else if (ver.Contains("rc"))
                            {
                                letterPos = ver.IndexOf("rc");
                            }
                            else if (ver.Contains("a"))
                            {
                                letterPos = ver.IndexOf("a");
                            }
                            ver = ver.Substring(0, letterPos);
                        }
                        float version = float.Parse(ver);
                        _versions.Add(name, version);
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
            
            //Not sure why I used a HashSet here...
            HashSet<string> drivers = new HashSet<string>();
            HashSet<string> statuses = new HashSet<string>();
            foreach (XmlNode node in root.SelectNodes("machine"))
            {
                Game g = new Game();
                g.Name = node.Attributes["name"].Value.ToString();
                
                if (node.Attributes["cloneof"] != null)
                {
                    //TODO: Make this a configurable parameter via App.config?
                    //Just because *I* don't want clones doesn't mean someone else doesn't.
                    //If it's a clone, we don't want it in our list. Skip it.
                    continue;
                }
                string driverStatus = node.SelectSingleNode("driver")?.Attributes["status"].Value.ToString();
                string driverEmulation = node.SelectSingleNode("driver")?.Attributes["emulation"].Value.ToString();

                //TODO: Is "good" what we really want here? Are there other better statuses like "Excellent" for example?
                if (driverStatus == "good" && driverEmulation == "good")
                {
                    float version = 0;
                    _versions.TryGetValue(g.Name, out version);
                    
                    //TODO: Figure out what issues we had with version .212 and note it. Or get rid of this check.
                    if (version != .212)
                    {
                        g.Description = node.SelectSingleNode("description").InnerText;
                        g.IsFavorite = false;

                        g.PlayCount = 0;
                        g.Year = node.SelectSingleNode("year").InnerText;
                        string category = _categories.Where(x => x.Key == g.Name).FirstOrDefault().Value;
                        if (category != null)
                        {
                            //TODO: Make this a configurable parameter via App.config?
                            //Just because *I* don't want Matrue games doesn't mean someone else doesn't.

                            //Filter out mature games
                            if (category.Contains("* Mature *"))
                            {
                                continue;
                            }
                            if (!category.Contains("/"))
                            {
                                g.Category = category;
                                g.SubCategory = "";
                            }
                            else
                            {
                                string mainCategory = category.Substring(0, category.IndexOf("/") - 1);
                                int start = category.IndexOf("/") + 2;
                                int end = category.Length - start;
                                string subCategory = category.Substring(start, end);
                                g.Category = mainCategory;
                                g.SubCategory = subCategory;
                            }

                            //TODO: Make this a configurable parameter via App.config?
                            //Filter out games by category/subcategory.
                            //I'm sure there's an easier/better way of handling this. Possibly check for "Coin Input"
                            if (g.Category == "Electromechanical" || g.SubCategory == "Reels" || g.Category == "Casino" || g.SubCategory == "Mahjong" || (g.Category == "Rhythm" && (g.SubCategory == "Dance" || g.SubCategory == "Instruments")) || g.Category == "Home Systems" || g.Category == "Professional Systems" || g.Category == "System" || g.Category == "Ball & Paddle" || g.Description.Contains("DECO Cassette") || g.Category == "Multiplay" || g.Description.Contains("PlayChoice-10") || g.Category == "Quiz" || g.Description.Contains("bootleg") || g.Category == "Utilities" || g.Category == "Handheld" || g.Category== "Computer" || g.Category== "Game Console" || g.Category== "Slot Machine" || g.Category== "Misc." || g.Category=="Tabletop" || g.Category== "Board Game" || g.Category=="Calculator")
                            {
                                continue;
                            }

                            g.Screenshot = g.Name + ".png";
                            
                            //Only add games for which we have a valid screenshot
                            //This is purposefully and intentionally mandatory since we want to display game images for 100% of the games in our list. 
                            if (isValidScreenshot(Path.Combine(_snapsDir, g.Screenshot)))
                            {
                                _games.Add(g);
                            }
                        }
                        else
                        {
                            //Is the category ever null?                         
                        }
                    }
                }
                else
                {
                    //TODO: Make this a configurable parameter via App.config?
                    //If the Status/Emulation isn't good, we don't want it in our list. Skip it.
                    continue;
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
