using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace TelnetCalc.Common
{
    public static class Extensions
    {

        public static byte[] GetBytes(this string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        public static string GetString(this byte[] b)
        {
            return Encoding.ASCII.GetString(b);
        } 
    }
}
