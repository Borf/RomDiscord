using RomDiscord.Models.Db;
using System.Reflection;

namespace RomDiscord.Services
{
	public class TaskScheduler : BackgroundService
	{
		public class ScheduledTask
		{
			public DateTime NextRun { get; set; }
			public TimeSpan? TimeSpan { get; set; } = null;
			public Action<IServiceProvider> Action { get; set; } = null!;
		}

		IServiceProvider services;
		List<ScheduledTask> tasks = new List<ScheduledTask>();

		public TaskScheduler(IServiceProvider services)
		{
			this.services = services;
		}

		public async Task InitializeAsync()
		{
			await LoadTasks();
		}
		TaskCompletionSource loaded = new TaskCompletionSource();
		CancellationTokenSource tokenSource = new CancellationTokenSource();
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await loaded.Task;
			Console.WriteLine("Done loading tasks!");
			while(true)
			{
				if (tasks.Count > 0)
				{
					var ts = tasks.Min(t => t.NextRun - DateTime.Now);
					if (ts.TotalMilliseconds > 0)
					{
						try
						{
							await Task.Delay(ts, tokenSource.Token);
						}
						catch (System.Threading.Tasks.TaskCanceledException)
						{
							//to be expected
						}
					}
				}
				else
					await Task.Delay(10000);
				tokenSource.TryReset();

				foreach(var t in tasks.ToList())
				{
					if(t.NextRun <= DateTime.Now)
					{
						t.Action(services);
						if (t.TimeSpan.HasValue)
							t.NextRun += t.TimeSpan.Value;
						else
							tasks.Remove(t);
					}
				}
			}
		}


		public void ScheduleTask(ScheduledTask task)
		{
			tasks.Add(task);
			tokenSource.Cancel();
		}


		private Task LoadTasks()
		{
			var assembly = Assembly.GetEntryAssembly();
			if (assembly == null)
				return Task.CompletedTask;
			foreach(var t in assembly.GetTypes())
			{
				var getTasks = t.GetMethod("GetTasks");
				if(getTasks != null)
				{ 
					object? v = services.GetService(t);
					List<ScheduledTask>? tasks = (List<ScheduledTask>?)getTasks.Invoke(v, null);
					if(tasks != null)
						this.tasks.AddRange(tasks);
				}
			}
			Console.WriteLine("Registered all tasks");
			loaded.SetResult();
			return Task.CompletedTask;
		}
	}
}
