namespace RomDiscord.Models.Pages.Quiz
{
	public class EditQuizQuestionModel
	{
		public string Name { get; set; } = "";
		public string Question { get; set; } = "";
		public string Answer { get; set; } = "";
		public string Type { get; set; } = "Open";
		public IFormFile? Image { get; set; } = null;
		public bool RemoveImage { get; set; } = false;
		public string Action { get; set; }
	}
}
