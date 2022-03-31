namespace RomDiscord.Models.Db
{
	public class AccessLevel
	{
		public int AccessLevelId { get; set; }
		public Role Role { get; set; }
		public User User { get; set; }
	}
}