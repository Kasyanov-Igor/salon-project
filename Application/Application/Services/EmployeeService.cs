using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmployeeService
    {
        private IEmployeeRepository _clientRepository;
        private IRepository<Employee> _repository;
        private ILogger<EmployeeService> _logger;
        private TokenService _tokenService;
        private MapperConfig _mapper;

        public EmployeeService(IEmployeeRepository clientRepository, IRepository<Employee> repository, MapperConfig mapper, ILogger<EmployeeService> logger,
                                   TokenService tokenService)
        {
            this._tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            this._clientRepository = clientRepository;
            this._repository = repository;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<string> AuthenticateAsync(DTOLogin userLoginDto)
        {
            Employee? user = await this._clientRepository.GetEmployeeByLogin(userLoginDto.Login, userLoginDto.Password);

            if (user == null)
            {
                _logger.LogWarning("Неудачная попытка входа: неверный логин или пароль."); // Логирование здесь
                throw new UnauthorizedAccessException("Неверные учетные данные"); // Или своё исключение
            }

            // Создание claims (бизнес-логика)
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.Name, user.Login)
            };

            // Генерация токена
            var token = _tokenService.GenerateToken(claims);

            return token; // Возврат токена для контроллера
        }

        public async Task<Employee> CreateClientAsync(DTOEmployee userDto)
        {
            Employee user = this._mapper.CreateMapper().Map<Employee>(userDto);

            if (!await this._clientRepository.IsValidEmail(user.Email))
            {
                throw new InvalidOperationException("Email address is already in use.");
            }

            // Проверка валидности телефона (если метод в репозитории)
            if (!await this._clientRepository.IsValidPhoneNumber(user.PhoneNumber))
            {
                throw new InvalidOperationException("Invalid phone number.");
            }

            // Генерация соли и хэширование пароля
            string salt = PasswordHelper.GenerateSalt();
            string hashedPassword = PasswordHelper.HashPassword(user.Password, salt);
            user.Password = hashedPassword;
            user.Salt = salt;

            await this._repository.Add(user);

            return user;
        }

        public async Task<IEnumerable<Employee>> GetClients()
        {
            return await this._repository.Get();
        }

        public async Task<ActionResult<Employee?>> GetClient(int id)
        {
            return await this._repository.GetById(id);
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            return await this._repository.Delete(id);
        }
    }
}
