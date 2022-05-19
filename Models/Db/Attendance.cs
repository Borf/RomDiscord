using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class Attendance
	{
		[Key]
		public int AttendanceId { get; set; }
		public Guild Guild { get; set; } = null!;
		public DateOnly Date { get; set; }
		public List<AttendanceMember> Members { get; set; } = new List<AttendanceMember>();
	}
}
