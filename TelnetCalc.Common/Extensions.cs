using System;
using System.Collections.Generic;
using System.Text;

namespace TelnetCalc.Common
{
    public static class Extensions
    {
        public static byte[] ToBytes(this int n)
        {
            return Encoding.ASCII.GetBytes(n + Environment.NewLine);
        }

    }
}
