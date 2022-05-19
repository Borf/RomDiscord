using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Models.Pages.Quiz;
using RomDiscord.Services;
using RomDiscord.Util;

namespace RomDiscord.Controllers
{
	[Route("[controller]")]
	[Controller]
	public class QuizController : Controller
	{
		private readonly Context context;
		private readonly ModuleSettings moduleSettings;
		private readonly DiscordSocketClient discord;
		private readonly IWebHostEnvironment hostingEnvironment;

		public QuizController(Context context, DiscordSocketClient discord, ModuleSettings moduleSettings, IWebHostEnvironment hostingEnvironment)
		{
			this.context = context;
			this.discord = discord;
			this.moduleSettings = moduleSettings;
			this.hostingEnvironment = hostingEnvironment;
		}
		public async Task<IActionResult> Index()
		{
			//var guild = context.Guilds.First(g => g.DiscordGuildId == HttpContext.SessionData().ActiveGuild.Id);

			var model = new Models.Pages.Quiz.Index()
			{
				Settings = new Models.Pages.Quiz.SettingsModel(), //new Models.Pages.Quiz.SettingsModel(moduleSettings, guild),
				Channels = new List<SocketGuildChannel>(),// discord.Guilds.First(g => g.Id == guild.DiscordGuildId).Channels,
				Quizes = await context.Quizes.Include(q => q.Questions).Include(q => q.QuizPlays).ToListAsync()
			};


			return View(model);
		}

		[HttpGet("EditQuiz/{id}")]
		public async Task<IActionResult> EditQuiz(int id)
		{
			return View(await context.Quizes.Include(q => q.Questions).FirstAsync(q => q.QuizId == id));
		}

		[HttpPost("EditQuiz/{id}/AddQuestion")]
		public async Task<IActionResult> AddQuestion(int id, [FromForm]AddQuizQuestionModel data)
		{
			var newQuestion = new QuizQuestion()
			{
				Quiz = context.Quizes.First(q => q.QuizId == id),
				Question = data.Question,
				Answer = data.Answer,
				Type = Enum.Parse<QuizQuestion.QType>(data.Type),
			};
			context.QuizQuestions.Add(newQuestion);
			await context.SaveChangesAsync();

			if (data.Image != null)
			{
				string outFile = Path.Combine(hostingEnvironment.WebRootPath, "img", "Quiz", newQuestion.QuizQuestionId + ".jpg");
				using (var stream = System.IO.File.Create(outFile))
					await data.Image.CopyToAsync(stream);
				newQuestion.Image = true;
				await context.SaveChangesAsync();
			}

			return Redirect(Request.Headers["Referer"]);
		}

		[HttpPost("EditQuestion/{id}")]
		public async Task<IActionResult> EditQuestion(int id, [FromForm]EditQuizQuestionModel data)
		{
			if (data.Action == "Delete")
			{
				context.QuizQuestions.Remove(context.QuizQuestions.First(q => q.QuizQuestionId == id));
				await context.SaveChangesAsync();
				return Redirect(Request.Headers["Referer"]);
			}
			var question = context.QuizQuestions.First(q => q.QuizQuestionId == id);
			question.Question = data.Question;
			question.Answer = data.Answer;
			question.Type = Enum.Parse<QuizQuestion.QType>(data.Type);
			if (data.RemoveImage)
			{
				question.Image = false;
				//TODO: remove image from disk
			}
			if (data.Image != null)
			{
				string outFile = Path.Combine(hostingEnvironment.WebRootPath, "img", "Quiz", id + ".jpg");
				using (var stream = System.IO.File.Create(outFile))
					await data.Image.CopyToAsync(stream);
				question.Image = true;
			}
			await context.SaveChangesAsync();
			return Redirect(Request.Headers["Referer"]);
		}


		[HttpPost("AddNewQuiz")]
		public async Task<IActionResult> AddNewQuiz([FromForm] AddNewQuizModel data)
		{
			ulong userid = ulong.Parse(User.Claims.First(u => u.Type == "id").Value);
			context.Quizes.Add(new Quiz()
			{
				QuizName = data.Name,
				Owner = context.Users.First(u => u.DiscordUserId == userid),
			});
			await context.SaveChangesAsync();
			return Redirect(Request.Headers["Referer"]);
		}

	}
}
