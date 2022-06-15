using Microsoft.EntityFrameworkCore;

namespace RomDiscord.Models.Db
{
    public class Context : DbContext
    {
        public DbSet<Guild> Guilds => Set<Guild>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<AccessLevel> AccessLevels => Set<AccessLevel>();
        public DbSet<ModuleSetting> ModuleSettings => Set<ModuleSetting>();
        //godequip
        public DbSet<GodEquip> GodEquips => Set<GodEquip>();
        public DbSet<GodEquipGuildBinding> GodEquipGuild => Set<GodEquipGuildBinding>();
        public DbSet<GodEquipRoll> GodEquipRolls => Set<GodEquipRoll>();
        //quiz
        public DbSet<Quiz> Quizes => Set<Quiz>();
        public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
        public DbSet<QuizPlay> QuizPlays => Set<QuizPlay>();
        public DbSet<QuizPlayPlayerScore> QuizPlayScores => Set<QuizPlayPlayerScore>();
        public DbSet<QuizPlayRound> QuizPlayRounds => Set<QuizPlayRound>();
        public DbSet<QuizPlayRoundQuiz> QuizPlayRoundQuizzes => Set<QuizPlayRoundQuiz>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Party> Parties => Set<Party>();
        public DbSet<Attendance> Attendance => Set<Attendance>();
        public DbSet<AttendanceMember> AttendanceMembers => Set<AttendanceMember>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<MvpScan> MvpScans => Set<MvpScan>();

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
