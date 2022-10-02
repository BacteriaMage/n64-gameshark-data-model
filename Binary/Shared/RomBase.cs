using System;

namespace BacteriaMage.N64.GameShark
{
    /// <summary>
    /// Base class for readers and writers of Game Shark ROM files.
    /// </summary>
    abstract class RomBase
    {
        protected BinaryReader Reader { get; private set; }
        protected BinaryWriter Writer { get; private set; }

        protected RomBase()
        {
        }

        protected RomBase(BinaryReader reader)
        {
            Reader = reader;
            Writer = new BinaryWriter(reader.Buffer);
        }

        protected void ReadRomFromFile(string path)
        {
            BinaryReader rom = BinaryReader.FromFile(path);

            if (!ValidateRom(rom))
            {
                throw new Exception("Not a valid N64 GameShark Pro ROM");
            }

            Reader = rom;
            Writer = new BinaryWriter(rom.Buffer);
        }

        private bool ValidateRom(BinaryReader rom)
        {
            if (rom?.Buffer == null || rom.Buffer.Length != 0x00040000)
            {
                // always 256 KB
                return false;
            }
            if (rom.Seek(0x00000000).ReadUInt32() != 0x80371240)
            {
                // N64 ROM Magic Number
                return false;
            }
            if (string.Compare(rom.Seek(0x00000020).ReadCString(13), "(C) MUSHROOM ") != 0)
            {
                // ROM Header Name
                return false;
            }
            if (rom.Seek(0x0002FB00).ReadUInt16() != 0x4754 && rom.Seek(0x0002FB00).ReadUInt16() != 0xffff)
            {
                // User settings block Magic Number
                return false;
            }

            return true;
        }

        protected void WriteRomToFile(string path)
        {
            Writer.WriteToFile(path);
        }

        protected void SeekGamesList()
        {
            Seek(0x00030000);
        }

        protected void SeekStart()
        {
            Seek(0x00000000);
        }

        protected void Seek(int address)
        {
            Reader.Seek(address);
            Writer.Seek(address);
        }
    }
}
