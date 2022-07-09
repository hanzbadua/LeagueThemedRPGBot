using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Globals.PlayerData = await DataHandler.LoadData<Dictionary<ulong, Player>>(DataHandler.PlayerDataLocation);
            var bot = new BotClient();
            await bot.RunBotAsync();
        }
    }
}
