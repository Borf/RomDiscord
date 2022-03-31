using Discord.Interactions;

namespace RomDiscord.DiscordModules
{
	public class GodEquipRaffleModule : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("godequipraffle", "Rolls the raffle bot")]
		public async Task GodEquipRaffleAsync()
		{
			await ReplyAsync("Raffling....");
		}

		[SlashCommand("godequipraffleperday", "Shows a post per day")]
		public async Task GodEquipPerDayAsync()
		{
			await ReplyAsync("Raffling....");
		}

	}
}