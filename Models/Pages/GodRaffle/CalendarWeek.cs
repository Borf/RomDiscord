using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.GodRaffle
{
	public class CalendarWeek
	{
		public int Year { get; set; }
		public int Week { get; set; }

		public class Day
		{
			public DateTime Date { get; set; }
			public DayOfWeek DayOfWeek { get; set; }
			public List<GodEquipRoll> rolls { get; set; }
		}
		public List<Day> Days { get; set; } = new List<Day>();
	}
}