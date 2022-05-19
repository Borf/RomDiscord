namespace RomDiscord.Models.Pages.Attendance
{
	public class Index
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public Dictionary<int, RomDiscord.Models.Db.Attendance> Attendance { get; set; } = null!;
	
	}
}
