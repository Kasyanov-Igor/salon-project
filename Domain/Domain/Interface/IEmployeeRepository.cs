using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IEmployeeRepository
    {
        public Task<Employee?> GetEmployeeByLogin(string login, string password);
        public Task<bool> IsValidEmail(string emailAddress);
        public Task<bool> IsValidPhoneNumber(string phoneNumber, string regionCode = "RU");
    }
}
