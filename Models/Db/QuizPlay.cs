using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
	public class QuizPlay
	{
		[Key]
		public int QuizPlayId { get; set; }

		public Quiz Quiz { get; set; }
	}
}
