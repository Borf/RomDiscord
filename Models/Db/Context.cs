using Microsoft.EntityFrameworkCore;

namespace RomDiscord.Models.Db
{
	public class Context : DbContext
	{
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }
        public DbSet<ModuleSetting> ModuleSettings { get; set; }
        //godequip
        public DbSet<GodEquip> GodEquips { get; set; }
        public DbSet<GodEquipGuildBinding> GodEquipGuild { get; set; }
        public DbSet<GodEquipRoll> GodEquipRolls { get; set; }
        //quiz
        public DbSet<Quiz> Quizes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions{ get; set; }
        public DbSet<QuizPlay> QuizPlays { get; set; }
        public DbSet<QuizPlayPlayerScore> QuizPlayScores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }))
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                .UseSqlite("Data Source=Database.db");
        }

    }
}
