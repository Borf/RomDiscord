using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class AttendanceMember
	{
		[Key]
		public int AttendanceMemberId { get; set; }
		public Member Member { get; set; } = null!;
		public Attendance Attendance { get; set; } = null!;
	}
}
