namespace RomDiscord.Models.Pages.Quiz
{
	public class AddQuizQuestionModel
	{
		public string Name { get; set; } = "";
		public string Question { get; set; } = "";
		public string Answer { get; set; } = "";
		public string Type { get; set; } = "Open";
		public IFormFile? Image { get; set; } = null;

	}
}
