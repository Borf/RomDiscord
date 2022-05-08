using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models.Db;

namespace RomDiscord.Controllers
{
	public class Weeklies : Controller
	{
		Context context;

		public Weeklies(Context context)
		{
			this.context = context;
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
