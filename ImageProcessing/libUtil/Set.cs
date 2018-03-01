using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public class Set<T> : Dictionary<T, T>
        {
            public void Add(T aValue)
            {
                base.Add(aValue, aValue);
            }
        }
    }
}