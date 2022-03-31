using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RomDiscord.Models;
using RomDiscord.Models.Pages;
using RomDiscord.Util;
using System.Diagnostics;

namespace RomDiscord.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			var session = HttpContext.Session.Get<SessionData>("Data");
			Console.WriteLine(session);
			if (User.Identity.IsAuthenticated)
			{
				Console.WriteLine(User.FindFirst(c => c.Type == "username")?.Value);
			}
			return View(new BaseModel(HttpContext));
		}
		
		[HttpGet]
		public IActionResult Login(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		[HttpGet]
		public IActionResult LoginDiscord(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}