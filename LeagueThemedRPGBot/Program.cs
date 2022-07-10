using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    public class Program
    {
        private static async Task Main()
        {
            Item.Weapons = await Data.LoadGameDataFromDirectory<Item>(Data.WeaponDataDirectory);
            Player.Data = await Data.LoadFileData<Dictionary<ulong, Player>>(Data.PlayerDataLocation);
            var bot = new BotClient();
            await bot.RunBotAsync();
        }
    }
}

// todo: implement full encounter