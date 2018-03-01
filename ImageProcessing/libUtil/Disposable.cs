using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public class Disposable : IDisposable
        {
            private bool m_disposed = false;

            ~Disposable()
            {
                if (!m_disposed)
                {
                    // dispose only unmanaged resources
                    Dispose(false);
                }
            }

	        /// <summary>
	        /// Destructor.
	        /// </summary>
	        public void Dispose()
            {
                if (!IsDisposed())
                {
                    // dispose managed and unmanaged resources
                    Dispose(true);
                }
            }

            protected virtual void Dispose(bool aDisposeManaged)
            {
                // mark as disposed
                m_disposed = true;

                if (aDisposeManaged)
                {
                    // don't dispose again
                    GC.SuppressFinalize(this);
                }
            }

            public bool IsDisposed()
            {
                return m_disposed;
            }
        }
    }
}