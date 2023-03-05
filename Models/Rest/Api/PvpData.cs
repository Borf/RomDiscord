namespace RomDiscord.Models.Rest.Api;


public class PvpData
{
    public class Rankinfo
    {
        public class Portrait
        {
            public int body { get; set; }
            public int hair { get; set; }
            public int haircolor { get; set; }
            public int gender { get; set; }
            public int head { get; set; }
            public int face { get; set; }
            public int mouth { get; set; }
            public int eye { get; set; }
            public int portraitFrame { get; set; }
            public int portrait { get; set; }
        }

        public string name { get; set; } = string.Empty;
        public Portrait? portrait { get; set; }
        public int rank { get; set; }
        public int score { get; set; }
        public string erank { get; set; } = string.Empty;
        public string profession { get; set; } = string.Empty;
        public string charid { get; set; } = string.Empty;
        public int level { get; set; }
        public string guildname { get; set; } = string.Empty;
    }

    public List<Rankinfo> rankinfo { get; set; } = new List<Rankinfo>();
}

