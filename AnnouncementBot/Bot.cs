using Discord.WebSocket;
using System;
using System.IO;
using System.Text;

namespace AnnouncementBot
{
    public class Bot
    {

        public static void Log(string logMessage)
        {
            StringBuilder bd = new StringBuilder();
            if (File.Exists("Logging//botLog.txt"))
            {
                string log = File.ReadAllText("Logging//botLog.txt");
                bd.Append(log);
                bd.AppendLine(DateTime.Now + " " + logMessage);
                File.WriteAllText("Logging//botlog.txt", bd.ToString());
                bd.Clear();
            }
            else
            {
                File.Create("Logging//botLog.txt");
                string log = File.ReadAllText("Logging//botLog.txt");
                bd.Append(log);
                bd.AppendLine(DateTime.Now + " " + logMessage);
                File.WriteAllText("Logging//botlog.txt", bd.ToString());
            }
        }

        public async static void SendMessage(SocketTextChannel msgChannel, string message)
        {
            await msgChannel.SendMessageAsync(message);
        }
    }
}
