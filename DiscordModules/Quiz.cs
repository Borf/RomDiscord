using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using RomDiscord.Models.Db;
using RomDiscord.Services;
using System.Collections.Generic;
using System.Linq;

namespace RomDiscord.DiscordModules
{
	public class QuizModule : InteractionModuleBase<SocketInteractionContext>
	{
		class QuizData
		{
			public List<int> QuizIds { get; internal set; }
			public int QuestionCount { get; internal set; } = 10;
			public QuizRunner QuizRunner { get; set; }
		}

		IServiceProvider services;
		static Dictionary<(ulong, ulong), QuizData> quizes = new Dictionary<(ulong, ulong), QuizData>();
		public QuizModule(IServiceProvider services)
		{
			this.services = services;
		}

		[SlashCommand("quiz", "Starts the quiz")]
		public async Task Quiz()
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)] = new QuizData();
			using var scope = services.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<Context>();

			SelectMenuBuilder quizId = new SelectMenuBuilder()
				.WithCustomId("QuizId")
				.WithOptions(db.Quizes.Select(q => new SelectMenuOptionBuilder().WithLabel(q.QuizName).WithValue(q.QuizId + "")).ToList())
				.WithMaxValues(3);

			SelectMenuBuilder questionCount = new SelectMenuBuilder()
				.WithCustomId("QuizQuestionCount")
				.AddOption("10", "10")
				.AddOption("20", "20")
				.AddOption("30", "30")
				.AddOption("40", "40")
				.WithPlaceholder("10")
				.WithMaxValues(3);

			var cb = new ComponentBuilder()
				.WithSelectMenu(quizId)
				.WithSelectMenu(questionCount)
				.WithButton("Start", "StartQuiz");

			await RespondAsync("Make your quiz selection", components: cb.Build());
		}


		[ComponentInteraction("QuizId")]
		public async Task SetQuiz(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].QuizIds = QuizIds.Select(q => int.Parse(q)).ToList();
			await DeferAsync(true);
		}

		[ComponentInteraction("QuizQuestionCount")]
		public async Task QuizQuestionCount(string[] QuestionCount)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].QuestionCount = int.Parse(QuestionCount[0]);
			await DeferAsync(true);
		}

		[ComponentInteraction("StartQuiz")]
		public async Task StartQuiz()
		{
			await DeferAsync(true);
			var quizSettings = quizes[(Context.Channel.Id, Context.Guild.Id)];
			if (quizSettings.QuizIds.Count == 0)
			{
				await RespondAsync("No quiz selected!");
				return;
			}
			await ModifyOriginalResponseAsync(msg => msg.Content = "Let's start!");

			var quizRunner = new QuizRunner(services, Context.Interaction.Channel, quizSettings.QuizIds, quizSettings.QuestionCount);
			quizSettings.QuizRunner = quizRunner;
			await quizRunner.Start();
			quizes.Remove((Context.Channel.Id, Context.Guild.Id));
		}
	}


	public class QuizRunner
	{
		private ISocketMessageChannel channel;
		private List<int> quizIds;
		private int questionCount;
		private IServiceProvider services;
		int question = 0;
		private List<QuizQuestion> questions;
		private string imagePath = "";

		public QuizRunner(IServiceProvider services, ISocketMessageChannel channel, List<int> quizIds, int questionCount)
		{
			this.channel = channel;
			this.quizIds = quizIds;
			this.questionCount = questionCount;
			this.services = services;

			this.imagePath = Path.Combine(services.GetRequiredService<IWebHostEnvironment>().WebRootPath, "img", "Quiz");
			using var scope = services.CreateScope();
			using var context = scope.ServiceProvider.GetRequiredService<Context>();
			this.questions = context.QuizQuestions.Where(q => quizIds.Contains(q.Quiz.QuizId)).ToList().OrderBy(q => Guid.NewGuid()).Take(questionCount).ToList();

		}

		bool inQuestion = false;

		CancellationTokenSource quizMessageInterrupt = new CancellationTokenSource();
		public async Task Start()
		{
			await channel.SendMessageAsync("Let's go");

			var discord = services.GetRequiredService<DiscordSocketClient>();
			discord.MessageReceived += MessageReceived;

			try
			{
				bool guessed = false;
				Random r = new Random();
				///await Task.Delay(3000);
				for (this.question = 0; this.question < this.questions.Count; this.question++)
				{
					if (questions[question].Image)
					{
						using var fs = new FileStream(Path.Combine(imagePath, questions[question].QuizQuestionId + ".jpg"), FileMode.Open);
						await channel.SendFileAsync(fs, "nocheat.jpg", $"**__Question {question + 1}__: {questions[question].Question}**");
					}
					else
						await channel.SendMessageAsync($"**__Question {question + 1}__: {questions[question].Question}**");
					inQuestion = true;
					quizMessageInterrupt = new CancellationTokenSource();
					guessed = false;
					try
					{
						await Task.Delay(5000, quizMessageInterrupt.Token);
					}
					catch (TaskCanceledException) 
					{
						guessed = true;
					}
					List<int> hintSpots = new List<int>();

					string answer = questions[question].Answer;

					for (int hintCount = 0; hintCount < 3 && !guessed; hintCount++)
					{
						var hint = "";
						for (int i = 0; i < answer.Length; i++)
							if (answer[i] != ' ' && !hintSpots.Contains(i))
								hint += "\\_ ";
							else
								hint += answer[i];

						//unmask
						int letter = r.Next(answer.Length);
						int c = 0;
						while (hintSpots.Contains(letter) && answer[letter] != ' ' && c < 10)
						{
							letter = r.Next(answer.Length);
							c++;
						}
						hintSpots.Add(letter);
						await channel.SendMessageAsync($"**Hint {hintCount + 1}:**: {hint}");
						try
						{ 
							await Task.Delay(15000, quizMessageInterrupt.Token);
						}
						catch (TaskCanceledException)
						{
							guessed = true;
							break;
						}
					}
					if(!guessed)
						await channel.SendMessageAsync($"Sorry, nobody guessed the answer. The answer is **{answer}**");
					inQuestion = false;
					await Task.Delay(5000);
				}
			}
			finally
			{
				await channel.SendMessageAsync($"That's it for this quiz for today!");
				discord.MessageReceived -= MessageReceived;
			}
		}

		private async Task MessageReceived(SocketMessage msg)
		{
			if (msg.Channel != this.channel)
				return;
			if (msg.Author.IsBot)
				return;
			if(msg.Content.ToLower() == questions[question].Answer.ToLower())
			{
				quizMessageInterrupt.Cancel();
				await msg.Channel.SendMessageAsync($"That is correct! {msg.Author.Mention} guessed the answer. The answer is **{questions[question].Answer}**");
			}
		}
	}
}