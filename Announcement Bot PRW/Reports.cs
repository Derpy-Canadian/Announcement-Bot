using Announcement_Bot_PRW.Classes;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Announcement_Bot_PRW
{
    public class Reports
    {
        public static DailyReport report = new DailyReport() // Create a blank report
        {
            dayOfReport = DateTime.Now,
            announcementsSent = 0,
            commandsRan = 0,
            guildsJoined = 0,
            guildsLeft = 0,
            messagesSent = 0,
            usersJoined = 0,
            usersLeft = 0
        };

        internal static void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            sendReport(true); // Send out the bot report
        }

        private async static void sendReport(bool isDaily)
        {
            SocketGuild supportServer = Program._client.GetGuild(513535156376698882);
            SocketTextChannel logChannel = (SocketTextChannel)supportServer.GetChannel(661434743929438209);
            string json = JsonConvert.SerializeObject(report, Formatting.Indented);
            File.WriteAllText("DailyReports\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + " Report.json", json);
            await logChannel.SendFileAsync("DailyReports\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + " Report.json", "Daily report for " + DateTime.Now);
            if (isDaily)
            {
                report = new DailyReport()
                {
                    dayOfReport = DateTime.Now,
                    announcementsSent = 0,
                    commandsRan = 0,
                    guildsJoined = 0,
                    guildsLeft = 0,
                    messagesSent = 0,
                    usersJoined = 0,
                    usersLeft = 0
                };
            }
        }

        internal static async Task userJoin(SocketGuildUser arg) { report.usersJoined++; }

        internal static async Task userLeave(SocketGuildUser arg) { report.usersLeft++; }

        internal static async Task joinGuild(SocketGuild arg) { report.guildsJoined++; }

        internal static async Task leftGuild(SocketGuild arg) { report.guildsLeft++; }
    }
}
