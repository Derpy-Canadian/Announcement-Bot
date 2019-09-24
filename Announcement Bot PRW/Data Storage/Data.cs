using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Announcement_Bot_PRW
{
    public class Data
    {
        static BinaryFormatter formatter = new BinaryFormatter();
        static string path = "Data\\GuildData.gd";

        public static void SaveGuilds(IEnumerable<Server> guilds)
        {
            try
            {
                // Save the data to GuildData.gd
                FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, guilds);

                // Close the stream
                stream.Close();
            }
            catch (Exception exept)
            {
                // Print the error message to console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exept.Message);
                Console.ResetColor();
            }
        }

        public static IEnumerable<Server> LoadGuilds()
        {
            try
            {
                if (File.Exists(path))
                {
                    // Load the data from GuildData.gd
                    FileStream stream = new FileStream(path, FileMode.Open);
                    var serverslist = formatter.Deserialize(stream) as List<Server>;

                    // Close the stream and return the list of servers
                    stream.Close();
                    return serverslist;
                }
                else
                {
                    // Print the error message to console
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Guild Data not found.");
                    Console.ResetColor();
                    return null;
                }
            }
            catch (Exception exept)
            {
                // Print the error messsage to console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exept.Message);
                Console.ResetColor();
                return null;
            }
        }

        public static bool SaveExists()
        {
            return File.Exists("Data\\GuildData.gd");
        }
    }
}
