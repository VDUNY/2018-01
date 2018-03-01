using System;
using System.Collections.Generic;
using System.Text;

namespace LLE
{
    namespace Util
    {
        public class MathUtils
        {
            static public T MinMax<T>(T aValue, T aMin, T aMax) where T : IComparable<T>
            {
                T tmp = aMin.CompareTo(aValue) <= 0 ? aValue : aMin;
                return tmp.CompareTo(aMax) <= 0 ? tmp : aMax;
            }
        }
    }
}