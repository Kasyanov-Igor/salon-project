using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Domains
{
    public class DTOClient
    {
        public string Name { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string ContactPhoneNumber { get; set; } = null!;

        public string EmailAddress { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
