using System;
using System.Diagnostics;
using System.IO;

namespace MAMEIronXP
{
    public class Logger
    {
        private string _logfile;
        private static TraceSwitch traceSwitch = new TraceSwitch("TraceLevelSwitch", null);
        public Logger(string logfile)
        {
            _logfile = logfile;
        }
        public void LogVerbose(string message)
        {
            if (traceSwitch.TraceVerbose)
            {
                WriteToLogFile(message);
            }
        }
        public void LogInfo(string message)
        {
            if (traceSwitch.TraceInfo)
            {
                WriteToLogFile(message);
            }
        }
        public void LogException(string message, Exception ex)
        {
            if (traceSwitch.TraceError)
            {
                WriteToLogFile($"{message}=====Exception: {ex.ToString()}");
            }
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
