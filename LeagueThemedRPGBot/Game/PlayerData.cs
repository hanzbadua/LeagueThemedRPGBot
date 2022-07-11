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

        public bool IsInitedByID(ulong Id)
        {
            return Data.ContainsKey(Id);
        }

        public void Load()
        {
            Data = DataFunctions.LoadFileData<Dictionary<ulong, Player>>(PlayerDataLocation);
        }

        public async Task SaveAsync()
        {
            await DataFunctions.SaveFileDataAsync(PlayerDataLocation, Data);
        }

        public Dictionary<ulong, Player> Data { get; private set; } = new();
    }
}
