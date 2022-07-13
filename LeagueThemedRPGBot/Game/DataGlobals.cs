using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LeagueThemedRPGBot.Game
{
    public static class DataGlobals
    {
        public static async Task SaveFileDataAsync<T>(string location, T data) 
        {
            using var saveData = File.Create(location);
            await JsonSerializer.SerializeAsync(saveData, data, SerializationOptions);
            await saveData.DisposeAsync();
        }

        public static void SaveFileData<T>(string location, T data)
        {
            using var saveData = File.Create(location);
            JsonSerializer.Serialize(saveData, data, SerializationOptions);
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
            return JsonSerializer.Deserialize<T>(fileData, SerializationOptions);
        }

        public static Dictionary<string, T> LoadGameDataFromDirectory<T>(string directoryLocation) where T : new()
        {
            if (!Directory.Exists(directoryLocation))
            {
                Directory.CreateDirectory(directoryLocation);
                return new Dictionary<string, T>();
            }

            var files = Directory.GetFiles(directoryLocation);
            var subdirs = Directory.GetDirectories(directoryLocation);
            var retval = new Dictionary<string, T>();

            foreach (var file in files)
            {
                using var data = File.OpenRead(file);
                var ds = JsonSerializer.Deserialize<T>(data, SerializationOptions);
                var key = ds.ToString().RemoveWhitespace().ToLowerInvariant();
                if (retval.ContainsKey(key)) key += "q";
                retval.Add(key, ds);
            }

            foreach (var dir in subdirs)
            {
                foreach (var file in Directory.GetFiles(dir))
                {
                    using var data = File.OpenRead(file);
                    var ds = JsonSerializer.Deserialize<T>(data, SerializationOptions);
                    var key = ds.ToString().RemoveWhitespace().ToLowerInvariant();
                    if (retval.ContainsKey(key)) key += "q";
                    retval.Add(key, ds);
                }
            }

            return retval;
        }

        private static readonly Regex whitespace = new(@"\s+");

        public static string RemoveWhitespace(this string s)
            => whitespace.Replace(s, string.Empty);

        public static JsonSerializerOptions SerializationOptions { get; } = new()
        {
            // May be necessary to set to 'Preserve' if we encounter later issues with serialization
            // NOTE: SWAPPING THE REFERENCEHANDLER CONTEXT WILL BREAK CURRENTLY SAVED PLAYERDATA
            ReferenceHandler = ReferenceHandler.IgnoreCycles, 
            AllowTrailingCommas = true,
            IncludeFields = false,
            WriteIndented = true
        };

    }
}
