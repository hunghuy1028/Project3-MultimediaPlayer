using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project3_MultimediaPlayer
{
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var info = value as string;
            //var tokens = info.Split(new string[] { "." },
            //    StringSplitOptions.None);
            //return tokens[0];

            var index = info.LastIndexOf(".");
            var token = info.Substring(0, index);
            return token;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
