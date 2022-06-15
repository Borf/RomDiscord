using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Party
{
	public class Index
	{
		public List<Member> Members { get; set; } = null!;
		public List<Db.Party> Parties { get; set; } = null!;
		public IReadOnlyCollection<SocketGuildChannel> Channels { get; set; } = null!;
		public ulong ActiveChannel { get; set; }
		public Db.Attendance LastAttendance { get; set; } = null!;
		public Dictionary<Member, List<bool>> MemberAttendance { get; set; } = new Dictionary<Member, List<bool>>();
	}
}
