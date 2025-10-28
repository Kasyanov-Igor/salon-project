using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interface
{
    public abstract class ADatabaseConnection: DbContext
    {
        protected abstract string ReturnConnectionString();
        protected string ConnectionString { get; private set; }

        public DbSet<Client> Clients => Set<Client>();

        public ADatabaseConnection()
        {
            ConnectionString = ReturnConnectionString();

            Database.EnsureCreated();
        }
    }
}
