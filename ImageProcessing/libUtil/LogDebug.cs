using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        /// <summary>
        /// Sends all log messages to the debug stream.
        /// </summary>
        public class LogDebug : ILog
        {
            /// <summary>
            /// Writes a log message to the debug stream.  The appropriate line terminator
            /// is provided.
            /// </summary>
            public virtual void WriteLine(Object aObject)
            {
                System.Diagnostics.Debug.WriteLine(aObject);
            }

            /// <summary>
            /// Writes a log message to the debug stream.
            /// </summary>
            public virtual void Write(Object aObject)
            {
                System.Diagnostics.Debug.Write(aObject);
            }
        }
    }
}