using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Announcement_Bot_PRW
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();

        public static DiscordSocketClient _client;
        private CommandService _commands;
        public static IServiceProvider _services;

        // Runbot task
        public async Task RunBot()
        {
            _client = new DiscordSocketClient(); // Define _client
            _commands = new CommandService(); // Define _commands

            _services = new ServiceCollection() // Define _services
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken = "TOKEN"; // Define the bot token

            _client.Log += Log; // Set up logging

            await RegisterCommandsAsync(); // Call registercommands

            await _client.LoginAsync(TokenType.Bot, botToken); // Log into the bot user

            await _client.StartAsync(); // Start the bot user
            await Task.Delay(-1); // Delay for -1 to keep the console window opened
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync; // Messagerecieved
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null); // Set up the command handler
        }

        private Task Log(LogMessage arg) // Logging
        {
            Console.WriteLine(arg.Message); // Write the log to console
            return Task.CompletedTask; // Return with completedtask
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage; // Create a variable with the message as SocketUserMessage
            IGuildChannel chnl = (IGuildChannel)arg.Channel; // Define the current channel
            var guild = ServerStorage.GetGuild((SocketGuild)chnl.Guild); // Get the data for the current guild
            if (message is null || message.Author.IsBot) return; // Checks if the message is empty or sent by a bot
            int argumentPos = 0; // Sets the argpos to 0 (the start of the message)
            if (message.HasStringPrefix(guild.prefix, ref argumentPos) || message.HasMentionPrefix(_client.CurrentUser, ref argumentPos) || message.HasStringPrefix("ab-", ref argumentPos)) // If the message has the prefix at the start or starts with someone mentioning the bot
            {
                var context = new SocketCommandContext(_client, message); // Create a variable called context
                var result = await _commands.ExecuteAsync(context, argumentPos, _services); // Create a veriable called result
            }
        }
    }
}
