using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RomDiscord.Models.Db;
using RomDiscord.Util;

namespace RomDiscord.Models.Pages
{
	public class IndexModel
	{
		public Guild? Guild { get; set; } = null;
		public bool BotInGuild { get; set; } = false;
		public string ClientId { get; internal set; }
	}
}
