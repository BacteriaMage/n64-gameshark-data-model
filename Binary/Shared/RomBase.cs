using System;
using System.Runtime.CompilerServices;

namespace BacteriaMage.N64.GameShark
{
    /// <summary>
    /// Base class for readers and writers of Game Shark ROM files.
    /// </summary>
    abstract class RomBase
    {
        protected BinaryReader Reader { get; private set; }
        protected BinaryWriter Writer { get; private set; }

        public RomVersion FirmwareVersion
        {
            get
            {
                SeekBuildTimestamp();
                return new RomVersion(Reader.ReadPrintableCString(15));
            }
        }

        protected RomBase() : this(new BinaryReader())
        {
        }

        protected RomBase(BinaryReader reader)
        {
            Reader = reader;
            Writer = new BinaryWriter(reader.Buffer ?? Array.Empty<byte>());
        }

        protected void ReadRomFromFile(string path)
        {
            BinaryReader rom = BinaryReader.FromFile(path);
            Reader = rom;

            if (!ValidateRom(rom))
            {
                throw new Exception("Not a valid N64 GameShark Pro ROM");
            }

            Writer = new BinaryWriter(rom.Buffer ?? Array.Empty<byte>());
        }

        private static bool Is(string a, string b)
        {
            return string.Compare(a, b, StringComparison.InvariantCulture) == 0;
        }

        private bool ValidateRom(BinaryReader rom)
        {
            int? bufferLength = rom.Buffer?.Length;
            if (bufferLength != 0x00040000)
            {
                // always 256 KiB
                Console.Error.WriteLine($"ERROR: Invalid GS ROM file size: 0x{bufferLength:X8}. All GameShark ROMs are exactly 256 KiB.");
                return false;
            }

            UInt32 gsRomMagicNumber = rom.Seek(0x00000000).ReadUInt32();
            if (gsRomMagicNumber != 0x80371240)
            {
                // N64 ROM Magic Number
                Console.Error.WriteLine($"ERROR: Invalid GS ROM magic number: 0x{gsRomMagicNumber:X8}. Expected 0x80371240.");
                return false;
            }

            string romHeader = rom.Seek(0x00000020).ReadPrintableCString(13);
            const string v1or2Header = "(C) DATEL D&D";
            const string v3ProHeader = "(C) MUSHROOM ";
            bool isV1or2 = Is(romHeader, v1or2Header);
            bool isV3Pro = Is(romHeader, v3ProHeader);
            if (!isV1or2 && !isV3Pro)
            {
                // ROM Header Name
                Console.Error.WriteLine($"ERROR: Invalid GS ROM header name: '{romHeader}'. Expected '{v1or2Header}' or '{v3ProHeader}'.");
                return false;
            }

            // Magic Number for user settings block. Only present in GS ROMs v3.0 and higher.
            if (FirmwareVersion.Number >= 3)
            {
                UInt16 userSettingsMagicNumber = rom.Seek(0x0002FB00).ReadUInt16();
                if (userSettingsMagicNumber != 0x4754 &&
                    userSettingsMagicNumber != 0xffff)
                {
                    Console.Error.WriteLine($"ERROR: Invalid magic number for user settings block: 0x{userSettingsMagicNumber:X4}. Expected 0x4754 or 0xffff.");
                    return false;
                }
            }

            return true;
        }

        protected void WriteRomToFile(string path)
        {
            Writer.WriteToFile(path);
        }

        protected void SeekGamesList()
        {
            Seek(FirmwareVersion.Number >= 3 ? 0x00030000 : 0x0002E000);
        }

        protected void SeekStart()
        {
            Seek(0x00000000);
        }

        protected void SeekBuildTimestamp()
        {
            Seek(0x00000030);
        }

        protected RomBase Seek(int address)
        {
            Reader.Seek(address);
            Writer.Seek(address);
            return this;
        }
    }
}
