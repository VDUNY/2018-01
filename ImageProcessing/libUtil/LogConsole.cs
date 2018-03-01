using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        /// <summary>
        /// Sends all log messages to the console.
        /// </summary>
        public class LogConsole : ILog
        {
            /// <summary>
            /// Writes a log message to the console.  The appropriate line terminator
            /// is provided.
            /// </summary>
            public virtual void WriteLine(Object aObject)
            {
                System.Console.WriteLine(aObject);
            }

            /// <summary>
            /// Writes a log message to the console.
            /// </summary>
            public virtual void Write(Object aObject)
            {
                System.Console.Write(aObject);
            }
        }
    }
}