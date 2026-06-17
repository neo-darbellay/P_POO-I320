using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShootMeUp.Model
{
    public class GameSettings
    {
        public enum GameSpeedOption
        {
            Slow,
            Normal,
            Fast
        }

        public enum ChunkAmountOption
        {
            Small,
            Normal,
            Medium,
            Large
        }

        public ChunkAmountOption ChunkAmount { get; set; } = ChunkAmountOption.Normal;

        [JsonIgnore]
        public int ChunkAmountValue =>
            ChunkAmount switch
            {
                ChunkAmountOption.Small => 16,
                ChunkAmountOption.Normal => 32,
                ChunkAmountOption.Medium => 64,
                ChunkAmountOption.Large => 128,
                _ => 32
            };

        public GameSpeedOption GameSpeed { get; set; } = GameSpeedOption.Normal;

        [JsonIgnore]
        public int GameSpeedValue =>
            GameSpeed switch
            {
                GameSpeedOption.Slow => 128,
                GameSpeedOption.Normal => 256,
                GameSpeedOption.Fast => 512,
                _ => 256
            };

        [JsonIgnore]
        private static readonly string FilePath = "GameSettings.json";

        [JsonIgnore]
        public static GameSettings Current { get; private set; } = new();

        [JsonIgnore]
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static void Load()
        {
            if (!File.Exists(FilePath))
                return;

            string json = File.ReadAllText(FilePath);
            Current = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions)
                      ?? new GameSettings();
        }

        public static void Save()
        {
            string json = JsonSerializer.Serialize(Current, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}