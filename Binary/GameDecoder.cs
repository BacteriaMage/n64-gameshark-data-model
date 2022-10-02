// bacteriamage.wordpress.com

using System;

namespace BacteriaMage.N64.GameShark
{
    /// <summary>
    /// Helper class to decode a binary encoded cheat list for one game.
    /// </summary>
    class GameDecoder
    {
        public BinaryReader Reader { get; private set; }

        public static Game FromReader(BinaryReader reader)
        {
            GameDecoder encoder = new GameDecoder(reader);
            return encoder.ReadGame();
        }

        public GameDecoder(BinaryReader reader)
        {
            Reader = reader;
        }

        public Game ReadGame()
        {
            Game game = Game.NewGame(ReadName());

            int cheats = Reader.ReadUByte();

            for (int cheat = 0; cheat < cheats; cheat++)
            {
                ReadCheat(game);
            }

            return game;
        }

        private void ReadCheat(Game game)
        {
            Cheat cheat = game.AddCheat(ReadName());

            int codes = Reader.ReadUByte();

            bool cheatOn = (codes & 0x80) > 0;

            codes &= 0x7F;

            cheat.Active = cheatOn;

            for (int code = 0; code < codes; code++)
            {
                ReadCode(cheat);
            }
        }

        private void ReadCode(Cheat cheat)
        {
            uint address = Reader.ReadUInt32();
            int value = Reader.ReadUInt16();

            cheat.AddCode(address, value);
        }

        private string ReadName()
        {
            string name = Reader.ReadCString(31);

            if (name.Length < 1 || name.Length > 30)
            {
                throw new Exception("Invalid game or cheat name");
            }

            name = name.Replace("`F6`", "Key");
            name = name.Replace("`F7`", "Have ");
            name = name.Replace("`F8`", "Lives");
            name = name.Replace("`F9`", "Energy");
            name = name.Replace("`FA`", "Health");
            name = name.Replace("`FB`", "Activate ");
            name = name.Replace("`FC`", "Unlimited ");
            name = name.Replace("`FD`", "Player ");
            name = name.Replace("`FE`", "Always ");
            name = name.Replace("`FF`", "Infinite ");

            return name;
        }
    }
}
