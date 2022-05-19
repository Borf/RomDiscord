using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class Guild
	{
		[Key]
		public int GuildId { get; set; }
		public string GuildName { get; set; } = "";
		public ulong DiscordGuildId { get; set; }
		public List<AccessLevel> AccessLevels { get; set; } = null!;
		public List<User> Users { get; set; } = null!;
	}
}