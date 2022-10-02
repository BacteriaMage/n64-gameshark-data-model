// bacteriamage.wordpress.com

using System;
using System.IO;
using System.Text;

namespace BacteriaMage.N64.GameShark
{
    /// <summary>
    /// Helper class for reading (big-endian) integers and c-style strings from byte buffers.
    /// </summary>
    class BinaryReader
    {
        public byte[] Buffer { get; set; }
        public int Position { get; set; }
        public int BytesRead { get; set; }

        public int Length => Buffer.Length;
        public bool EndReached => Position >= Length;

        public BinaryReader(byte[] buffer)
            : base()
        {
            Buffer = buffer;
        }

        public BinaryReader()
        {
        }

        public static BinaryReader FromFile(string path)
        {
            return new BinaryReader(File.ReadAllBytes(path));
        }

        public BinaryReader Seek(int address)
        {
            Position = address;
            return this;
        }

        public int ReadUByte()
        {
            if (Buffer == null || Position == Buffer.Length)
            {
                throw new IndexOutOfRangeException("End of buffer reached");
            }
            if (Position < 0 || Position > Buffer.Length)
            {
                throw new IndexOutOfRangeException("Invalid position");
            }

            byte b = Buffer[Position++];

            BytesRead++;

            return b;
        }

        public int ReadSByte()
        {
            return (sbyte)ReadUByte();
        }

        public int ReadUInt16()
        {
            return (ReadUByte() << 8) + ReadUByte();
        }

        public int ReadSInt16()
        {
            return (short)ReadUInt16();
        }

        public uint ReadUInt32()
        {
            uint high = (uint)ReadUInt16();
            uint low = (uint)ReadUInt16();

            return (high << 16) + low;
        }

        public int ReadSInt32()
        {
            return (int)ReadUInt32();
        }

        public string ReadCString()
        {
            return ReadCString(0);
        }

        public string ReadCString(int max)
        {
            StringBuilder builder = new StringBuilder();

            while (NextCharacter(out string character) && (max < 1 || builder.Length < max))
            {
                builder.Append(character);
            }

            return builder.ToString();
        }

        private bool NextCharacter(out string character)
        {
            if (NextByte(out byte b))
            {
                character = ByteToCharacter(b);
                return true;
            }
            else
            {
                character = "";
                return false;
            }
        }

        private string ByteToCharacter(byte b)
        {
            if (b > 127)
            {
                return string.Concat('`', b.ToString("X2"), '`');
            }
            else
            {
                return Encoding.ASCII.GetString(new byte[] { b });
            }
        }

        private bool NextByte(out byte b)
        {
            b = (byte)ReadUByte();
            return b != 0;
        }
    }
}
