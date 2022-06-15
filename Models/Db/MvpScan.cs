using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomDiscord.Models.Db
{
	public class MvpScan
	{
		[Key]
		public int MvpScanId { get; set; }
		public DateTime ScanTime { get; set; }

		[Column(TypeName = "VARCHAR(4)")]
		public string Channel { get; set; } = "";

		public int MvpId { get; set; }

		[Column(TypeName = "VARCHAR(255)")]
		public string CharName { get; set; } = "";

		public int AliveTime { get; set; }

	}
}
