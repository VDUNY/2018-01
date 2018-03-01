using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace LLE
{
    namespace Util
    {
        public class StringAnsi : Disposable
        {

            private IntPtr m_ptr;

            public StringAnsi(string aString)
            {
                m_ptr = Marshal.StringToHGlobalAnsi(aString);
            }

            protected override void Dispose(bool aDisposeManaged)
            {
                Marshal.FreeHGlobal(m_ptr);

                base.Dispose(aDisposeManaged);
            }

            unsafe public byte* Ptr()
            {
                return (byte*)m_ptr.ToPointer();
            }
        };
    }
}