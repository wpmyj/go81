using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Helpers
{
    public class RandomString
    {
        private static readonly Random rd = new Random();
        public static string Generate(int minSize = 6, int optionalSize = 0)
        {
            var sz = rd.Next(minSize, 1 + minSize + optionalSize);
            const string str = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = string.Empty;
            for (var i = 0; i < sz; ++i)
            {
                result += str[rd.Next(str.Length)];
            }
            return result;
        }
        public static int ClickStartNum(int min = 30, int max = 50)
        {
            return rd.Next(min, max);
        }
    }

    public static class TextHelper
    {
        public static bool IsNarrow(this char c)
        {
            return
                ('\u0000' <= c && c <= '\u00FF')
                || ('\uFF61' <= c && c <= '\uFF9F')
                || ('\uFFE8' <= c && c <= '\uFFEE')
                ;
        }
    }
}