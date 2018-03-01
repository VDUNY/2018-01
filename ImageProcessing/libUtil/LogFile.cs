using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace LLE.Util
{
    /// <summary>
    /// Subclass of Util.Log
    /// Adds a date time stamp and the logl level _name in front of the message
    /// to better match LLE standard
    /// </summary>
    public class LogFile : ILog, IDisposable
    {
        DateTime openLogDate;
        StreamWriter logFile;
        string logFilename;

        /// <summary>
        /// Initializes the log file
        /// </summary>
        /// <param _name="filename">Path and _name of the file.  This _name will be appended with
        /// a current date stamp</param>
        public LogFile(string filename)
        {
            OpenLogFile(filename);
        }

        private void OpenLogFile(string filename)
        {
            openLogDate = DateTime.Now.Date;
            logFilename = filename + "." + openLogDate.ToString("yyyyMMdd");
            if (File.Exists(logFilename))
                logFile = File.AppendText(logFilename);
            else
                logFile = File.CreateText(logFilename);

            logFile.AutoFlush = true;
        }

        /// <summary>
        /// Writes a log message to the trace stream.  The appropriate line terminator
        /// is provided.
        /// </summary>
        public virtual void WriteLine(Object aObject)
        {
            logFile.Write(aObject);
            logFile.WriteLine("");
        }

        /// <summary>
        /// Writes a log message to the trace stream.
        /// </summary>
        public virtual void Write(Object aObject)
        {
            if (openLogDate != DateTime.Now.Date)
            {
                logFile.Close();
                OpenLogFile(logFilename);
            }

            logFile.Write(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.ff tt") + " ");
            logFile.Write(aObject);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (logFile != null)
                logFile.Close();
        }

        #endregion
    }
}
