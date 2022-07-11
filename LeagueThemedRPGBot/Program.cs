using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    public class Program
    {
        private static async Task Main()
        {
            var bot = new BotClient();
            await bot.RunBotAsync();
        }
    }
}

// todo: implement full encounter