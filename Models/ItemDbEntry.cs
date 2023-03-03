namespace RomDiscord.Models
{
    public class ItemDbEntry
    {
        public class Buff
        {
            public int id { get; set; }
            public string BuffName { get; set; } = "";
            public Dictionary<string, int> BuffRate { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, object> BuffEffect { get; set; } = new Dictionary<string, object>();
            public string Dsc { get; set; } = "";
        }
        public class RefineStorage
        {
            public int Level { get; set; }
            public List<Buff> Buffs { get; set; } = new List<Buff>();
        }
        public int id { get; set; }
        public string NameZh { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Type { get; set; }
        public int Quality { get; set; }
        public int MaxNum { get; set; }
        public int SellPrice { get; set; }
        public string Desc { get; set; } = "";
        public int NoSale { get; set; } = 0;
        public int NoStorage { get; set; } = 0;
        public int Feature { get; set; } = 0;
        public int Condition { get; set; } = 0;
        public int Share { get; set; } = 0;

        public int AdventureSort { get; set; } = 0;
        public int AdventureValue { get; set; } = 0;
        //public List<Buff> AdventureReward { get; set; } = new List<Buff>();
        //public List<Buff> StorageReward { get; set; } = new List<Buff>();
        //public List<RefineStorage> RefineStorageReward { get; set; } = new List<RefineStorage>();
    }
}
