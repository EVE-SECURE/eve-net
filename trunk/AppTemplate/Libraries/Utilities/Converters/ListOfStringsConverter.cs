using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace AppTemplate.Utilities
{
   [ValueConversion(typeof(List<string>), typeof(string))]
   public class ListOfStringsConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return string.Join(",", (value as List<string>).ToArray());
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return (value as string).Split(',');  
      }
   }
}
