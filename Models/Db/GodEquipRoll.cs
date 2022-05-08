namespace RomDiscord.Models.Db
{
	public class GodEquipRoll
	{
		public int GodEquipRollId { get; set; }
		public GodEquipGuildBinding GodEquip { get; set; }
		public ulong UserId { get; set; }
		public string Name { get; set; } = "";
		public DateOnly Date { get; set; }
	}
}
