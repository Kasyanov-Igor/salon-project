using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interface
{
    public abstract class ADatabaseConnection: DbContext
    {
        protected abstract string ReturnConnectionString();
        protected string ConnectionString { get; private set; }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Record> Records => Set<Record>();
        public DbSet<Employee> Employees => Set<Employee>();

        public ADatabaseConnection()
        {
            ConnectionString = ReturnConnectionString();

            //Database.EnsureCreated();
        }

        // Добавь метод для применения миграций (если нужно вручную)
        public void ApplyMigrations()
        {
            Database.Migrate();  // Это применит все pending миграции
        }

    }
}
