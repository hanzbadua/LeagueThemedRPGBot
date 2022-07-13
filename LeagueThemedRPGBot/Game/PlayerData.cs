using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueThemedRPGBot.Game
{
    public class PlayerData 
    {
        private const string PlayerDataLocation = "playerData.json";

        public PlayerData()
        {
            Data = DataGlobals.LoadFileData<Dictionary<ulong, Player>>(PlayerDataLocation);
        }

        public bool IsInitedByID(ulong Id)
        {
            return Data.ContainsKey(Id);
        }

        public async Task SaveAsync()
        {
            await DataGlobals.SaveFileDataAsync(PlayerDataLocation, Data);
        }

        public Dictionary<ulong, Player> Data { get; private set; } = new();
    }
}
