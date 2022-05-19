using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizQuestion
	{
		[Key]
		public int QuizQuestionId { get; set; }
		public Quiz Quiz { get; set; } = null!;
		public string Question { get; set; } = "";
		public enum QType
		{
			Open,
			TrueFalse,
			MultipleChoice
		}
		public QType Type { get; set; } = QType.Open;
		public string Answer { get; set; } = "";
		public bool Image { get; set; } = false;
	}
}
