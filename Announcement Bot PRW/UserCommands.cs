using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Announcement_Bot_PRW
{
    public class UserCommands : ModuleBase<SocketCommandContext>
    {
        Process currentProcess = Process.GetCurrentProcess();

        [Command("help")]
        private async Task Help(string attrib = null)
        {
            // Create the help embeds
            var eb = new EmbedBuilder();
            var eb2 = new EmbedBuilder();
            eb2.WithCurrentTimestamp();
            eb.WithColor(Color.Blue);
            eb2.WithColor(Color.Red);
            eb.AddField("User Commands", @"
-help == Shows this message
-ping == Shows the bots latency
-optin == Opt into being DM'd for announcements
-optout == Opt out of being DM'd for announcements
-about == Get information on this bot
-whois == Get information on a user
");
            eb2.AddField("Staff Commands", @"
-addmodlog == Adds a moderation log to your server. Do not delete this channel. Use -removemodlog
-removemodlog == Removes moderation log from your server. Do this instead of deleting the channel yourself
-announce == Create an announcement with the specified message. Run this in your announcement channel
-ban == Ban the specified user. If no reason is added it will default to reason not set
-kick == Kick the specified user. If no reason is added it will default to reason not set
-testmodlog == Send a test message to your moderation log", true);// These lines of code create the embed for help

            // Build the embeds
            var ebb = eb.Build();
            var ebb2 = eb2.Build();
            if (attrib == null)
            {
                try
                {
                    // Send the embeds to DMs
                    var Message = await Context.Channel.SendMessageAsync("Sending help your way!"); // Posts a message telling the user it is sending help to DMs
                    await Context.Message.Author.SendMessageAsync("", false, ebb);
                    await Context.Message.Author.SendMessageAsync("", false, ebb2);
                    await Message.ModifyAsync(msg => msg.Content = "Help has been sent!"); // Edit the message to say help has been sent
                }
                catch
                {
                    // Notify the user of the failed DM
                    await ReplyAsync("Failed to DM you help. Use 'help -nodm' instead");
                }
            }
            else if (attrib == "-nodm")
            {
                // Send the embeds in the channel if -nodm is ran
                await ReplyAsync("", false, ebb);
                await ReplyAsync("", false, ebb2);
            }
        }


        [Command("announce")]
        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [RequireBotPermission(GuildPermission.MentionEveryone)]
        private async Task Announce([Remainder] string Announcement = null)
        {
            int announcesent = 0;
            // If there is no announcement, notify the user of the errr
            if (Announcement == null)
                await ReplyAsync("Invalid or no announcement");
            else
            {
                try
                {
                    // Create the announcement embeds (for DM and server)
                    var eb = new EmbedBuilder();
                    var eb2 = new EmbedBuilder();
                    var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");
                    eb.WithTitle("Announcement from " + Context.Message.Author.Username + " in " + Context.Guild.Name);
                    eb.WithColor(new Color(126, 170, 255));
                    eb.WithDescription(Announcement);
                    eb.WithCurrentTimestamp();
                    eb2.WithTitle("Announcement from " + Context.Message.Author.Username);
                    eb2.WithColor(new Color(126, 170, 255));
                    eb2.WithDescription(Announcement);
                    eb2.WithCurrentTimestamp();

                    // Build the embeds
                    var ebb2 = eb2.Build();
                    var ebb = eb.Build();

                    // Delete the context message then begin sending the announcement if the announcement role exists
                    try { await Context.Message.DeleteAsync(); } catch { }
                    if (Context.Guild.Roles.Contains(role))
                    {
                        // Try sending the server announcement
                        try { await ReplyAsync("@everyone", false, ebb2); } catch { }

                        foreach (SocketGuildUser user in Context.Guild.Users) // Iterate over every user
                        {
                            if (user.Roles.Contains(role)) // If the user has the opt in role
                            {
                                // Try to send them the announcement in DMs
                                try
                                {
                                    await user.SendMessageAsync("", false, ebb);
                                    announcesent++;
                                    if (announcesent >= 5)
                                    {
                                        await Task.Delay(3000);
                                        announcesent = 0;
                                        Console.WriteLine("Cooldown");
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            // Create the opt in role (if it doesn't exist) then proceed with the announcements
                            await Context.Guild.CreateRoleAsync("AnnouncementOptIn");
                            await ReplyAsync(role.Mention, false, ebb);
                            foreach (SocketGuildUser user in Context.Guild.Users)
                            {
                                if (user.Roles.Contains(role))
                                {
                                    await user.SendMessageAsync("", false, ebb);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception e)
                {
                    // Send the error message
                    await ReplyAsync($"An error has ocurred: {e.Message} at {e.Source}");
                }
            }
        }


        [Command("ping")]
        private async Task Ping(string site = null)
        {
            if (site != null && Context.Message.Author.Id == 363850072309497876)
            {
                // Get the latency to the website 
                var ping = new System.Net.NetworkInformation.Ping();
                var result = ping.Send(site);
                var Message = await Context.Channel.SendMessageAsync("Pinging " + site + "..."); // Posts a message telling the user it is pinging
                await Message.ModifyAsync(msg => msg.Content = "Pong! 🏓**" + result.RoundtripTime + "ms** roundtrip time from " + site); // Edits the message with the latency. This is done in an edit more or less so the user can get the pinging message to know it is working whereas without that it might have high ping and they would think the command does not work
                await ReplyAsync("**" + Program._client.Latency + " ms** latency from discord to the bot");
            }
            else if (site != null && Context.Message.Author.Id != 363850072309497876)
            {
                // If the user executing the command is not Starman, tell them they can't ping a website
                await ReplyAsync(":warning: You are unauthorized to use this command! :warning: ");
            }
            else
            {
                // Send the ping
                var Message = await Context.Channel.SendMessageAsync("Pinging the server..."); // Posts a message telling the user it is pinging
                await Message.ModifyAsync(msg => msg.Content = "Pong! 🏓**" + Program._client.Latency + "ms**"); // Edits the message with the latency. This is done in an edit more or less so the user can get the pinging message to know it is working whereas without that it might have high ping and they would think the command does not work
            }
        }


        [Command("whois")]
        private async Task WhoIs([Remainder]IGuildUser user = null)
        {
            // If an invalid user is chosen, notify the user
            if (user == null)
                await ReplyAsync("Invalid User");
            else
            {
                // Create the embed
                var eb = new EmbedBuilder();
                SocketGuildUser user2 = user as SocketGuildUser;
                eb.WithTitle("Information on " + user.Username);
                eb.WithColor(Color.Blue);
                eb.WithCurrentTimestamp();
                eb.AddField("Account Created", user.CreatedAt, true);
                eb.AddField("ID", user.Id, true);
                eb.AddField("Joined This Server", user.JoinedAt, true);
                eb.AddField("Bot", user.IsBot, true);
                try
                {
                    if (user.Activity.Type == ActivityType.Listening)
                        eb.AddField("Listening", user.Activity, true);
                    else if (user.Activity.Type == ActivityType.Playing)
                        eb.AddField("Playing", user.Activity, true);
                    else if (user.Activity.Type == ActivityType.Streaming)
                        eb.AddField("Streaming", user.Activity, true);
                    else if (user.Activity.Type == ActivityType.Watching)
                        eb.AddField("Watching", user.Activity, true);
                }
                catch
                {
                    eb.AddField("Playing", "Nothing");
                }
                eb.WithThumbnailUrl(user.GetAvatarUrl());
                var ebb = eb.Build();

                // Send the embed
                await ReplyAsync("", false, ebb);
            }

        }


        [Command("optin")]
        private async Task optin()
        {
            // Get the user and define the opt in role
            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");

            if (Context.Guild.Roles.Contains(role))
            {
                if (!user.Roles.Contains(role))
                {
                    // If the user does not have the role, give it to them
                    await user.AddRoleAsync(role);
                    await ReplyAsync("You are now opted into announcements. If you wish to opt out type -optout");
                }
                else
                {
                    // Otherwise tell them they are already opted in
                    await ReplyAsync("You are already opted into announcements. If you wish to opt out type -optout");
                }
            }
            else
            {
                // If the role does not exist, create it 
                await Context.Guild.CreateRoleAsync("AnnouncementOptIn");
                if (!user.Roles.Contains(role))
                {
                    // If the user does not have the role, give it to them
                    await user.AddRoleAsync(role);
                    await ReplyAsync("You are now opted into announcements. If you wish to opt out type -optout");
                }
                else
                {
                    // Otherwise tell them they are already opted in
                    await ReplyAsync("You are already opted into announcements. If you wish to opt out type -optout");
                }
            }
        }


        [Command("optout")]
        private async Task optout()
        {
            // Get the user and define the opt in role
            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");

            if (Context.Guild.Roles.Contains(role))
            {
                if (user.Roles.Contains(role))
                {
                    // If the user has the role, remove it
                    await user.RemoveRoleAsync(role);
                    await ReplyAsync("You are now opted out of announcements. If you wish to opt in again type -optin");
                }
                else
                {
                    // Otherwise tell them they are not opted in
                    await ReplyAsync("You are not opted into announcements. If you wish to opt in type -optin");
                }
            }
            else
            {
                // If the role does not exist, create it
                await Context.Guild.CreateRoleAsync("AnnouncementOptIn");

                if (user.Roles.Contains(role))
                {
                    // If the user has the role, remove it
                    await user.RemoveRoleAsync(role);
                    await ReplyAsync("You are now opted out of announcements. If you wish to opt in again type -optin");
                }
                else
                {
                    // Otherwise tell them they are not opted in
                    await ReplyAsync("You are not opted into announcements. If you wish to opt in type -optin");
                }
            }
        }


        [Command("about")]
        private async Task about()
        {
            // Create the embed
            var eb = new EmbedBuilder();
            eb.WithTitle("Announcement Bot Beta");
            eb.WithCurrentTimestamp();
            eb.WithColor(Color.Blue);
            eb.AddField("Credits", "Starman#9216 - Idea, and Programming", true);
            eb.AddField("Version", "2.1", true);
            eb.AddField("Programming Language", "C#", true);
            eb.AddField("Library", "Discord.NET", true);
            eb.AddField("Guilds", Program._client.Guilds.Count, true);
            int users = 0;
            foreach (SocketGuild guild in Program._client.Guilds)
            {
                foreach (SocketGuildUser user in guild.Users)
                {
                    users++;
                }
            }
            eb.AddField("Users", users, true);
            eb.AddField("Memory Usage", currentProcess.PrivateMemorySize64 / 1024 / 1024 + "MB", true);
            var ebb = eb.Build();

            // Send the embed
            await ReplyAsync("", false, ebb);
        }


        [Command("serverinfo")]
        private async Task ServerInfo()
        {
            // Create the embed
            var eb = new EmbedBuilder();
            eb.WithTitle("Information on " + Context.Guild.Name);
            eb.AddField("Owner", Context.Guild.Owner, true);
            eb.AddField("Created On", Context.Guild.CreatedAt, true);
            eb.AddField("ID", Context.Guild.Id, true);
            eb.AddField("Member Count", Context.Guild.MemberCount, true);
            eb.AddField("Voice Region ID", Context.Guild.VoiceRegionId, true);
            eb.AddField("Text Channel Count", Context.Guild.TextChannels.Count, true);
            eb.AddField("Voice Channel Count", Context.Guild.VoiceChannels.Count, true);
            eb.WithThumbnailUrl(Context.Guild.IconUrl);
            eb.WithCurrentTimestamp();
            var ebb = eb.Build();

            // Send the embed
            await ReplyAsync("", false, ebb);
        }
    }
}
