namespace RomDiscord.Models.Pages.Events
{
	public class NewEventPost
	{
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
		public string Where { get; set; } = "";
		public bool DiscordEvent { get; set; } = false;
		public IFormFile? Image { get; set; } = null;
		public DateTime When { get; set; } = DateTime.Now;
		public int LengthHours { get; set; }
		public int LengthMinutes { get; set; }
		public bool Repeats { get; set; } = false;
		public int? RepeatDay { get; set; } = null;
		public int? RepeatHours { get; set; } = null;
		public DateTime? Ends { get; set; } = null;
	}
}
