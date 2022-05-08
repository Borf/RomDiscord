using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class Role
	{
		[Key]
		public int RoleId { get; set; }
		public string Name { get; set; } = "";
	}
}