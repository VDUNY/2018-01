using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public class 
            SortedSet<T> : SortedDictionary<T, T>
        {
            public void Add(T aValue)
            {
                base[aValue] = aValue;
            }
        }
    }
}