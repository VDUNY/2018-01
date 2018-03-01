using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        /// <summary>
        /// LLEException is the base exception for all exceptions thrown by LLE objects.  LLEExceptions
        /// are logged to the log facility.
        /// </summary>
        public class LLEException : Exception
        {
            private string m_location = "";

	        /// <summary>
	        /// Constructor.  Logs the exception to the log facility.
	        /// </summary>
            public LLEException(LogLevel aLevel, string aMessage, string aLocation) :
                base(aMessage)
            {
                m_location = aLocation;

                // write the exception to the log facility
                Log.Instance.WriteLine(aLevel, this);
            }

            public LLEException(LogLevel aLevel, string aMessage) :
                base(aMessage)
            {
                // write the exception to the log facility
                Log.Instance.WriteLine(aLevel, this);
            }

            public string Location
            {
                get
                {
                    return m_location;
                }
            }

            public override string ToString()
            {
                if (Location != "")
                {
                    return Message + " (" + Location + ")";
                }
                else
                {
                    return Message;
                }
            }
        }
    }
}