using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TelnetCalc.Common;

namespace TelnetCalc.Server
{
    public class SocketReader
    {

        private readonly Socket _socket;

        public SocketReader(Socket socket)
        {
            _socket = socket;
        }

        public void Read(Action<int> valueReadCallback)
        {
            var buffer = new byte[MainSettings.ChunkSize];
            int bytesRead;

            // Read data in blocks of known chunk size.
            while ((bytesRead = TryReadChunk(buffer)) == MainSettings.ChunkSize)
            {
                // Convert to 32-bit int. If not valid number, we're done.
                if (!TryConvertToInt32(buffer, out int value))
                {
                    break;
                }

                // When we get a good value, invoke callback so value can be processed.
                valueReadCallback?.Invoke(value);
            }
        }

        private int TryReadChunk(byte[] buffer)
        {
            // We can't be sure that we're receiving the full 9+ bytes at the same
            // time, so loop to read data until we fill the buffer. Under normal
            // circumstances, we should, in which case there's just a single
            // Receive call here.
            int bytesRead;
            int bufferOffset = 0;

            while (bufferOffset < buffer.Length)
            {
                bytesRead = _socket.Receive(buffer, bufferOffset, buffer.Length - bufferOffset, SocketFlags.None);
                if (bytesRead == 0)
                {
                    break;
                }
                bufferOffset += bytesRead;
            }
            return bufferOffset;
        }

        private bool TryConvertToInt32(byte[] buffer, out int value)
        {
            value = 0;

            // Make sure chunk correctly terminates with new line sequence.
            // Loop here to support Windows with two-byte sequence.
            for (var i = 0; i < MainSettings.NewLineSize; i++)
            {
                if (buffer[MainSettings.ValueSize + i] != MainSettings.NewLineSequence[i])
                {
                    return false;
                }
            }

            // Read through first 9 bytes and look for numeric digit. Use
            // the proper multiplier for its place and construct the numeric value.
            // If we find a non-numeric char, we short-circuit and return false.
            byte b;
            int place;

            for (var i = 0; i < MainSettings.ValueSize; i++)
            {
                b = buffer[i];

                if (b < 48 || b > 57)
                {
                    return false;
                }

                place = (int)Math.Pow(10, MainSettings.ValueSize - i - 1);
                value += ((b - 48) * place);
            }

            return true;
        }

    }
}
