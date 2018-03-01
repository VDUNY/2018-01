using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        /// <summary>
        /// Log is the application-wide log facility.  All log messages are reported to
        /// the singleton instance of the Log class.  Log targets (ILog objects) may register
        /// to receive log messages.
        /// </summary>
        public class Log
        {
            /// <summary>
            /// Singleton accessor.
            /// </summary>
            static public Log Instance = new Log();

            private Set<ILog> m_logSet = new Set<ILog>();
            private LogLevel m_logLevel = LogLevel.Debug;

            /// <summary>
            /// Constructor.
            /// </summary>
            private Log()
            {
            }

            public void SetLogLevel(LogLevel aLogLevel)
            {
                lock (this)
                {
                    m_logLevel = aLogLevel;
                }
            }

            /// <summary>
            /// Sends a log message to all registered targets.  The appropriate line terminator will be
            /// provided.
            /// </summary>
            public virtual void WriteLine(LogLevel aLogLevel, Object aObject)
            {
                lock (this)
                {
                    if (aLogLevel <= m_logLevel)
                    {
                        // write the message to all registered targets
                        Set<ILog>.Enumerator itr = m_logSet.GetEnumerator();
                        while (itr.MoveNext())
                        {
                            itr.Current.Value.Write((int)aLogLevel + " ");
                            itr.Current.Value.WriteLine(aObject);
                        }
                    }
                }
            }

            /// <summary>
            /// Sends a log message to all registered targets.
            /// </summary>
            public virtual void Write(LogLevel aLogLevel, Object aObject)
            {
                lock (this)
                {
                    if (aLogLevel <= m_logLevel)
                    {
                        // write the message to all registered targets
                        Set<ILog>.Enumerator itr = m_logSet.GetEnumerator();
                        while (itr.MoveNext())
                        {
                            itr.Current.Value.Write((int)aLogLevel + " ");
                            itr.Current.Value.Write(aObject);
                        }
                    }
                }
            }

            /// <summary>
            /// Registers a new log target.
            /// </summary>
            public virtual void AddLog(ILog aLog)
            {
                lock (this)
                {
                    m_logSet.Add(aLog);
                }
            }

            /// <summary>
            /// Removes a previously registered log target.
            /// </summary>
            public virtual void RemoveLog(ILog aLog)
            {
                lock (this)
                {
                    m_logSet.Remove(aLog);
                }
            }
        }
    }
}