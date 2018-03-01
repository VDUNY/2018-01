using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LLE
{
    namespace Util
    {
        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
        public class MinMaxAttribute : Attribute
        {
	        protected Decimal m_min;
	        protected Decimal m_max;

	        public MinMaxAttribute(sbyte aMin, sbyte aMax)
            {
	            m_min = Convert.ToDecimal(aMin);
                m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(short aMin, short aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(int aMin, int aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(long aMin, long aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(byte aMin, byte aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(ushort aMin, ushort aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(uint aMin, uint aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(ulong aMin, ulong aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(double aMin, double aMax)
            {
                m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        public MinMaxAttribute(float aMin, float aMax)
            {
	            m_min = Convert.ToDecimal(aMin);
	            m_max = Convert.ToDecimal(aMax);
	        }

	        static public Decimal Min(Object aInstance, string aMemberName, Decimal aDefault)
	        {
		        MemberInfo memberInfo = aInstance.GetType().GetMember(aMemberName)[0];
		        Object[] attributes = memberInfo.GetCustomAttributes(typeof(MinMaxAttribute), true);
		        if (attributes.GetLength(0) >= 1) {
			        return ((MinMaxAttribute)attributes[0]).m_min;
		        }
		        else {
			        return aDefault;
		        }
	        }

	        static public Decimal Max(Object aInstance, string aMemberName, Decimal aDefault)
	        {
		        MemberInfo memberInfo = aInstance.GetType().GetMember(aMemberName)[0];
		        Object[] attributes = memberInfo.GetCustomAttributes(typeof(MinMaxAttribute), true);
		        if (attributes.GetLength(0) >= 1) {
			        return ((MinMaxAttribute)attributes[0]).m_max;
		        }
		        else {
			        return aDefault;
		        }
	        }
        }
    }
}