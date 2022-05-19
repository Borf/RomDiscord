using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizPlay
	{
		[Key]
		public int QuizPlayId { get; set; }

		public DateTime DateTime { get; set; }
		public Guild Guild { get; set; } = null!;
		public string Channel { get; set; } = "";
		public List<QuizPlayRound> Rounds { get; set; } = null!;
	}
}
