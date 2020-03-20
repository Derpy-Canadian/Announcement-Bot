using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Announcement_Bot_PRW
{
    public class StaffCommands : ModuleBase<SocketCommandContext>
    {
        [Command("addmodlog")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        private async Task infot()
        {
            // Add a mod log to the guild collected from guildstorage and create the channel
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
            // Remove the modlog from the guild collected from guildstorage and remove the channel
            var guild = ServerStorage.GetGuild(Context.Guild);
            var chnl = Context.Guild.GetChannel(guild.Modlogid);
            await ReplyAsync("Now removing modlog from your guild...");
            await chnl.DeleteAsync();
            guild.Modlogid = 0;
            guild.Modlog = false;
            ServerStorage.SaveGuilds();
            await ReplyAsync("Your guild no longer has a moderation log");
        }


        [Command("testmodlog")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        private async Task DBG()
        {
            // Variable definitions 
            var guild = ServerStorage.GetGuild(Context.Guild);
            SocketTextChannel log = (SocketTextChannel)Context.Guild.GetChannel(guild.Modlogid);

            // Notify the user
            await ReplyAsync("Sending a test message to your modlog");

            // Create the embed and send it to the modlog
            var eb = new EmbedBuilder();
            eb.WithTitle("This is a moderation log test message");
            eb.WithCurrentTimestamp();
            eb.WithColor(Color.Blue);
            eb.WithDescription("This is a test message to test your servers mod log sent from " + Context.Message.Author.Username);
            var ebb = eb.Build();
            await log.SendMessageAsync("", false, ebb);
            await ReplyAsync("The message has been sent successfully ");
        }


        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        private async Task Kick(IGuildUser user = null, [Remainder]string reason = "reason not set")
        {
            // If an invalid user is chosen, notify the user
            if (user == null)
                await ReplyAsync("Invalid User");
            else
            {
                // Delete the context message and send the kicked message
                await Context.Message.DeleteAsync();
                await ReplyAsync("Kicked " + user.Username + " for " + reason);

                // Get the guild and save all guilds
                var guild = ServerStorage.GetGuild(Context.Guild);
                ServerStorage.SaveGuilds();

                if (guild.Modlog)
                {
                    // Create a modlog embed and send it to the modlog
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


        [Command("ban")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        private async Task Ban(IGuildUser user = null, [Remainder]string reason = "reason not set")
        {
            // If an invalid user is chosen, notify the user
            if (user == null) 
                await ReplyAsync("Invalid User");
            else
            {
                // Delete the context message and tell the user that the ban worked
                await Context.Message.DeleteAsync();
                await ReplyAsync("Banned " + user.Username + " for " + reason);

                // Get the current guild and save all guilds
                var guild = ServerStorage.GetGuild(Context.Guild);

                ServerStorage.SaveGuilds();
                if (guild.Modlog)
                {
                    // Create the modlog embed and send it
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
                // Ban the user
                await Context.Guild.AddBanAsync(user, 1, reason);
            }
        }


        [Command("purge")]
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
    }
}
