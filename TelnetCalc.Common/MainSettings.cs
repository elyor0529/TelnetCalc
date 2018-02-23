using System;
using System.Collections.Generic;
using System.Text;

namespace TelnetCalc.Common
{
    public struct MainSettings
    {

        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Data buffer for incoming data.
        public const int DataSize = 1024;

        /// BackLog size
        public const int BackLog = 100;

    }
}
