using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Z.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace AnnouncementBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        Process currentProcess = Process.GetCurrentProcess();

        [Command("ping")]
        private async Task Ping(string site = null)
        {
            if (site != null && Context.Message.Author.Id == 363850072309497876)
            {
                var ping = new System.Net.NetworkInformation.Ping();
                var result = ping.Send(site);
                var Message = await Context.Channel.SendMessageAsync("Pinging " + site + "..."); // Posts a message telling the user it is pinging
                await Message.ModifyAsync(msg => msg.Content = "Pong! 🏓**" + result.RoundtripTime + "ms** roundtrip time from " + site); // Edits the message with the latency. This is done in an edit more or less so the user can get the pinging message to know it is working whereas without that it might have high ping and they would think the command does not work
                await ReplyAsync("**" + Program._client.Latency + " ms** latency from discord to the bot");
            }
            else if (site != null && Context.Message.Author.Id != 363850072309497876)
            {
                await ReplyAsync(":warning: You are unauthorized to use this command! :warning: ");
            }
            else
            {
                var Message = await Context.Channel.SendMessageAsync("Pinging the server..."); // Posts a message telling the user it is pinging
                await Message.ModifyAsync(msg => msg.Content = "Pong! 🏓**" + Program._client.Latency + "ms**"); // Edits the message with the latency. This is done in an edit more or less so the user can get the pinging message to know it is working whereas without that it might have high ping and they would think the command does not work
            }
        }
        [Command("help")]
        private async Task Help(string attrib = null)
        {
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
-invite == Get an invite link to this bot
-suggest == Suggest a feature for the bot
-donate == Get the link to send a donation (minimum $1 USD) to help keep this and my other bots online
");
            eb2.AddField("Staff Commands", @"
-prefix == Change the bot prefix for this server
-addmodlog == Adds a moderation log to your server. Do not delete this channel. Use -removemodlog
-removemodlog == Removes moderation log from your server. Do this instead of deleting the channel yourself
-announce == Create an announcement with the specified message. Run this in your announcement channel
-rannounce == Send an announcement to a specific role. Role name is case sensitive.
-ban == Ban the specified user. If no reason is added it will default to reason not set
-kick == Kick the specified user. If no reason is added it will default to reason not set
-testmodlog == Send a test message to your moderation log", true);// These lines of code create the embed for help

            var ebb = eb.Build();
            var ebb2 = eb2.Build();
            if (attrib == null)
            {
                try
                {
                    var Message = await Context.Channel.SendMessageAsync("Sending help your way!"); // Posts a message telling the user it is sending help to DMs
                    await Context.Message.Author.SendMessageAsync("", false, ebb);
                    await Context.Message.Author.SendMessageAsync("", false, ebb2);
                    await Message.ModifyAsync(msg => msg.Content = "Help has been sent!"); // Edit the message to say help has been sent
                }
                catch
                {
                    await ReplyAsync("Failed to DM you help. Use 'help -nodm' instead");
                }
            }
            else if (attrib == "-nodm")
            {
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
            if (Announcement == null)
                await ReplyAsync("Invalid or no announcement");
            else
            {
                try
                {
                    Program.report.announcementsSent++;
                    var eb = new EmbedBuilder();
                    var eb2 = new EmbedBuilder();
                    var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");
                    eb.WithTitle("Announcement from " + Context.Message.Author.Username + " in " + Context.Guild.Name);
                    eb.WithColor(Color.Blue);
                    eb.WithDescription(Announcement);
                    eb.WithCurrentTimestamp();
                    eb2.WithTitle("Announcement from " + Context.Message.Author.Username);
                    eb2.WithColor(Color.Blue);
                    eb2.WithDescription(Announcement);
                    eb2.WithCurrentTimestamp();
                    var ebb2 = eb2.Build();
                    var ebb = eb.Build();
                    try { await Context.Message.DeleteAsync(); } catch { }
                    if (Context.Guild.Roles.Contains(role))
                    {
                        try { await ReplyAsync("@everyone", false, ebb2); } catch { }
                        foreach (SocketGuildUser user in Context.Guild.Users)
                        {
                            if (user.Roles.Contains(role))
                            {
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
                catch(Exception e)
                {
                    await ReplyAsync($"An error has ocurred: {e.Message} at {e.Source}");
                }
            }
        }
        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        private async Task Ban(IGuildUser user = null, [Remainder]string reason = "reason not set")
        {
            if (user == null)
                await ReplyAsync("Invalid User");
            else
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync("Banned " + user.Username + " for " + reason);
                var guild = ServerStorage.GetGuild(Context.Guild);
                ServerStorage.SaveGuilds();
                if (guild.Modlog)
                {
                    var eb = new EmbedBuilder();
                    eb.WithColor(Color.Red);
                    eb.WithCurrentTimestamp();
                    if (user.IsBot)
                    {
                        eb.WithTitle("A bot has been banned!");
                        eb.AddField("Bot", user.Username, true);
                    }
                    else
                    {
                        eb.WithTitle("A user has been banned!");
                        eb.AddField("User", user.Username, true);
                    }
                    eb.AddField("Moderator", Context.Message.Author.Username, true);
                    eb.AddField("Reason", reason, inline: true);
                    eb.WithThumbnailUrl(user.GetAvatarUrl());
                    var ebb = eb.Build();
                    SocketTextChannel ml = (SocketTextChannel)Context.Guild.GetChannel(guild.Modlogid);
                    await ml.SendMessageAsync("", false, ebb);
                }
                await Context.Guild.AddBanAsync(user, 1, reason);
            }
        }
        [Command("purge", RunMode = RunMode.Async)]
        [Summary("Deletes the specified amount of messages.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChat(int amount = 0)
        {
            if (amount != 0)
            {
                IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
                const int delay = 3000;
                IUserMessage m = await ReplyAsync($"Messages purged. This message will be deleted in 3 seconds");
                await Task.Delay(delay);
                await m.DeleteAsync();
            }
        }
        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        private async Task Kick(IGuildUser user = null, [Remainder]string reason = "reason not set")
        {
            if (user == null)
                await ReplyAsync("Invalid User");
            else
            {
                await Context.Message.DeleteAsync();
                await ReplyAsync("Kicked " + user.Username + " for " + reason);
                var guild = ServerStorage.GetGuild(Context.Guild);
                ServerStorage.SaveGuilds();
                if (guild.Modlog)
                {
                    var eb = new EmbedBuilder();
                    eb.WithColor(Color.Teal);
                    eb.WithCurrentTimestamp();
                    eb.WithTitle("A user has been kicked!");
                    if (user.IsBot)
                    {
                        eb.WithTitle("A bot has been kicked!");
                        eb.AddField("Bot", user.Username, true);
                    }
                    else
                    {
                        eb.WithTitle("A user has been kicked!");
                        eb.AddField("User", user.Username, true);
                    }
                    eb.AddField("Moderator", Context.Message.Author.Username, true);
                    eb.AddField("Reason", reason, true);
                    eb.WithThumbnailUrl(user.GetAvatarUrl());
                    SocketTextChannel ml = (SocketTextChannel)Context.Guild.GetChannel(guild.Modlogid);
                    var ebb = eb.Build();
                    await ml.SendMessageAsync("", false, ebb);
                }
                await user.KickAsync(reason);
            }
        }
        [Command("whois")]
        private async Task WhoIs([Remainder]IGuildUser user = null)
        {
            if (user == null)
                await ReplyAsync("Invalid User");
            else
            {
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
                await ReplyAsync("", false, ebb);
            }

        }
        [Command("optin")]
        private async Task optin()
        {
            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");
            if (Context.Guild.Roles.Contains(role))
            {
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await ReplyAsync("You are now opted into announcements. If you wish to opt out type -optout");
                }
                else
                {
                    await ReplyAsync("You are already opted into announcements. If you wish to opt out type -optout");
                }
            }
            else
            {
                await Context.Guild.CreateRoleAsync("AnnouncementOptIn");
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await ReplyAsync("You are now opted into announcements. If you wish to opt out type -optout");
                }
                else
                {
                    await ReplyAsync("You are already opted into announcements. If you wish to opt out type -optout");
                }
            }
        }
        [Command("optout")]
        private async Task optout()
        {
            SocketGuildUser user = (SocketGuildUser)Context.Message.Author;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "AnnouncementOptIn");
            if (Context.Guild.Roles.Contains(role))
            {
                if (user.Roles.Contains(role))
                {
                    await user.RemoveRoleAsync(role);
                    await ReplyAsync("You are now opted out of announcements. If you wish to opt in again type -optin");
                }
                else
                {
                    await ReplyAsync("You are not opted into announcements. If you wish to opt in type -optin");
                }
            }
            else
            {
                await Context.Guild.CreateRoleAsync("AnnouncementOptIn");
                if (user.Roles.Contains(role))
                {
                    await user.RemoveRoleAsync(role);
                    await ReplyAsync("You are now opted out of announcements. If you wish to opt in again type -optin");
                }
                else
                {
                    await ReplyAsync("You are not opted into announcements. If you wish to opt in type -optin");
                }
            }
        }
        [Command("rannounce")]
        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [RequireBotPermission(GuildPermission.MentionEveryone)]
        private async Task rannounce(SocketRole role = null, [Remainder] string Announcement = null)
        {
            int announcesent = 0;
            if (role == null || Announcement == null)
            {
                await ReplyAsync("Invalid role or no announcement message");
            }
            else
            {
                var eb = new EmbedBuilder();
                var eb2 = new EmbedBuilder();
                eb.WithTitle("Announcement from " + Context.Message.Author.Username + " in " + Context.Guild.Name);
                eb.WithColor(Color.Blue);
                eb.WithDescription(Announcement);
                eb.WithCurrentTimestamp();
                eb2.WithTitle("Announcement from " + Context.Message.Author.Username);
                eb2.WithColor(Color.Blue);
                eb2.WithDescription(Announcement);
                eb2.WithCurrentTimestamp();
                var ebb2 = eb2.Build();
                var ebb = eb.Build();
                await Context.Message.DeleteAsync();
                if (Context.Guild.Roles.Contains(role))
                {
                    await ReplyAsync(role.Mention, false, ebb2);
                    foreach (SocketGuildUser user in Context.Guild.Users)
                    {
                        if (user.Roles.Contains(role))
                        {
                            await user.SendMessageAsync("", false, ebb);
                            announcesent++;
                            if (announcesent >= 5)
                            {
                                await Task.Delay(3000);
                                announcesent = 0;
                            }
                        }
                    }
                }
                else
                {
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
            }
        }
        [Command("testmodlog")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        private async Task DBG()
        {
            var guild = ServerStorage.GetGuild(Context.Guild);
            SocketTextChannel log = (SocketTextChannel)Context.Guild.GetChannel(guild.Modlogid);
            await ReplyAsync("Sending a test message to your modlog");
            var eb = new EmbedBuilder();
            eb.WithTitle("This is a moderation log test message");
            eb.WithCurrentTimestamp();
            eb.WithColor(Color.Blue);
            eb.WithDescription("This is a test message to test your servers mod log sent from " + Context.Message.Author.Username);
            var ebb = eb.Build();
            await log.SendMessageAsync("", false, ebb);
            await ReplyAsync("The message has been sent successfully ");
        }
        [Command("addmodlog")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        private async Task infot()
        {
            var guild = ServerStorage.GetGuild(Context.Guild);
            await ReplyAsync("Adding a moderation log to this guild...");
            IGuildChannel chnl = await Context.Guild.CreateTextChannelAsync("mod-log");
            guild.Modlog = true;
            guild.Modlogid = chnl.Id;
            ServerStorage.SaveGuilds();
            await ReplyAsync("Your guild now has a moderation log. To remove this log type -removemodlog");
        }
        [Command("removemodlog")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        private async Task RemoveModLog()
        {
            var guild = ServerStorage.GetGuild(Context.Guild);
            var chnl = Context.Guild.GetChannel(guild.Modlogid);
            await ReplyAsync("Now removing modlog from your guild...");
            await chnl.DeleteAsync();
            guild.Modlogid = 0;
            guild.Modlog = false;
            ServerStorage.SaveGuilds();
            await ReplyAsync("Your guild no longer has a moderation log");
        }
        [Command("about")]
        private async Task about()
        {
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
            await ReplyAsync("", false, ebb);
        }

        [Command("playing")]
        private async Task playing([Remainder]string playtxt)
        {
            if (Context.Message.Author.Id == 363850072309497876)
            {
                await Program._client.SetGameAsync(playtxt);
                System.IO.File.WriteAllText("playing.txt", playtxt);
                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch
                {
                    await ReplyAsync("Couldn't delete your message. The game I am playing was changed and saved though");
                }
            }
            else
            {
                await ReplyAsync("Only the owner of this bot can use this command");
            }
        }
        [Command("serverinfo")]
        private async Task ServerInfo()
        {
            var eb = new EmbedBuilder();
            eb.WithTitle("Information on " + Context.Guild.Name);
            eb.AddField("Owner",Context.Guild.Owner, true);
            eb.AddField("Created On", Context.Guild.CreatedAt, true);
            //eb.AddField("Emote Count", Context.Guild.Emotes.Count, true);
            eb.AddField("ID", Context.Guild.Id, true);
            eb.AddField("Member Count", Context.Guild.MemberCount, true);
            eb.AddField("Voice Region ID", Context.Guild.VoiceRegionId, true);
            eb.AddField("Text Channel Count", Context.Guild.TextChannels.Count, true);
            eb.AddField("Voice Channel Count", Context.Guild.VoiceChannels.Count, true);
            eb.WithThumbnailUrl(Context.Guild.IconUrl);
            eb.WithCurrentTimestamp();
            var ebb = eb.Build();
            await ReplyAsync("", false, ebb);
        }
        [Command("mem")]
        private async Task Mem()
        {
            await ReplyAsync(currentProcess.PrivateMemorySize64 / 1024 / 1024 + "MB");
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        private async Task Prefix(string prefix)
        {
            var guild = ServerStorage.GetGuild(Context.Guild);
            string oldprefix = guild.prefix;
            guild.prefix = prefix;
            await ReplyAsync("Your prefix has been changed from " + oldprefix + " to " + prefix);
            ServerStorage.SaveGuilds();
        }

        [Command("modlogcount")]
        private async Task MLCOUNT()
        {
            int count = 0;
            foreach (Server sv in Data.LoadGuilds())
            {
                if (sv.Modlog)
                {
                    count++;
                }
            }
            await ReplyAsync("There are " + count + " modlog enabled servers");
        }

        [Command("guildcount")]
        private async Task GuildCount()
        {
            await ReplyAsync("This bot is in " + Program._client.Guilds.Count + " guilds");
        }

        [Command("invite")]
        private async Task Invite()
        {
            await ReplyAsync(@"https://discordbots.org/bot/506258153545924618");
        }

        [Command("testdatastorage")]
        private async Task TestStorage()
        {
            try
            {
                var server = ServerStorage.GetGuild(Context.Guild);
                var eb = new EmbedBuilder();

                eb.WithTitle(server.name);
                eb.AddField("HasModLog", server.Modlog, true);
                eb.AddField("Prefix", server.prefix, true);
                eb.AddField("Server ID", server.ID, true);

                await ReplyAsync("", false, eb.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

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
                await ReplyAsync($"Here's your report, {Context.Message.Author.Mention}", false, eb.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

        }

        [Command("messageall")]
        private async Task MsgAll(string channelName, [Remainder]string messageToSend)
        {
            if (Context.Message.Author.Id == 363850072309497876)
            {
                await ReplyAsync("Sending your mass message...");
                try
                {
                    foreach (SocketGuild guild in Program._client.Guilds)
                    {
                        foreach (SocketTextChannel channel in guild.TextChannels)
                        {
                            if (channel.Name.ToLower() == channelName.ToLower())
                            {
                                await channel.SendMessageAsync(messageToSend);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    await ReplyAsync(e.Message);
                }
                
                await ReplyAsync("Done");
            }
            else
            {
                await ReplyAsync("No.");
            }
        }

        [Command("suggest")]
        private async Task Suggest([Remainder]string suggestion)
        {
            if(suggestion.ToLower() != "hey" || suggestion.ToLower() != "hello" || suggestion.ToLower() != "hi")
            {
                SocketGuild guild = Program._client.GetGuild(543324164191289354);
                SocketTextChannel suggestions = guild.TextChannels.FirstOrDefault(x => x.Id == 544633024357203969);
                var eb = new EmbedBuilder();
                eb.WithTitle("Suggestion From " + Context.Message.Author);
                eb.WithCurrentTimestamp();
                eb.WithColor(Color.Blue);
                eb.WithDescription(suggestion);
                await suggestions.SendMessageAsync("", false, eb.Build());
                await ReplyAsync("Your suggestion has been sent!");
            }
        }

        [Command("donate")]
        private async Task Donate()
        {
            await ReplyAsync("https://donatebot.io/checkout/543324164191289354");
        }

        [Command("nick")]
        private async Task Nick(SocketGuildUser user, [Remainder]string nickname)
        {
            try
            {
                await user.ModifyAsync(x => x.Nickname = nickname);
                await ReplyAsync(user + "'s nickname has been changed");
            }
            catch(Exception e)
            {
                await ReplyAsync(e.Message);
            }

        }

        [Command("blacklist")]
        private async Task BlackList(string username, int discriminator)
        {
            try
            {
                if (Context.Message.Author.Id == 363850072309497876)
                {
                    SocketGuildUser user = null;
                    foreach (SocketGuild guild in Program._client.Guilds)
                    {
                        foreach (SocketGuildUser user2 in guild.Users)
                        {
                            string usernameanddiscrim = user2.Username + "#" + user2.Discriminator;
                            if (usernameanddiscrim == username + "#" + discriminator)
                            {
                                user = user2;
                                await ReplyAsync("Found user! blacklisting..");
                            }
                        }
                    }

                    // Loading routine
                    StringBuilder sb = new StringBuilder();
                    if (File.Exists("Data\\blacklist.txt"))
                        sb.AppendLine(File.ReadAllText("Data\\blacklist.txt"));
                    else
                        File.WriteAllText("Data\\blacklist.txt", "");

                    if (!File.ReadAllText("Data\\blacklist.txt").Contains(user.Id.ToString()))
                    {
                        // blacklist the user
                        sb.AppendLine(user.Id.ToString());
                        await ReplyAsync("Blacklisted " + user);
                        await user.SendMessageAsync("You have been blacklisted from using commands on this bot. If you are the owner of a server and need to be unblacklisted contact Starman via the support server");
                    }
                    else
                    {
                        // unblacklist the user
                        sb.Replace(user.Id.ToString(), "");
                        await ReplyAsync("Unblacklisted " + user);
                        await user.SendMessageAsync("You have been unblacklisted!");
                    }

                    // Save the blacklist
                    File.WriteAllText("Data\\blacklist.txt", sb.ToString());
                }

            }
            catch (Exception e)
            {
                await ReplyAsync($":warning: An error has ocurred: {e.Message}");
            }
        }

        [Command("reply")]
        private async Task Reply(string username, int discriminator, [Remainder]string message)
        {
            SocketGuild guild = Program._client.GetGuild(543324164191289354);
            if (Context.Message.Author.Id == 363850072309497876)
            {
                SocketGuildUser user = null;
                foreach (SocketGuild guild2 in Program._client.Guilds)
                {
                    foreach (SocketGuildUser user2 in guild2.Users)
                    {
                        string usernameanddiscrim = user2.Username + "#" + user2.Discriminator;
                        if (usernameanddiscrim == username + "#" + discriminator)
                        {
                            user = user2;
                            await ReplyAsync("User found. Messaging...");
                        }
                    }
                }
                var eb = new EmbedBuilder();
                eb.WithTitle("Response to your suggestion");
                eb.WithColor(Color.Blue);
                eb.WithDescription(message);
                eb.WithCurrentTimestamp();
                await user.SendMessageAsync("", false, eb.Build());
                var eb2 = new EmbedBuilder();
                eb2.WithTitle("Suggestion Response");
                eb2.WithColor(Color.Blue);
                eb2.WithCurrentTimestamp();
                eb2.WithDescription(message);
            }
        }
    }
}
