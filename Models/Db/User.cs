namespace RomDiscord.Models.Db
{
	public class User
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
		public int DiscordUserId { get; set; }
	}
}