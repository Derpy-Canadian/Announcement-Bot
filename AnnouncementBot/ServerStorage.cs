using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
namespace AnnouncementBot
{
    public class ServerStorage
    {
        private static List<Server> servers;

        static ServerStorage()
        {
            if (Data.SaveExists())
            {
                servers = Data.LoadGuilds().ToList();
            }
            else
            {
                servers = new List<Server>();
                SaveGuilds();
            }
        }
        
        public static void SaveGuilds()
        {
            Data.SaveGuilds(servers);
        }

        public static Server GetGuild(SocketGuild guild)
        {
            return GetOrCreateAccount(guild.Id);
        }

        private static Server GetOrCreateAccount(ulong id)
        {
            var result = from a in servers
                         where a.ID == id
                         select a;

            var account = result.FirstOrDefault();
            if(account == null) account = CreateGuildAccount(id);
            return account;
        }

        private static Server CreateGuildAccount(ulong id)
        {
            var newGuild = new Server()
            {
                ID = id,
                Modlog = false,
                Modlogid = 0,
                name = Program._client.GetGuild(id).Name,
                prefix = "-",
                ModLogReasons = null,
                watchList = null
            };

            servers.Add(newGuild);
            SaveGuilds();
            return newGuild;
        }
    }
}
