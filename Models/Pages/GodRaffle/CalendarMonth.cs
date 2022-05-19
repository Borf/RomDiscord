using RomDiscord.Models.Db;

namespace RomDiscord.Models.Pages.GodRaffle
{
	public class CalendarMonth
	{
		public int Year { get; set; }
		public int Month { get; set; }

		public class Day
		{
			public int Date { get; set; }
			public DayOfWeek DayOfWeek { get; set; }
			public List<GodEquipRoll> rolls { get; set; } = null!;
		}
		public List<Day> Days { get; set; } = new List<Day>();
	}
}