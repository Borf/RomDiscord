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
			public List<List<int>> Rounds { get; set; } = Enumerable.Repeat(new List<int>(), 5).ToList(); //warning, all elements are the same
			public int QuestionCount { get; internal set; } = 10;
			public QuizRunner QuizRunner { get; set; } = null!;
			public SocketInteraction Response { get; internal set; } = null!;
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

			var cb = new ComponentBuilder();
			for (int i = 0; i < 3; i++)
			{
				var options = db.Quizes.Select(q => new SelectMenuOptionBuilder().WithLabel(q.QuizName).WithValue(q.QuizId + "")).ToList();
				SelectMenuBuilder quizId = new SelectMenuBuilder()
					.WithCustomId("QuizId" + i)
					.WithPlaceholder("Round " + (i+1))
					.WithOptions(options)
					//.WithMinValues(i == 0 ? 1 : 0)
					.WithMaxValues(Math.Min(options.Count, 10));
				cb.WithSelectMenu(quizId);
			}

			SelectMenuBuilder questionCount = new SelectMenuBuilder()
				.WithCustomId("QuizQuestionCount")
				.AddOption("10", "10")
				.AddOption("20", "20")
				.AddOption("30", "30")
				.AddOption("40", "40")
				.WithPlaceholder("10");


			cb.WithSelectMenu(questionCount)
				.WithButton("Start", "StartQuiz");

			quizes[(Context.Channel.Id, Context.Guild.Id)].Response = this.Context.Interaction;
			await RespondAsync("Make your quiz selection", components: cb.Build());
		}

		[SlashCommand("quizstop", "stops the quiz")]
		public async Task QuizStop()
		{
			if (!quizes.ContainsKey((Context.Channel.Id, Context.Guild.Id)))
				await RespondAsync("There is no quiz!");
			else
			{
				await RespondAsync("Aww, quiz if over");
				var quiz = quizes[(Context.Channel.Id, Context.Guild.Id)];
				if (quiz != null)
					quiz.QuizRunner.Stop();
			}
		}



		[ComponentInteraction("QuizId0")]
		public async Task SetQuiz0(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].Rounds[0] = QuizIds.Select(q => int.Parse(q)).ToList();
			await DeferAsync(true);
		}
		[ComponentInteraction("QuizId1")]
		public async Task SetQuiz1(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].Rounds[1] = QuizIds.Select(q => int.Parse(q)).ToList();
			await DeferAsync(true);
		}
		[ComponentInteraction("QuizId2")]
		public async Task SetQuiz2(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].Rounds[2] = QuizIds.Select(q => int.Parse(q)).ToList();
			await DeferAsync(true);
		}
		[ComponentInteraction("QuizId3")]
		public async Task SetQuiz3(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].Rounds[3] = QuizIds.Select(q => int.Parse(q)).ToList();
			await DeferAsync(true);
		}
		[ComponentInteraction("QuizId4")]
		public async Task SetQuiz4(string[] QuizIds)
		{
			quizes[(Context.Channel.Id, Context.Guild.Id)].Rounds[4] = QuizIds.Select(q => int.Parse(q)).ToList();
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
			await quizSettings.Response.DeleteOriginalResponseAsync();
			if (quizSettings.Rounds[0].Count == 0)
			{
				await RespondAsync("No quiz selected!");
				return;
			}
			var quizRunner = new QuizRunner(services, Context.Interaction.Channel, quizSettings.Rounds, quizSettings.QuestionCount, Context.Guild);
			quizSettings.QuizRunner = quizRunner;
			await quizRunner.Start();
			quizes.Remove((Context.Channel.Id, Context.Guild.Id));
		}
	}


	public class QuizRunner
	{
		private ISocketMessageChannel channel;
		private List<List<int>> rounds;
		private SocketGuild guild;
		private int questionCount;
		private IServiceProvider services;
		int question = 0;
		int round = 0;
		bool running = true;
		private List<QuizQuestion> questions = new List<QuizQuestion>();
		private string imagePath = "";
		private int scoring = 0;
		private int[] scorePoints = new int[] { 5, 3, 2, 1, 1 };
		private DateTime roundEnd;

		public QuizRunner(IServiceProvider services, ISocketMessageChannel channel, List<List<int>> rounds, int questionCount, SocketGuild guild)
		{
			this.channel = channel;
			this.rounds = rounds;
			this.questionCount = questionCount;
			this.services = services;
			this.rounds = this.rounds.Where(r => r.Count > 0).ToList();
			this.guild = guild;

			this.imagePath = Path.Combine(services.GetRequiredService<IWebHostEnvironment>().WebRootPath, "img", "Quiz");
		}

		private int playRoundId;
		public int QuizPlayId { get; private set; }
		CancellationTokenSource quizMessageInterrupt = new CancellationTokenSource();

		public void Stop()
		{
			running = false;
			quizMessageInterrupt.Cancel();
		}


		public async Task Start()
		{
			using (var scope = services.CreateScope())
			{
				using var context = scope.ServiceProvider.GetRequiredService<Context>();
				var play = new QuizPlay()
				{
					Channel = channel.Name,
					Guild = context.Guilds.First(g => g.DiscordGuildId == guild.Id),
					DateTime = DateTime.Now
				};
				context.QuizPlays.Add(play);
				await context.SaveChangesAsync();
				this.QuizPlayId = play.QuizPlayId;
			}
			var discord = services.GetRequiredService<DiscordSocketClient>();
			discord.MessageReceived += MessageReceived;
			await channel.SendMessageAsync($"Let's get this quiz started! we have {rounds.Count} rounds");
			await Task.Delay(2000);
			try
			{
				for (round = 0; round < rounds.Count && running; round++)
				{
					this.playRoundId = -1;
					using (var scope = services.CreateScope())
					{
						using var context = scope.ServiceProvider.GetRequiredService<Context>();
						this.questions = context.QuizQuestions.Where(q => rounds[round].Contains(q.Quiz.QuizId)).ToList().OrderBy(q => Guid.NewGuid()).Take(questionCount).ToList();

						string categories = string.Join(", ", context.Quizes.Where(q => rounds[round].Contains(q.QuizId)).Select(q => q.QuizName).ToList());
						await channel.SendMessageAsync($"Round {round + 1}: {categories}");

						var playRound = new QuizPlayRound()
						{
							QuizPlay = context.QuizPlays.First(qp => qp.QuizPlayId == QuizPlayId),
						};
						context.QuizPlayRounds.Add(playRound);
						await context.SaveChangesAsync();
						playRoundId = playRound.QuizPlayRoundId;
						foreach(var q in rounds[round])
						{
							context.QuizPlayRoundQuizzes.Add(new QuizPlayRoundQuiz()
							{
								QuizPlayRound = playRound,
								Quiz = context.Quizes.First(qq => qq.QuizId == q)
							});
						}
						await context.SaveChangesAsync();
					}


					bool guessed = false;
					Random r = new Random();
					await Task.Delay(6000);
					for (this.question = 0; this.question < this.questions.Count && running; this.question++)
					{
						if (questions[question].Image)
						{
							using var fs = new FileStream(Path.Combine(imagePath, questions[question].QuizQuestionId + ".jpg"), FileMode.Open);
							await channel.SendFileAsync(fs, "nocheat.jpg", $"**__Question {question + 1}__: {questions[question].Question}**");
						}
						else
							await channel.SendMessageAsync($"**__Question {question + 1}__: {questions[question].Question}**");
						quizMessageInterrupt = new CancellationTokenSource();
						guessed = false;
						roundEnd = DateTime.MinValue;
						scoring = 0;
						try
						{
							await Task.Delay(10000, quizMessageInterrupt.Token);
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
							scoring++;
							await channel.SendMessageAsync($"**Hint {hintCount + 1}:**: {hint}");
							try
							{
								await Task.Delay(20000, quizMessageInterrupt.Token);
							}
							catch (TaskCanceledException)
							{
								guessed = true;
								break;
							}
						}
						if (!guessed)
							await channel.SendMessageAsync($"Sorry, nobody guessed the answer. The answer is **{answer}**");
						await Task.Delay(5000);
					}
				}

			}
			finally
			{
				await channel.SendMessageAsync($"That's it for this quiz for today!");
				discord.MessageReceived -= MessageReceived;

				using (var scope = services.CreateScope())
				{
					using var context = scope.ServiceProvider.GetRequiredService<Context>();
					var play = context.QuizPlays.Include(qp => qp.Rounds).ThenInclude(r => r.Scores).First(qp => qp.QuizPlayId == QuizPlayId);
					var totalScores = new Dictionary<ulong, int>();
					foreach(var round in play.Rounds)
					{
						foreach(var score in round.Scores)
						{
							if (!totalScores.ContainsKey(score.PlayerId))
								totalScores[score.PlayerId] = 0;
							totalScores[score.PlayerId] += score.Score;
						}
					}


					EmbedBuilder builder = new EmbedBuilder()
						.WithTitle("Results for the quiz!");
					int i = 1;
					foreach (var kv in totalScores.ToList().OrderByDescending(kv => kv.Value))
					{
						builder.AddField(i+"", "<@" + kv.Key + ">" + ": " + kv.Value + " points", true);
						i++;
					}

					await channel.SendMessageAsync($"The final score", false, builder.Build());

				}

			}
		}

		private async Task MessageReceived(SocketMessage msg)
		{
			if (msg.Channel != this.channel)
				return;
			if (msg.Author.IsBot)
				return;
			if(questions != null && questions.Count > question && msg.Content.ToLower() == questions[question].Answer.ToLower() && (roundEnd == DateTime.MinValue || roundEnd > DateTime.Now))
			{
				int currentScore = 0;
				using (var scope = services.CreateScope())
				{
					using var context = scope.ServiceProvider.GetRequiredService<Context>();
					var score = context.QuizPlayScores.FirstOrDefault(qps => qps.Round.QuizPlayRoundId == playRoundId && qps.PlayerId == msg.Author.Id);
					if (score == null)
					{
						score = new QuizPlayPlayerScore()
						{
							PlayerId = msg.Author.Id,
							Round = context.QuizPlayRounds.First(qpr => qpr.QuizPlayRoundId == playRoundId),
							Score = 0
						};
						context.QuizPlayScores.Add(score);
					}
					score.Score+=scorePoints[scoring];
					await context.SaveChangesAsync();
					currentScore = score.Score;
					scoring++;
				}

				if (roundEnd == DateTime.MinValue)
					quizMessageInterrupt.Cancel();
				await msg.Channel.SendMessageAsync($"That is correct! {msg.Author.Mention} guessed the answer. The answer is **{questions[question].Answer}**. {msg.Author.Mention} now has {currentScore} points");
				if(roundEnd == DateTime.MinValue)
					roundEnd = DateTime.Now.AddSeconds(1);
			}
		}
	}
}