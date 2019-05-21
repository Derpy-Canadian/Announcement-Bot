using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnnouncementBot
{
    public class Data
    {
        static BinaryFormatter formatter = new BinaryFormatter();
        public static void SaveGuilds(IEnumerable<Server> guilds)
        {
            try
            {
                // Variables
                string path = "Data\\GuildData.gd";

                // Save
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, guilds);

                // Cleanup
                stream.Close();
            }
            catch (Exception exept)
            {
                // Error
                Console.WriteLine("~r~" + exept.Message);
            }
        }

        public static IEnumerable<Server> LoadGuilds()
        {
            try
            {
                // Variables
                string path = "Data\\GuildData.gd";

                // Statements
                if (File.Exists(path))
                {
                    // Load
                    FileStream stream = new FileStream(path, FileMode.Open);
                    var serverslist = formatter.Deserialize(stream) as List<Server>;

                    // Cleanup
                    stream.Close();
                    return serverslist;
                }
                else
                {
                    // Error
                    Console.WriteLine("~r~Blips file not found in " + path + @"!
~w~Don't worry about this if you just installed the mod, this is normal.");
                    return null;
                }
            }
            catch (Exception exept)
            {
                // Error
                Console.WriteLine("~r~" + exept.Message);
                return null;
            }
        }

        public static bool SaveExists()
        {
            return File.Exists("Data\\GuildData.gd");
        }
    }
}
