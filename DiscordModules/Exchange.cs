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
		ItemDb itemDb;
		public ExchangeModule(IServiceProvider services, ItemDb itemDb)
		{
			this.services = services;
			this.itemDb = itemDb;
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
			await RespondAsync("You removed the notification for item " + itemDb[itemId].NameZh + " - " + itemId);
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var notifiction = context.ExchangePrivateNotifications.FirstOrDefault(n => n.ItemId == itemId && n.DiscordId == Context.User.Id);
			if (notifiction != null)
				context.ExchangePrivateNotifications.Remove(notifiction);
			await context.SaveChangesAsync();
		}

		[SlashCommand("listnotifications", "Lists all your notification")]
		[EnabledInDm(true)]
		public async Task ListNotifications()
		{
			await DeferAsync();
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			var notifictions = context.ExchangePrivateNotifications.Where(n => n.DiscordId == Context.User.Id);
			try
			{
				await ModifyOriginalResponseAsync(m => m.Content = string.Join("\n", notifictions.Select(n => itemDb.db[n.ItemId].NameZh + " - " + n.ItemId)));
			}
			catch (Exception ex)
			{
				await ModifyOriginalResponseAsync(m => m.Content = ex.Message);
			}
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

			int itemId = 0;
			int.TryParse(typed, out itemId);

			var results = itemDb.db
				.Where(kv => kv.Value.NameZh.ToLower().Contains(typed.ToLower()) || kv.Key == itemId)
				.Select(i => new AutocompleteResult(i.Value.NameZh, i.Value.id));

			// max - 25 suggestions at a time (API limit)
			return Task.FromResult(AutocompletionResult.FromSuccess(results.Take(25).ToList()));
		}
	}
}