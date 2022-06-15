using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Members
{
	public class UpdateMemberModel
	{
		public string Name { get; set; } = null!;
		public ulong DiscordId { get; set; }
		public string AlternativeNames { get; set; } = "";
		public string ShortNote { get; set; } = "";
		public string LongNote { get; set; } = "";
		public List<Job> Jobs { get; set; } = null!;
		public DateTime? JoinDate { get; set; } = null;
	}
}
