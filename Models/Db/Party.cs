using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomDiscord.Models.Db
{
	public class Party
	{
		[Key]
		public int PartyId { get; set; }
		public Guild Guild { get; set; } = null!;
		public string Name { get; set; } = "";

		[ForeignKey("LeaderId")]
		public Member Leader { get; set; } = null!;
		public List<Member> Members { get; set; } = null!;
	}
}
