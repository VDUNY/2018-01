using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public interface IOpenable
        {
            void open();
            void close();
            bool isOpen();
        }
    }
}