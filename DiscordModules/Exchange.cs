using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using System.Collections.Generic;
using System.Linq;

namespace RomDiscord.DiscordModules
{
	public class ExchangeModule : InteractionModuleBase<SocketInteractionContext>
	{
		IServiceProvider services;

		public ExchangeModule(IServiceProvider services)
		{
			this.services = services;
		}

		[SlashCommand("addnotification", "Adds a notification")]
		[EnabledInDm(true)]
		public async Task AddNotification(
			[Summary("Item")]
			[Autocomplete(typeof(ItemNameHandler))] 
			int itemId)
		{
			await RespondAsync("You added a notification for item " + itemId);
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var notifiction = context.ExchangePrivateNotifications.FirstOrDefault(n => n.ItemId == itemId && n.DiscordId == Context.User.Id);
			if (notifiction == null)
			{
				context.ExchangePrivateNotifications.Add(new ExchangePrivateNotification()
				{
					DiscordId = Context.User.Id,
					ItemId = itemId
				});
				await context.SaveChangesAsync();
			}
		}
		[SlashCommand("removenotification", "Removes a notification")]
		[EnabledInDm(true)]
		public async Task RemoveNotification(
		[Summary("Item")]
			[Autocomplete(typeof(ItemNameHandler))]
			int itemId)
		{
			await RespondAsync("You removed the notification for item " + itemId);
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var notifiction = context.ExchangePrivateNotifications.FirstOrDefault(n => n.ItemId == itemId && n.DiscordId == Context.User.Id);
			if (notifiction != null)
				context.ExchangePrivateNotifications.Remove(notifiction);
			await context.SaveChangesAsync();
		}
	}


	public class ItemNameHandler : AutocompleteHandler
	{
		private ItemDb itemDb;

		ItemNameHandler(ItemDb itemDb)
		{
			this.itemDb = itemDb;
		}
		public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
		{
			var typed = autocompleteInteraction.Data.Options.First().Value.ToString() ?? "";

			var results = itemDb.db
				.Where(kv => kv.Value.NameZh.ToLower().Contains(typed.ToLower()))
				.Select(i => new AutocompleteResult(i.Value.NameZh, i.Value.id));

			// max - 25 suggestions at a time (API limit)
			return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25).ToList()));
		}
	}
}