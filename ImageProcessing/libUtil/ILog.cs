using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public interface ILog
        {
	        /// <summary>
	        /// Send a log message.  The log facility will provide the appropriate line terminator.
	        /// </summary>
            void WriteLine(Object aObject);

	        /// <summary>
	        /// Send a log message.
	        /// </summary>
            void Write(Object aObject);
        }
    }
}