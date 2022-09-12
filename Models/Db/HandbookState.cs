using System.ComponentModel.DataAnnotations;

namespace RomDiscord.Models.Db
{
    public enum Stat
    {
        Str,
        Int,
        Dex,
        Agi,
        Vit,
        Luk,

        HpRegen,
        SpRegen,

        MaxHP,
        MaxSP,

        Def,
        MDef,
        MDefPercent,
        DefPercent,

        Flee,
        Hit,

        Atk,
        Matk,
        RefineAtk,
        CritDmg,
        Critical,
        AttackSpd,
        DmgToMvpMini,
        MoveSpdPercent,
        MDmgInc,

        StunRes,
        StoneRes,

        AgaintShadow,
        AgainstWater,
        AgainstWind,
        AgDemiHuman,
        AgainstFire,
        AgainstFish,
        AgainstEarth,
        AgainstHoly,
        AgainstPlayer,
        AgaintPoison,
        AgainstInsect,
        AgainstDemon,
        AgainstDragon,
        AgainstUndead,
        AgaintGhost,
        AgainstBeast,
        AgainstFormless,
        AgainstPlant,

        SPPercentRestore,
        IgnoreMDef,
        HPPercentRestore,
        HealingReceived,
        MDmgPercent,
        DmgPercent,
        RefineMatk,
        CritRes,

        HolyDmg,
        WaterDmg,
        EarthDmg,
        FireDmg,
        WindDmg,
        NeutralDmg,


        WaterReduc,
        EarthReduc,
        BruteReduc,
        PlayerReduc,
        DarkReduc,
        UndeadReduc,
        WindReduc,
        AngelReduc,
        MagicReduc,
        DemonReduc,
        DemiHumanRe,
        GhostElementDamage,
        NeutralReduc,
        LSizeReduc,
        SSizeReduc,
        DmgReduc,
        GhostReduc,
        PoisonReduc,
        SkillDmgReduc,
        AutoAttackReduction,

        PoisonElementDamage

    }
    public class HandbookState
	{
		[Key]
		public int HandbookStateId { get; set; }
		public DateOnly Date { get; set; }
		public ulong DiscordUserId { get; set; }
		public Stat Stat { get; set; }
		public int Value { get; set; }
	}


    static class StatUtil
    {
        public static string AsString(this Stat stat)
        {
            return stat.ToString();
        }

        public static Stat AsEnum(string stat)
        {
            stat = stat.Replace("-", "");
            stat = stat.Replace(".", "");
            stat = stat.Replace("/", "");
            stat = stat.Replace("%", "percent");
            return Enum.Parse<Stat>(stat, true);
        }

        internal static bool IsStat(string stat)
        {
            Stat res;
            stat = stat.Replace("-", "");
            stat = stat.Replace(".", "");
            stat = stat.Replace("/", "");
            stat = stat.Replace("%", "percent");
            int r = 0;
            if (int.TryParse(stat, out r))
                if (r != 0)
                    return false;
            if (Enum.TryParse<Stat>(stat, true, out res))
                return true;
            return false;
        }
        internal static string StartsWithStat(string stat)
        {
            stat = stat.Replace("-", "");
            stat = stat.Replace(".", "");
            stat = stat.Replace("/", "");
            stat = stat.Replace("%", "percent");

            var str = Enum.GetNames(typeof(Stat));
            var possibles = str.Where(s => stat.ToLower().StartsWith(s.ToLower())).ToList();
            if (possibles.Count > 0)
                return possibles[0];
            return "";
        }
    }
}
