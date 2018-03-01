using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LLE
{
    namespace Util
    {
	    public class StringUnicode : Disposable
        {
	        private IntPtr m_ptr;

		    public StringUnicode(string aString)
            {
            	m_ptr = Marshal.StringToHGlobalUni(aString);
            }

            protected override void Dispose(bool aDisposeManaged)
            {
               	Marshal.FreeHGlobal(m_ptr);

                base.Dispose(aDisposeManaged);
            }

            unsafe public char* Ptr()
            {
                return (char*)m_ptr.ToPointer();
            }
	    };
    }
}