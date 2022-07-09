using System.Text.Json;

namespace LeagueThemedRPGBot.Game
{
    public static class DataHandler
    {
        public const string PlayerDataLocation = "playerData.json";
        public static async Task SaveData<T>(string location, T data) {
            using var saveData = File.Create(location);
            await JsonSerializer.SerializeAsync(saveData, data);
            await saveData.DisposeAsync();
        }

        public static async Task<T> LoadData<T>(string location) where T: new()
        {
            // if data file doesn't exist we just assume the bot is starting fresh
            // as im not stupid
            if (!File.Exists(location)) {
                var newData = new T();
                await File.CreateText(location).DisposeAsync();
                await SaveData(location, newData);
            }

            using var fileData = File.OpenRead(location);
            return await JsonSerializer.DeserializeAsync<T>(fileData);
        }
    }
}
