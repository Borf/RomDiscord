using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Attendance
{
	public class RecordModel
	{
		public List<Member> AllMembers { get; set; } = null!;
		public List<Member> Members { get; set; } = null!;
		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
	}
}
