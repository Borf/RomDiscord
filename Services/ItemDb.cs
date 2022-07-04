using RomDiscord.Models;
using System.Text.Json;

namespace RomDiscord.Services
{
	public class ItemDb
	{
		public Dictionary<int, ItemDbEntry> db = new Dictionary<int, ItemDbEntry>();
		public ItemDb()
		{
			db = JsonSerializer.Deserialize<Dictionary<string, ItemDbEntry>>(new FileStream("items.json", FileMode.Open))
				?.ToDictionary(keySelector: kv => int.Parse(kv.Key), elementSelector: kv => kv.Value) ?? new Dictionary<int, ItemDbEntry>();

		}

		public ItemDbEntry this[int id]
		{
			get { return db[id]; }
		}
	}
}
