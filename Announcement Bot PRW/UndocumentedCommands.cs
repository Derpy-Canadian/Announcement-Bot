using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Announcement_Bot_PRW
{
    public class UndocumentedCommands : ModuleBase<SocketCommandContext>
    {
        Process currentProcess = Process.GetCurrentProcess();

        [Command("botreport")]
        private async Task BotReportCmd()
        {
            try
            {
                // Guild info
                int guildCount = 0;
                int roleCount = 0;
                // Channel info
                int textChannels = 0;
                int voiceChannels = 0;
                int totalChannels = 0;
                // User info
                int userCount = 0;
                int dndCount = 0;
                int invCount = 0;
                int idlCount = 0;
                int onlCount = 0;
                int afkCount = 0;
                int offCount = 0;

                // Collect all of the data
                foreach (SocketGuild guild in Program._client.Guilds)
                {
                    guildCount++;
                    foreach (IRole role in guild.Roles)
                    {
                        roleCount++;
                    }
                    foreach (SocketTextChannel channel in guild.TextChannels)
                    {
                        totalChannels++;
                        textChannels++;
                    }
                    foreach (SocketVoiceChannel channel in guild.VoiceChannels)
                    {
                        totalChannels++;
                        voiceChannels++;
                    }
                    foreach (SocketGuildUser user in guild.Users)
                    {
                        if (user.Status == UserStatus.AFK)
                            afkCount++;
                        else if (user.Status == UserStatus.Idle)
                            idlCount++;
                        else if (user.Status == UserStatus.Invisible)
                            invCount++;
                        else if (user.Status == UserStatus.Offline)
                            offCount++;
                        else if (user.Status == UserStatus.Online)
                            onlCount++;
                        else if (user.Status == UserStatus.DoNotDisturb)
                            dndCount++;
                        userCount++;
                    }
                }

                // Put all of the data in an embed
                var eb = new EmbedBuilder();
                eb.WithTitle("Announcement Bot Report");
                eb.WithCurrentTimestamp();
                eb.AddField($@"Guild Info", $@"Total Guilds: {guildCount}
Total Roles: {roleCount}", true);
                eb.AddField($@"Channel Info", $@"Total Channels: {totalChannels}
Text Channels: {textChannels}
Voice Channels: {voiceChannels}", true);
                eb.AddField($@"User Info", $@"Total Users: {userCount}
Online Users: {onlCount}
Offline Users: {offCount}
DnD Users: {dndCount}
Idle Users: {idlCount}
", true);

                // Send the embed
                await ReplyAsync($"Here's your report, {Context.Message.Author.Mention}", false, eb.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message); // Send the error message
            }
        }


        [Command("mem")]
        private async Task Mem()
        {
            // Send the memory usage
            await ReplyAsync(currentProcess.PrivateMemorySize64 / 1024 / 1024 + "MB");
        }
    }
}
