using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizPlayRound
	{
		[Key]
		public int QuizPlayRoundId { get; set; }
		public QuizPlay QuizPlay { get; set; } = null!;
		public List<QuizPlayRoundQuiz> Quizes { get; set; } = null!;
		public List<QuizPlayPlayerScore> Scores { get; set; } = null!;
	}
}
