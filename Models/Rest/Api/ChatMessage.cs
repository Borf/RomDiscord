using Humanizer.Localisation;
using RomDiscord.Models.Db;
using System.Text.Json.Serialization;

namespace RomDiscord.Models.Rest.Api;


public class ItemInfo
{
    public class EnchantInfo
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Enchant Enchant { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Level { get; set; }

        public override string ToString()
        {
            Enchant baseEnchant = Enchant;
            int level = 0;
            if (Enchant >= Enchant.Focus && Enchant <= Enchant.AntiMage4_)
            {
                level = (int)Enchant % 10;
                baseEnchant = (Enchant)((int)Enchant / 10 * 10);

            }
            switch (baseEnchant)
            {
                case Enchant.Str: return "Str +" + Level / 1.0;
                case Enchant.Vit: return "Vit +" + Level / 1.0;
                case Enchant.Dex: return "Dex +" + Level / 1.0;
                case Enchant.Agi: return "Agi +" + Level / 1.0;
                case Enchant.Int: return "Int +" + Level / 1.0;
                case Enchant.Luk: return "Luk +" + Level / 1.0;
                case Enchant.MaxHp: return "MaxHP +" + Level / 1.0;
                case Enchant.MaxHpPer: return "MaxHP% +" + Level / 10.0 + "%";
                case Enchant.MaxSp: return "MaxSP +" + Level / 1.0;
                case Enchant.MaxSpPer: return "MaxSP% +" + Level / 10.0 + "%";
                case Enchant.Atk: return "Atk +" + Level / 1.0;
                case Enchant.MAtk: return "MAtk +" + Level / 1.0;
                case Enchant.Def: return "Def +" + Level / 1.0;
                case Enchant.MDef: return "MDef +" + Level / 1.0;
                case Enchant.Hit: return "Hit +" + Level / 1.0;
                case Enchant.Critical: return "Critical +" + Level / 1.0;
                case Enchant.Flee: return "Flee +" + Level / 1.0;
                case Enchant.CritDef: return "Crit.Def +" + Level / 10.0 + "%";
                case Enchant.CritDmg: return "Crit.Dmg +" + Level / 10.0 + "%";
                case Enchant.CritRes: return "Crit.Res +" + Level / 1.0;
                case Enchant.HealingReceived: return "Healing Received +" + Level / 10.0 + "%";
                case Enchant.HealingIncrease: return "Healing Increase +" + Level / 10.0 + "%";
                case Enchant.PhyDmgInc: return "Phy. Dmg Inc +" + Level / 10.0 + "%";
                case Enchant.AttackSpeed: return "Attack Spd +" + Level / 10.0 + "%";
                case Enchant.SilenceRes: return "Silence Res +" + Level / 10.0 + "%";
                case Enchant.FreezeRes: return "Freeze Res +" + Level / 10.0 + "%";
                case Enchant.StoneRes: return "Stone Res +" + Level / 10.0 + "%";
                case Enchant.StunRes: return "Stun Res +" + Level / 10.0 + "%";
                case Enchant.BlindRes: return "Blind Res +" + Level / 10.0 + "%";
                case Enchant.PoisonRes: return "Poison Res +" + Level / 10.0 + "%";
                case Enchant.SnareRes: return "Snare Res +" + Level / 10.0 + "%";
                case Enchant.FearRes: return "Fear Res +" + Level / 10.0 + "%";
                case Enchant.CurseRes: return "Curse Res +" + Level / 10.0 + "%";
                case Enchant.DmgReduc: return "Dmg Reduc +" + Level / 10.0 + "%";


                case Enchant.Focus: return "Focus " + level + " (Chant disrupts durability " + 2.0 * level + "%)";
                case Enchant.Morale: return "Morale " + level + " (Ignore Def +" + 5.0 * level + "%)";
                case Enchant.Magic: return "Magic " + level + " (Shorten CT Variable +" + 2.5 * level + "%)";
                case Enchant.Arch: return "Arch " + level + " (Ranged Atk Inc. +" + 2.5 * level + "%)";
                case Enchant.Sharp: return "Sharp " + level + " (Crit.Dmg +" + 5.0 * level + "%)";
                case Enchant.Tenacity: return "Tenacity " + level + " (Physical Dmg Reduc +" + 2.5 * level + "%)";
                case Enchant.Patience: return "Patience " + level + " (CC Reduction " + 5.0 * level + "%)";
                case Enchant.AntiMage: return "Anti-mage " + level + " (Ignore M.Def " + 5.0 * level + "%)";
                case Enchant.SharpBlade: return "Sharp Blade " + level + " (Melee Atk Inc +" + 2.5 * level + "%)";
                case Enchant.Arcane: return "Arcane " + level + " (M.Dmg +" + 2.0 * level + "%)";
                case Enchant.DivineBlessing: return "Divine Blessing " + level + " (Magic Reduc +" + 2.5 * level + "%)";
                case Enchant.Armor: return "Armor " + level + " (Crit Res +" + 5.0 * level + ")";
                case Enchant.Zeal: return "Zeal " + level + " (Auto Attack Dmg Inc +" + 2.5 * level + "%)";
                case Enchant.Insight: return "Insight " + level + " (Ignore MDef +" + 5.0 * level + "%)";
                case Enchant.Blasphemy: return "Blasphemy " + level + " (Skill Dmg Reduc +" + 2.5 * level + "%)";
                case Enchant.ArmorBreaking: return "Armor Breaking " + level + " (Pen +" + 1.5 * level + "%)";
                case Enchant.AntiMage_: return "Anti-mage " + level + " (MPen +" + 1.5 * level + "%)"; //tuna talisman (acce)
                                                                                                       //blasphemy	skill dmg reduc
                default: return Enchant + " " + Level;
            }

        }

    }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RefineLevel { get; set; }
    public ulong Price { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Guid { get; set; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<EnchantInfo>? Enchants { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Broken { get; set; } = null;
    public int ItemId { get; set; }

}
public class NewChatMessage
{
    public enum MessageType
    {
        Guild,
        ChatBox
    }
    public string Message { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Guild;
    public List<ItemInfo> ItemInfos { get; set; } = new List<ItemInfo>();
    public ulong UserId { get; set; }
    public PhotoInfo? Photo { get; set; }
    public class PhotoInfo
    {
        public ulong Charid { get; set; }
        public int Source { get; set; }
        public int SourceId { get; set; }
        public ulong TimeStamp { get; set; }
    }
}


public class ChatMessage
{
    public string channel { get; set; }
    public string str { get; set; }
    public string name { get; set; }
    public string id { get; set; }
    public int portrait { get; set; }
    public string rolejob { get; set; }
    public int baselevel { get; set; }
    public int hair { get; set; }
    public int haircolor { get; set; }
    public int body { get; set; }
    public string gender { get; set; }
    public string guildname { get; set; }
    public int appellation { get; set; }
    public int head { get; set; }
    public int eye { get; set; }
    public string accid { get; set; }
    public int roomid { get; set; }
    public Photo photo { get; set; }
    public Expression expression { get; set; }
    public int serverid { get; set; }
}

public class Photo
{
}

public class Expression
{
}
