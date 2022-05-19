using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class AccessLevel
	{
		[Key]
		public int AccessLevelId { get; set; }
		public Role Role { get; set; } = null!;
		public User User { get; set; } = null!;
	}
}