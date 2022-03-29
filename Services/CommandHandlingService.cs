using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace RomDiscord.Services
{
	public class CommandHandlingService
	{
		private IServiceProvider serviceProvider;
		private DiscordSocketClient discord;
        private CommandService commandService;
        public CommandHandlingService(CommandService commandService, DiscordSocketClient discord, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.discord = discord;
            this.commandService = commandService;
            commandService.CommandExecuted += CommandExecutedAsync;
            discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // Register modules that are public and inherit ModuleBase<T>.
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
        }

        private async Task MessageReceivedAsync(SocketMessage rawMessage)
		{
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            // This value holds the offset where the prefix ends
            var argPos = 0;
            // Perform prefix check. You may want to replace this with
            // (!message.HasCharPrefix('!', ref argPos))
            // for a more traditional command format like !help.
            if (!message.HasMentionPrefix(discord.CurrentUser, ref argPos) &&
                !message.HasCharPrefix('!', ref argPos))
                return;

            var context = new SocketCommandContext(discord, message);
            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await commandService.ExecuteAsync(context, argPos, serviceProvider);
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync,
   		}

            private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
		{
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }
	}
}
