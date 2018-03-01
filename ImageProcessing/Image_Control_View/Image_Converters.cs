using System;
using System.Windows.Data;  // ValueConversion attribute, IValueConverter interface; req ref -> PresentationFramework
using System.Windows.Markup;    // MarkupExtension - simplifies xaml assignment of converter class; req ref -> System.Xaml
using System.Windows.Media; // Brushes

using Image_Control_ViewModel;

// Provides a way for xaml to bind properties of different types, e.g. a background brush to the width of a control.
// The ValueConversion attribute indicates to development tools the types of data involved.
// Deriving from MarkupExtension simplifies the syntax for assigning a converter to a control property
namespace Image_Converters
{

    /// <summary>
    /// Convert a radiobutton IsChecked to bool
    /// </summary>
    /// <remarks>
    /// Called during InitializeComponent
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(ImageControlViewModel.ScalingChoice))]
    public class ConvertBoolToScaling : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// prevents development error re ctor with 0 params.
        /// </summary>
        public ConvertBoolToScaling()   // prevents development tool error re ctor with 0 params.
        {
        }

        /// <summary>
        /// Updating target (view) with value from the source (viewmodel)
        /// </summary>
        /// <param name="value">Dependency Property 'Scaling' value</param>
        /// <param name="targetType">Boolean</param>
        /// <param name="parameter">xaml ConverterParameter: AUTO_SCALING, STATIC_SCALING or EXPANDED_SCALING</param>
        /// <param name="culture"></param>
        /// <returns>true - marks the rdo btn as the selected scaling. false - does not mark the rdo btn as the selected scaling. </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString().Equals(parameter)) return true;    // e.g. 
            else return false;
        }

        /// <summary>
        /// Updating source (viewmodel) with value from the target (view)
        /// Requires two-way binding
        /// </summary>
        /// <param name="value">true if selected radio button. false if radio button not selected.</param>
        /// <param name="targetType">ImageViewModel.ScalingChoice</param>
        /// <param name="parameter">xaml ConverterParameter: AUTO_SCALING, STATIC_SCALING or EXPANDED_SCALING</param>
        /// <param name="culture"></param>
        /// <returns>"AUTO_SCALING, "STATIC_SCALING" or "EXPANDED_SCALING"</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bVal = (bool)value;
            string strParam = parameter.ToString();
            string rtnVal = "";
            if ((bVal == true) && (parameter.Equals("NO_SCALING")))
            {
                rtnVal = "NO_SCALING";
            }
            if ((bVal == true) && (parameter.Equals("STATIC_SCALING")))
            {
                rtnVal = "STATIC_SCALING";
            }
            if ((bVal == true) && (parameter.Equals("EXPANDED_SCALING")))
            {
                rtnVal = "EXPANDED_SCALING";
            }
            if ((bVal == true) && (parameter.Equals("AUTO_SCALING")))
            {
                rtnVal = "AUTO_SCALING";
            }
            return rtnVal;
        }

        /// <summary>
        /// a markup extension only sets the value of a property once, when an element is being loaded.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        /// <remarks>Called during InitializeComponent</remarks>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }   // class ConvertBoolToScaling

}   // Image_Converters

