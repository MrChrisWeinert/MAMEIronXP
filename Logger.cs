using System;
using System.IO;

namespace MAMEIronXP
{
    public class Logger
    {
        private string _logfile;
        public Logger(string logfile)
        {
            _logfile = logfile;
        }
        public void LogInfo(string message)
        {
            WriteToLogFile(message);
        }
        private void WriteToLogFile(string text)
        {
            using (StreamWriter sw = File.AppendText(_logfile))
            {
                sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}\t{text}");
                sw.Flush();
                sw.Close();
            }
        }
    }
}
