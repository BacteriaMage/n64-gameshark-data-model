// bacteriamage.wordpress.com

using System.Collections.Generic;

namespace BacteriaMage.N64.GameShark
{
    /// <summary>
    /// Read the full list of games and cheats from a GameShark ROM image.
    /// </summary>
    class RomReader : RomBase
    {
        public static List<Game> FromFile(string path)
        {
            RomReader reader = new RomReader();
            reader.ReadRomFromFile(path);
            return reader.ReadGames();
        }

        private RomReader()
        {
        }

        private List<Game> ReadGames()
        {
            List<Game> games = new List<Game>();

            SeekGamesList();

            int gamesCount = Reader.ReadSInt32();

            for (int gameIndex = 0; gameIndex < gamesCount; gameIndex++)
            {
                games.Add(ReadGame());
            }

            return games;
        }

        private Game ReadGame()
        {
            return GameDecoder.FromReader(Reader);
        }
    }
}
