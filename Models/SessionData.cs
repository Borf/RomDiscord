namespace RomDiscord.Models
{
	public class SessionData
	{
		public class SessionGuild
		{
			public long Id { get; set; }
			public string Name { get; set; }
			public string Icon { get; set; }
		}
		public List<SessionGuild> Guilds { get; set; } = new List<SessionGuild>();
	}
}
