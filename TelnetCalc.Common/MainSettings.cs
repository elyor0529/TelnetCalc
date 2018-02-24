using System;
using System.Collections.Generic;
using System.Text;

namespace TelnetCalc.Common
{
    public struct MainSettings
    {
         
        public const int BufferSize = 1024;
          
        public const int BackLog = 100;

        public const int DataSize = 9;

        public const int MaxConnections = 100;
         
        public static byte[] NewLineSequence = Environment.NewLine.GetBytes();

        public static int NewLineSize = NewLineSequence.Length;

        public static int ChunkSize = DataSize + NewLineSize;
         
    }
}
