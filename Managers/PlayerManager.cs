using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using BountyBot.Entities;

namespace BountyBot.Managers
{
    internal static class PlayerManager
    {
        private static readonly string recordPath = Directory.GetCurrentDirectory() + "\\players.dat";

        // Fields
        private static Dictionary<ulong,Player> players;
        private static Player[] leaderboard;

        // Properties
        public static Dictionary<ulong, Player> Players { get => players; }
        public static Player[] Leaderboard { get => leaderboard; }

        // Load players from file
        public static void Init()
        {
            if (!File.Exists(recordPath))
                SavePlayers(new Dictionary<ulong,Player>());
            LoadPlayers();
        }

        // Functions
        public static void AddPlayer(Player player)
        {
            LoadPlayers();
            players.Add(player.UID, player);
            SavePlayers();
        }
        public static Player[] UpdateLeaderboard() => leaderboard = players.Values.OrderBy(x => x.Points).ToArray();

        // JSON Functions
        public static Dictionary<ulong, Player> LoadPlayers()
        {
            players = JsonSerializer.Deserialize<Dictionary<ulong, Player>>(File.ReadAllText(recordPath));
            UpdateLeaderboard();
            return players;
        }
        public static void SavePlayers() => File.WriteAllText(recordPath, JsonSerializer.Serialize(players));
        public static void SavePlayers(Dictionary<ulong,Player> players) => File.WriteAllText(recordPath, JsonSerializer.Serialize(players));
    }
}
