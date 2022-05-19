using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizPlayRoundQuiz
	{
		[Key]
		public int QuizPlayRoundQuizId { get; set; }
		public QuizPlayRound QuizPlayRound { get; set; } = null!;
		public Quiz Quiz { get; set; } = null!;
	}
}
