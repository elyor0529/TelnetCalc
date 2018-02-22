using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Text;
using TelnetCalc.Core.Common;
using Xunit;

namespace TelnetCalc.Core.xUnit
{
    public class SocketTests
    {
        [Fact]
        public void TestReadSuccess()
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();

            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("003456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("023456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));

            reader.Read(value => values.Add(value));

            Assert.Equal(
                new int[] { 123456789, 23456789, 3456789 },
                values.ToArray());
        }

        [Fact]
        public void TestReadSuccessWithLatentReceipt()
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();

            StubReceive(socket, 5, SocketReader.ChunkSize - 5, GetBytes("6789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("12345", false));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));

            reader.Read(value => values.Add(value));

            Assert.Equal(
                new int[] { 123456789, 123456789 },
                values.ToArray());
        }

        [Fact]
        public void TestReadSuccessWithTerminateSequence()
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();
            var terminated = false;

            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("terminate"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));

            reader.Read(value => values.Add(value), () => terminated = true);

            Assert.Equal(
                new int[] { 123456789, 123456789 },
                values.ToArray());
            Assert.True(terminated);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("12")]
        [InlineData("123")]
        [InlineData("1234")]
        [InlineData("12345")]
        [InlineData("123456")]
        [InlineData("1234567")]
        [InlineData("12345678")]
        [InlineData("123456789")]
        [InlineData("123456789  ")]
        public void TestReadInvalidInputFirstLine(string line)
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();

            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes(line, false));
            reader.Read(value => values.Add(value));

            Assert.Empty(values);
        }

        [Fact]
        public void TestReadInvalidSubsequentLine()
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();

            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("12345678", false));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("003456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("023456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));

            reader.Read(value => values.Add(value));

            Assert.Equal(
                new int[] { 123456789, 23456789, 3456789 },
                values.ToArray());
        }

        [Fact]
        public void TestTerminations()
        {
            var socket = A.Fake<IConnectionProxy>();
            var reader = new SocketReader(socket);
            var values = new List<int>();

            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("12345678", false));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("003456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("023456789"));
            StubReceive(socket, 0, SocketReader.ChunkSize, GetBytes("123456789"));

            reader.Read(value => values.Add(value));

            Assert.Equal(
                new int[] { 123456789, 23456789, 3456789 },
                values.ToArray());
        }

        private static byte[] GetBytes(string str, bool newline = true)
        {
            return Encoding.ASCII.GetBytes(str + (newline ? Environment.NewLine : ""));
        }

        private static void StubReceive(IConnectionProxy socket, int offset, int size, byte[] bytes)
        {
            A.CallTo(() => socket.Receive(A<byte[]>.Ignored, offset, size))
                .Invokes((byte[] buffer, int offsetArg, int sizeArg) => { bytes.CopyTo(buffer, offset); })
                .Returns(bytes.Length)
                .Once();
        }
    }
}
