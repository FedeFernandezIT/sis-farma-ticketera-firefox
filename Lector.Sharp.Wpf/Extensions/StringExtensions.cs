using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Extensions
{
    public static class StringExtensions
    {
        public static string Strip(this string str, string pattern, string fill = "")
        {
            return Regex.Replace(str.Trim(), pattern, fill);
        }
    }
}
