using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizPlayPlayerScore
	{
		[Key]
		public int QuizPlayPlayerScoreId { get; set; }
		public QuizPlayRound Round { get; set; } = null!;
		public ulong PlayerId { get; set; }
		public int Score { get; set; } = 0;
	}
}
