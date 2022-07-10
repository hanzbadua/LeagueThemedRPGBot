using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    public static class Data
    {
        public const string PlayerDataLocation = "playerData.json";
        public static async Task SaveData<T>(string location, T data) {
            using var saveData = File.Create(location);
            await JsonSerializer.SerializeAsync(saveData, data, opt);
            await saveData.DisposeAsync();
        }

        public static async Task<T> LoadData<T>(string location) where T : new()
        {
            // if data file doesn't exist we just assume the bot is starting fresh
            // as im not stupid
            if (!File.Exists(location)) {
                var newData = new T();
                await SaveData(location, newData);
                return newData;
            }

            using var fileData = File.OpenRead(location);
            return await JsonSerializer.DeserializeAsync<T>(fileData, opt);
        }

        private static readonly JsonSerializerOptions opt = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
    }
}
