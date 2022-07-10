using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Item.Weapons = await Data.LoadGameDataFromDirectory<Item>(Data.WeaponDataDirectory);
            Player.Data = await Data.LoadFileData<Dictionary<ulong, Player>>(Data.PlayerDataLocation);
            var bot = new BotClient();
            await bot.RunBotAsync();
        }
    }
}

// todo: unequip implementation
// better getitembyname lookup implementation