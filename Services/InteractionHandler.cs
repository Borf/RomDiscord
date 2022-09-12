using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RomDiscord.Services
{
	public class InteractionHandler
	{
		private readonly DiscordSocketClient _client;
		private readonly InteractionService _handler;
		private readonly CommandService _commandService;
		private readonly IServiceProvider _services;
		private readonly IConfiguration _configuration;
		private List<Action> _messageHandlers = new List<Action>();

		public InteractionHandler(DiscordSocketClient client, InteractionService handler, CommandService commandService, IServiceProvider services, IConfiguration config)
		{
			_client = client;
			_handler = handler;
			_services = services;
			_configuration = config;
			_commandService = commandService;
		}

		public async Task InitializeAsync()
		{
			// Process when the client is ready, so we can register our commands.
			_client.Ready += ReadyAsync;
			_handler.Log += Log;
			_commandService.Log += Log;

			// Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
			await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
			await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			// Process the InteractionCreated payloads to execute Interactions commands
			_client.InteractionCreated += HandleInteraction;
			_client.MessageReceived += HandleMessage;

			//look for message handlers
			//var assembly = Assembly.GetEntryAssembly();
			//if (assembly == null)
			//	return;
			//foreach (var t in assembly.GetTypes())
			//{
			//	var getTasks = t.GetMethod("GetTasks");
			//	if (getTasks != null)
			//	{
			//		using var scope = services.CreateScope();
			//		object? v = scope.ServiceProvider.GetService(t);
			//		List<ScheduledTask>? tasks = (List<ScheduledTask>?)getTasks.Invoke(v, null);
			//		if (tasks != null)
			//			this.tasks.AddRange(tasks);
			//	}
			//}
			//Console.WriteLine("Registered all tasks");
		}

		private Task HandleMessage(SocketMessage rawMessage)
		{
			// Ignore system messages, or messages from other bots
			if (!(rawMessage is SocketUserMessage message))
				return Task.CompletedTask;
			if (message.Source != MessageSource.User)
				return Task.CompletedTask;

			var context = new SocketCommandContext(_client, message);
			_commandService.ExecuteAsync(context, "ImageUpload", _services);
			return Task.CompletedTask;
		}

		private Task Log(LogMessage log)
		{
			Console.WriteLine(log);
			return Task.CompletedTask;
		}

		private async Task ReadyAsync()
		{
			// Context & Slash commands can be automatically registered, but this process needs to happen after the client enters the READY state.
			// Since Global Commands take around 1 hour to register, we should use a test guild to instantly update and test our commands.
			//if (Program.IsDebug())
			//await _handler.RegisterCommandsToGuildAsync(724054882717532171, true); //test guild
			await _handler.RegisterCommandsToGuildAsync(819438757663997992, true); //lumi


			await _handler.RegisterCommandsGloballyAsync(true);





			//var globalCommand = new SlashCommandBuilder();
			//globalCommand.WithName("dmtest");
			//globalCommand.WithDescription("This is my first global slash command");
			//globalCommand.WithDMPermission(true);

			//await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());

			//else
			//	await _handler.RegisterCommandsGloballyAsync(true);
		}

		private async Task HandleInteraction(SocketInteraction interaction)
		{
			Console.WriteLine("Interaction!");
			try
			{
				// Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
				var context = new SocketInteractionContext(_client, interaction);

				// Execute the incoming command.
				var result = await _handler.ExecuteCommandAsync(context, _services);

				if (!result.IsSuccess)
					switch (result.Error)
					{
						case InteractionCommandError.UnmetPrecondition:
							// implement
							break;
						case InteractionCommandError.Exception:
							Console.WriteLine(result.ErrorReason);
							Console.WriteLine(((Discord.Interactions.ExecuteResult)result).Exception);
							break;
						default:
							break;
					}
			}
			catch
			{
				// If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
				// response, or at least let the user know that something went wrong during the command execution.
				if (interaction.Type is InteractionType.ApplicationCommand)
					await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
			}
		}
	}
}