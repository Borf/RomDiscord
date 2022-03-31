using Microsoft.AspNetCore.Mvc.RazorPages;
using RomDiscord.Util;

namespace RomDiscord.Models.Pages
{
	public class BaseModel : PageModel
	{
		public SessionData SessionData { get; set; }

		public BaseModel(HttpContext httpContext)
		{
			SessionData = httpContext.Session.Get<SessionData>("Data");
		}
	}
}
