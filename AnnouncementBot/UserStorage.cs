using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnouncementBot
{
    public class UserStorage
    {
        private static List<User> users;

        static UserStorage()
        {
            if (Data2.SaveExists())
            {
                users = Data2.LoadUsers().ToList();
            }
            else
            {
                users = new List<User>();
                SaveUsers();
            }
        }

        public static void SaveUsers()
        {
            Data2.SaveUsers(users);
        }

        public static User GetUser(SocketGuildUser user)
        {
            return GetOrCreateAccount(user.Id, user.Username);
        }

        private static User GetOrCreateAccount(ulong id, string username)
        {
            var result = from a in users
                         where a.clientID == id
                         select a;

            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id, username);
            return account;
        }

        private static User CreateUserAccount(ulong id, string username)
        {
            var newUser = new User()
            {
                clientID = id,
                username = username,
                warnings = new List<Warning>(),
            };

            users.Add(newUser);
            SaveUsers();
            return newUser;
        }
    }
}
