using System;
using System.Collections.Generic;
using System.Text;

namespace TelnetCalc.Core.Common
{
    /// <summary>
    /// A proxy for a socket connection.
    /// </summary>
    /// <remarks>
    /// This interface exists to make <see cref="SocketReader"/>
    /// testable.
    /// </remarks>
    public interface IConnectionProxy
    {
        int Receive(byte[] buffer, int offset, int size);
    }
}
