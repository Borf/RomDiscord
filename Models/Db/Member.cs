using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RomDiscord.Models.Db
{
	public class Member
	{
		[Key]
		public int MemberId { get; set; }
		public Guild? Guild { get; set; }
		public string Name { get; set; } = "";
		public string DiscordName { get; set; } = "";
		public ulong DiscordId { get; set; }
		public string Jobs { get; set; } = "";
		public string? ShortNote { get; set; }
		public string? LongNote { get; set; }
		public DateTime? JoinDate { get; set; }
		public int? PartyId { get; set; }
//		[ForeignKey("PartyId")] 
//		public Party? Party { get; set; }

		[NotMapped]
		public List<Job> JobList { 
			get { if (string.IsNullOrEmpty(Jobs) || Jobs.Trim() == "") return new List<Job>();  return Jobs.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries).Select(c => Enum.Parse<Job>(c)).ToList(); }
			set { if (value == null) { Jobs = ""; } else { Jobs = String.Join(",", value.Select(c => c.ToString())); } } }
	}

	public enum Job
	{
		ArcaneMaster,
		Chronomancer,
		Runemaster,
		DivineAvenger,
		BladeSoul,
		PhantomDance,
		StellarHunter,
		SolarTrouvere,
		LunaDanseuse,
		Saint,
		DragonFist,
		Lightbringer,
		Begetter,
		SoulBinder,
		NoviceGuardian,
		InfernoArmor,
		Yamata,
		Ryuoumaru,
		Slayer,
		SpiritWhisperer,
		Tyrant,
	}
}
