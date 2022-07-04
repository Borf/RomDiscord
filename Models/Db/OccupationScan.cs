using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomDiscord.Models.Db
{
	public class OccupationScan
	{
		[Key]
		public int OccupationScanId { get; set; }
		public DateTime ScanTime { get; set; }

		[Column(TypeName = "VARCHAR(4)")]
		public string Channel { get; set; } = "";

		public int CastleId { get; set; }

		[Column(TypeName = "VARCHAR(64)")]
		public string GuildName { get; set; } = "";

		[Column(TypeName = "VARCHAR(64)")]
		public string LeaderName { get; set; } = "";
		public int MemberCount { get; set; }

	}
}
