using System;
using System.Collections.Generic;
using System.Text;

namespace TelnetCalc.Common
{
    public static class Extensions
    {
        public static byte[] GetBytes(this string n)
        {
            return Encoding.ASCII.GetBytes(n + Environment.NewLine);
        }

        public static string GetString(this byte[] b)
        {
            return Encoding.ASCII.GetString(b);
        }

    }
}
