using System;
using System.Windows.Data;  // ValueConversion attribute, IValueConverter interface; req ref -> PresentationFramework
using System.Windows.Markup;    // MarkupExtension - simplifies xaml assignment of converter class; req ref -> System.Xaml
using System.Windows.Media; // Brushes

// Provides a way for xaml to bind properties of different types, e.g. a background brush to the width of a control.
// The ValueConversion attribute indicates to development tools the types of data involved.
// Deriving from MarkupExtension simplifies the syntax for assigning a converter to a control property
namespace Converters
{


    [ValueConversion(typeof(bool), typeof(Color))]
    class ConvertBoolToBackground : MarkupExtension, IValueConverter
    {
        public ConvertBoolToBackground() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            string color = "";
            if (val.Equals("True")) { color = Colors.Green.ToString(); }
            else if (val.Equals("False")) { color = default(Color).ToString(); }
            else color = Colors.Gray.ToString();
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    /// <summary>
    /// Format an int for display in text-type properties
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    class ConvertIntToString : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertIntToString() { }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">25 or 30</param>
        /// <param name="targetType">System.String</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"25" or "30"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int conv = 0;
            conv = System.Convert.ToInt32(value);
            string str = "";
            if (conv < 0) { str = "----"; }
            else { str = string.Format("{000}", conv); }
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"45" or "500" or ""</param>
        /// <param name="targetType">System.Int32</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>45 or 500</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value.ToString().Equals("") || value.ToString().Equals("-"))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    int val = System.Convert.ToInt32(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return -9999;
            }
            catch (Exception ex)
            {   // if we get here, may be using OneWayToSource in the xaml binding
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   // ConvertIntToString : MarkupExtension, IValueConverter

    /// <summary>
    /// Format an int for display in text-type properties. If the string is empty, force value to be 0.
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    class ConvertInt0ForEmptyString : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertInt0ForEmptyString()
        {

        }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">25 or 30</param>
        /// <param name="targetType">System.String</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"25" or "30"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int conv = 0;
            conv = System.Convert.ToInt32(value);
            string str = "";
            if (conv > 0) { str = string.Format("{000}", conv); }
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"45" or "500" or ""</param>
        /// <param name="targetType">System.Int32</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>45 or 500</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value.ToString().Equals("-"))
                {
                    return Binding.DoNothing;
                }
                else if (value.ToString().Equals(""))
                {
                    return 0;
                }
                else
                {
                    int val = System.Convert.ToInt32(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //    class ConvertInt0ForEmptyString : IValueConverter

    /// <summary>
    /// True becomes false; false becomes true.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]     // Init new instance of ValueConversionAttribute class (source, target) = (viewmodel, view)
    public class ConvertNegateBoolean : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public ConvertNegateBoolean()   // prevents development tool error re ctor with 0 params.
        {

        }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value"> true or false</param>
        /// <param name="targetType">System.Boolean</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool temp = !System.Convert.ToBoolean(value);
            return temp;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">true or false</param>
        /// <param name="targetType">System.Boolean</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool temp = !System.Convert.ToBoolean(value);
            return temp;
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //     public class ConvertNegateBoolean : MarkupExtension, IValueConverter

    /// <summary>
    /// Format a float to 0 decimal positions for display in text-type properties
    /// </summary>
    [ValueConversion(typeof(Single), typeof(string))]
    class ConvertSingleToPrecision0 : MarkupExtension, IValueConverter
    {

                /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertSingleToPrecision0() { }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">25</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"25"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float conv = 0.0F;
            conv = System.Convert.ToSingle(value);
            string str = "";
            str = string.Format("{0:0}", conv);
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"25"</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>25</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    Single val = System.Convert.ToSingle(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //     class ConvertSingleToPrecision0 : MarkupExtension, IValueConverter

    /// <summary>
    /// Turn 2 into 2.0. Turn 2.32783 into 2.3
    /// </summary>
    [ValueConversion(typeof(Single), typeof(string))]  // Init new instance of ValueConversionAttribute class (source, target) = (viewmodel, view)
    class ConvertSingleToPrecision1 : MarkupExtension, IValueConverter
    {

        /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertSingleToPrecision1() { }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">19.224 or 19.2</param>
        /// <param name="targetType">System.String</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"19.2" or "19.2"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float conv = 0.0F;
            string str = "";
            try
            {
                conv = System.Convert.ToSingle(value);
                if (conv < 0.0f) str = "---";
                else str = string.Format("{0:0.0}", conv);
            }
            catch (FormatException ex)
            {
                str = "----";
                Console.WriteLine(ex);
            }
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"19.2" or "19.225"</param>
        /// <param name="targetType">Single</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>19.2 or 19.2</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    Single val = System.Convert.ToSingle(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //     class ConvertSingleToPrecision1 : MarkupExtension, IValueConverter

    /// <summary>
    /// Turn 2.3 into 2.300. Turn 2.32783 into 2.328
    /// </summary>
    [ValueConversion(typeof(Single), typeof(string))]  // Init new instance of ValueConversionAttribute class (source, target) = (viewmodel, view)
    class ConvertSingleToPrecision3 : MarkupExtension, IValueConverter
    {

        /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertSingleToPrecision3() { }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">19.2249 or 19.2</param>
        /// <param name="targetType">System.String</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"19.225" or "19.200"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float conv = 0.0F;
            string str = "";
            try
            {
                conv = System.Convert.ToSingle(value);
                str = string.Format("{0:0.000}", conv);
            }
            catch (FormatException ex)
            {
                str = "----";
                Console.WriteLine(ex);
            }
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"19.2" or "19.2258"</param>
        /// <param name="targetType">Single</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>19.2 or 19.236</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    Single val = System.Convert.ToSingle(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //     class ConvertSingleToPrecision3: MarkupExtension, IValueConverter

    /// <summary>
    /// Turn 2.3 into 2.3000. Turn 2.32789 into 2.3279
    /// </summary>
    [ValueConversion(typeof(Single), typeof(string))]  // Init new instance of ValueConversionAttribute class (source, target) = (viewmodel, view)
    class ConvertSingleToPrecision4 : MarkupExtension, IValueConverter
    {

        /// <summary>
        /// prevents development tool error re ctor with 0 params
        /// </summary>
        public ConvertSingleToPrecision4() { }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">19.22493 or 19.2</param>
        /// <param name="targetType">System.String</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>"19.2249" or "19.2000"</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float conv = 0.0F;
            string str = "";
            try
            {
                conv = System.Convert.ToSingle(value);
                str = string.Format("{0:0.0000}", conv);
            }
            catch (FormatException ex)
            {
                str = "----";
                Console.WriteLine(ex);
            }
            return str;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">"19.2" or "19.2258"</param>
        /// <param name="targetType">Single</param>
        /// <param name="parameter">In xaml, e.g. ConverterParameter=50</param>
        /// <param name="culture"></param>
        /// <returns>19.2 or 19.2258</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (String.IsNullOrEmpty(value.ToString()))
                {
                    return Binding.DoNothing;
                }
                else
                {
                    Single val = System.Convert.ToSingle(value);
                    return val;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Binding.DoNothing;
            }
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   //     class ConvertSingleToPrecision4: MarkupExtension, IValueConverter


}   // Converters
