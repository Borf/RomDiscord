using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class User
	{
		[Key]
		public int UserId { get; set; }
		public string UserName { get; set; }
		public ulong DiscordUserId { get; set; }
		public Guild? Guild { get; set; }
	}
}