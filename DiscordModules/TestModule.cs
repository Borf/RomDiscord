using Discord.Commands;

namespace RomDiscord.DiscordModules
{
	public class TestModule : ModuleBase<SocketCommandContext>
	{

		[Command("ping")]
		[Alias("pong", "hello")]
		public Task PingAsync()
			=> ReplyAsync("pong!");


	}
}