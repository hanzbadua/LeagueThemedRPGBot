using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeagueThemedRPGBot.Game
{
    public static class Data
    {
        public const string PlayerDataLocation = "playerData.json";
        public const string WeaponDataDirectory = "weapons";

        public static async Task SaveFileData<T>(string location, T data) {
            using var saveData = File.Create(location);
            await JsonSerializer.SerializeAsync(saveData, data, opt);
            await saveData.DisposeAsync();
        }

        public static async Task<T> LoadFileData<T>(string location) where T : new()
        {
            // if data file doesn't exist we just assume the bot is starting fresh
            // as im not stupid
            if (!File.Exists(location)) {
                var newData = new T();
                await SaveFileData(location, newData);
                return newData;
            }

            using var fileData = File.OpenRead(location);
            return await JsonSerializer.DeserializeAsync<T>(fileData, opt);
        }

        public static async Task<Dictionary<string, T>> LoadGameDataFromDirectory<T>(string directoryLocation) where T : new()
        {
            if (!Directory.Exists(WeaponDataDirectory))
            {
                Directory.CreateDirectory(WeaponDataDirectory);
                return new Dictionary<string, T>();
            }

            var files = Directory.GetFiles(WeaponDataDirectory);
            var retval = new Dictionary<string, T>();

            foreach (var file in files)
            {
                using var data = File.OpenRead(file);
                var ds = await JsonSerializer.DeserializeAsync<T>(data, opt);
                retval.Add(ds.ToString(), ds);
            }

            return retval;
        }

        public static Item GetWeaponByName(string name)
        {
            if (Item.Weapons.ContainsKey(name)) return Item.Weapons[name];

            return null;
        }

        private static readonly JsonSerializerOptions opt = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            AllowTrailingCommas = true
        };
    }
}
