namespace RomDiscord.Models.Db
{
	public class Guild
	{
		public int GuildId { get; set; }
		public string GuildName { get; set; }
		public User GuildOwner { get; set; }
		public List<AccessLevel> AccessLevels { get; set; }
	}
}