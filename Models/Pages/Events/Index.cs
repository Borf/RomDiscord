using Discord.WebSocket;
using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.Events
{
	public class Index
    {
		public int Year { get; set; }
		public int Month { get; set; }
		public List<Event> Events { get; set; } = new List<Event>();
		public SettingsModel Settings { get; set; } = new SettingsModel();
		public IReadOnlyCollection<SocketTextChannel> Channels { get; set; } = new List<SocketTextChannel>();

		public List<Event> GetEvents(DateTime date)
		{
			List<Event> events = Events.Where(e => e.When.Date == date.Date).ToList();

			var maybeEvents = Events.Where(e => e.Repeats && e.RepeatTime != TimeSpan.Zero);
			foreach (var e in maybeEvents)
			{
				var d = e.When;
				var inc = e.RepeatTime;
				if (inc < new TimeSpan(1, 0, 0, 0))
					inc = new TimeSpan(1, 0, 0, 0);
				if (d.Date < date.Date)
				{//TODO: optimize this
					while (d.Date <= date.Date)
					{
						if (d.Date == date.Date)
						{
							events.Add(e);
							break;
						}
						d = d.Add(inc);
					}
				}
				else if (d.Date > date.Date)
				{
					while (d.Date >= date.Date)
					{
						if (d.Date == date.Date)
						{
							events.Add(e);
							break;
						}
						d = d.Subtract(inc);
					}
				}
			}
			return events;
		}
	}
}
