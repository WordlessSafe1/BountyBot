using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BountyBot.Managers
{
    static internal class GuildManager
    {
        private static readonly string recordPath = Directory.GetCurrentDirectory() + "\\servers.cfg";

        public static (ulong id, bool deployment)[] Guilds { get => guilds; }
        private static (ulong, bool)[] guilds;
        public static void Init()
        {
            if (!File.Exists(recordPath))
                throw new FileNotFoundException();
            guilds = LoadGuilds(File.ReadAllLines(recordPath));
        }

        private static (ulong, bool)[] LoadGuilds(string[] guildsConfig)
        {
            string[] temp;
            List<(ulong, bool)> tmpGuilds = new();
            foreach(string line in guildsConfig)
            {
                temp = line.Split(", ");
                switch (temp.Length)
                {
                    case 2:
                        tmpGuilds.Add((Convert.ToUInt64(temp[0]), Convert.ToBoolean(temp[1])));
                        break;
                    case 1:
                        tmpGuilds.Add((Convert.ToUInt64(temp[0]), false));
                        break;
                    default:
                        Exception ex = new FormatException("Servers file not properly formatted.");
                        Environment.FailFast(ex.Message, ex);
                        throw ex;
                }
            }
            return tmpGuilds.ToArray();
        }
    }
}
