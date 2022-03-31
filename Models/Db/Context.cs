using FileContextCore;
using Microsoft.EntityFrameworkCore;

namespace RomDiscord.Models.Db
{
	public class Context : DbContext
	{
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccessLevel> AccessLevels { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.UseLoggerFactory(LoggerFactory.Create(builder => { builder.AddConsole(); }))
                //.EnableSensitiveDataLogging()
                //.EnableDetailedErrors()
                .UseFileContextDatabase();
                ;
        }

    }
}
