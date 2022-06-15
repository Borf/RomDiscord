using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class Event
	{
		[Key]
		public int EventId { get; set; }
		public Guild Guild { get; set; } = null!;
		public string Name { get; set; } = "";
		public string Description { get; set; } = "";
		public string Where { get; set; } = "";
		public bool DiscordGuildEvent { get; set; } = false;
		public ulong DiscordEventId { get; set; } = 0;
		public ulong DiscordMessageId { get; set; } = 0;
		public ulong DiscordNotificationId { get; set; } = 0;
		public bool HasImage { get; set; } = false;
		public DateTime When { get; set; } = DateTime.Now;
		public TimeSpan Length { get; set; } = TimeSpan.FromMinutes(30);
		public bool Repeats { get; set; } = false;
		public TimeSpan RepeatTime { get; set; } = TimeSpan.Zero;
		public DateTime? Ends { get; set; } = null;


		public DateTime NextWhen
		{
			get
			{
				var w = When;
				if (Repeats)
				{
					while (w < DateTime.Now)
						w += RepeatTime;
					while (w > DateTime.Now)
						w -= RepeatTime;
					w += RepeatTime;
				}
				return w;
			}
		}
		public DateTime CurrentWhen
		{
			get
			{
				var w = When;
				if (Repeats)
				{
					while (w < DateTime.Now)
						w += RepeatTime;
					while (w > DateTime.Now)
						w -= RepeatTime;
					if (!(w <= DateTime.Now && (w + Length) > DateTime.Now))
						w += RepeatTime;
				}
				return w;
			}
		}

		public bool IsActive
        {
			get
			{
				var w = When;
				if (Repeats)
				{
					while (w < DateTime.Now)
						w += RepeatTime;
					while (w > DateTime.Now)
						w -= RepeatTime;
				}
				if (w <= DateTime.Now && (w + Length) > DateTime.Now)
					return true;
				return false;
			}
		}
    }
}
