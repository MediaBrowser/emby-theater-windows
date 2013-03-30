
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BDInfo
{
    public class TSStreamBuffer
    {
        private MemoryStream Stream = new MemoryStream();
        private int SkipBits = 0;
        private byte[] Buffer;
        private int BufferLength = 0;
        public int TransferLength = 0;

        public TSStreamBuffer()
        {
            Buffer = new byte[4096];
            Stream = new MemoryStream(Buffer);
        }

        public long Length
        {
            get
            {
                return (long)BufferLength;
            }
        }

        public long Position
        {
            get
            {
                return Stream.Position;
            }
        }

        public void Add(
            byte[] buffer,
            int offset,
            int length)
        {
            TransferLength += length;

            if (BufferLength + length >= Buffer.Length)
            {
                length = Buffer.Length - BufferLength;
            }
            if (length > 0)
            {
                Array.Copy(buffer, offset, Buffer, BufferLength, length);
                BufferLength += length;
            }
        }

        public void Seek(
            long offset,
            SeekOrigin loc)
        {
            Stream.Seek(offset, loc);
        }

        public void Reset()
        {
            BufferLength = 0;
            TransferLength = 0;
        }

        public void BeginRead()
        {
            SkipBits = 0;
            Stream.Seek(0, SeekOrigin.Begin);
        }

        public void EndRead()
        {
        }

        public byte[] ReadBytes(int bytes)
        {
            if (Stream.Position + bytes >= BufferLength)
            {
                return null;
            }

            byte[] value = new byte[bytes];
            Stream.Read(value, 0, bytes);
            return value;
        }

        public byte ReadByte()
        {
            return (byte)Stream.ReadByte();
        }

        public int ReadBits(int bits)
        {
            long pos = Stream.Position;

            int shift = 24;
            int data = 0;
            for (int i = 0; i < 4; i++)
            {
                if (pos + i >= BufferLength) break;
                data += (Stream.ReadByte() << shift);
                shift -= 8;
            }
            BitVector32 vector = new BitVector32(data);

            int value = 0;
            for (int i = SkipBits; i < SkipBits + bits; i++)
            {
                value <<= 1;
                value += (vector[1 << (32 - i - 1)] ? 1 : 0);
            }

            SkipBits += bits;
            Stream.Seek(pos + (SkipBits >> 3), SeekOrigin.Begin);
            SkipBits = SkipBits % 8;

            return value;
        }
    }
}
