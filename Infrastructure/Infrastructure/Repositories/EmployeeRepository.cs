using Application.Services;
using Domain.Entities;
using Domain.Interface;
using Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using PhoneNumbers;

namespace Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private ADatabaseConnection _connection;

        public EmployeeRepository(ADatabaseConnection connection)
        {
            this._connection = connection;
        }

        public async Task<bool> IsValidEmail(string emailAddress)
        {
            return !await this._connection.Employees.AnyAsync(u => u.Email == emailAddress);
        }

        public async Task<Employee?> GetEmployeeByLogin(string login, string password)
        {
            var user = await _connection.Employees.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null) return null;

            string hashedPassword = PasswordHelper.HashPassword(password, user.Salt);

            return await _connection.Employees.FirstOrDefaultAsync(u => u.Login == login && u.Password == hashedPassword);
        }

        public async Task<bool> IsValidPhoneNumber(string phoneNumber, string regionCode = "RU")
        {
            return await Task.Run(() =>
            {
                var phoneUtil = PhoneNumberUtil.GetInstance();

                try
                {
                    var numberProto = phoneUtil.Parse(phoneNumber, regionCode);
                    return phoneUtil.IsValidNumber(numberProto);
                }
                catch (NumberParseException)
                {
                    return false;
                }
            });
        }
    }
}
