using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class Quiz
	{
		[Key]
		public int QuizId { get; set; }
		public string QuizName { get; set; } = "";
		public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
		public List<QuizPlay> QuizPlays { get; set; } = new List<QuizPlay>();
		public User Owner { get; set; } = null!;
	}
}
