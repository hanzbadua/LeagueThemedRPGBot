using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LeagueThemedRPGBot.Game
{
    public static class DataFunctions
    {
        public static async Task SaveFileDataAsync<T>(string location, T data) 
        {
            using var saveData = File.Create(location);
            await JsonSerializer.SerializeAsync(saveData, data, opt);
            await saveData.DisposeAsync();
        }

        public static void SaveFileData<T>(string location, T data)
        {
            using var saveData = File.Create(location);
            JsonSerializer.Serialize(saveData, data, opt);
        }

        public static T LoadFileData<T>(string location) where T : new()
        {
            // if data file doesn't exist we just assume the bot is starting fresh
            // as im not stupid
            if (!File.Exists(location)) {
                var newData = new T();
                SaveFileData(location, newData);
                return newData;
            }

            using var fileData = File.OpenRead(location);
            return JsonSerializer.Deserialize<T>(fileData, opt);
        }

        public static Dictionary<string, T> LoadGameDataFromDirectory<T>(string directoryLocation) where T : new()
        {
            if (!Directory.Exists(directoryLocation))
            {
                Directory.CreateDirectory(directoryLocation);
                return new Dictionary<string, T>();
            }

            var files = Directory.GetFiles(directoryLocation);
            var retval = new Dictionary<string, T>();

            foreach (var file in files)
            {
                using var data = File.OpenRead(file);
                var ds = JsonSerializer.Deserialize<T>(data, opt);
                retval.Add(ds.ToString().RemoveWhitespace().ToLowerInvariant(), ds);
            }

            return retval;
        }

        

        private static readonly Regex whitespace = new(@"\s+");

        public static Random Rng { get; } = new();

        public static string RemoveWhitespace(this string s)
            => whitespace.Replace(s, string.Empty);

        private static readonly JsonSerializerOptions opt = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            AllowTrailingCommas = true
        };

    }
}
