namespace RomDiscord.Util
{
	public static class Util
	{
		public static string NameToId(string name)
		{
			name = name.ToLower();
			name = name.Trim();
			name = name.Replace(" ", "");
			name = name.Replace("'", "");
			return name;
		}
	}
}
