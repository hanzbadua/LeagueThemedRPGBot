using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueThemedRPGBot.Game
{
    public static class Globals
    {
        public static Dictionary<ulong, Player> PlayerData { get; set; }

        public static bool PlayerIsAlreadyInitialized(ulong playerID)
        {
            return PlayerData.ContainsKey(playerID);
        }
    }
}
