using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Player.Data = await Data.LoadData<Dictionary<ulong, Player>>(Data.PlayerDataLocation);
            var bot = new BotClient();
            await bot.RunBotAsync();
        }
    }
}
