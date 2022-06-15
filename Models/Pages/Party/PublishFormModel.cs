namespace RomDiscord.Models.Pages.Party
{
	public class PublishFormModel
	{
		public IFormFile? Image { get; set; } = null;
		public string Description { get; set; } = "";
		public ulong Channel { get; set; } = 0;
	}
}
