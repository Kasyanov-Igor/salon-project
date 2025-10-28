using Domain.Domains;
using Domain.Entities;

namespace Domain.Interface
{
    public interface IClientRepository
    {
        public Task<Client?> GetClient(string login, string password);
        public Task<bool> GetEmail(string emailAddress);
        public Task<bool> UpdateClientAsync(int clientId, DTOClient? newClient);

        public Task<bool> IsValidPhoneNumber(string phoneNumber, string regionCode = "RU");
    }
}
