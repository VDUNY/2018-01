using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public class Types
        {
            static public bool IsTypeOf<T>(Object aObject)
            {
                try
                {
                    T tmp = (T)aObject;
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}