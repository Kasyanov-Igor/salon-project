using Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class SqliteConnection : ADatabaseConnection
    {
        public const string _DATABASE_NAME = "./projectDB.db";

        private static readonly object _saveLock = new object();

        protected override string ReturnConnectionString()
        {
            return $"Data Source={_DATABASE_NAME}";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(ConnectionString);  // Для SQLite
            }
        }
    }
}
