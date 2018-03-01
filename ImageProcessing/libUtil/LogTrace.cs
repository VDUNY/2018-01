using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        /// <summary>
        /// Sends all log messages to the trace stream.
        /// </summary>
        public class LogTrace : ILog
        {
	        /// <summary>
	        /// Writes a log message to the trace stream.  The appropriate line terminator
	        /// is provided.
	        /// </summary>
            public virtual void WriteLine(Object aObject)
            {
                System.Diagnostics.Trace.WriteLine(aObject);
            }

	        /// <summary>
	        /// Writes a log message to the trace stream.
	        /// </summary>
            public virtual void Write(Object aObject)
            {
                System.Diagnostics.Trace.Write(aObject);
            }
        }
    }
}