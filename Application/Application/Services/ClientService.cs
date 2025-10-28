using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Web.Mvc;

namespace Application.Services
{
	public class ClientService
	{
		private IClientRepository _clientRepository;
		private IRepository<Client> _repository;
		private ILogger<ClientService> _logger;
		private TokenService _tokenService;
		private MapperConfig _mapper;

		public ClientService(IClientRepository clientRepository, IRepository<Client> repository, MapperConfig mapper, ILogger<ClientService> logger,
								   TokenService tokenService)
		{
			this._tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
			this._clientRepository = clientRepository;
			this._repository = repository;
			this._mapper = mapper;
			this._logger = logger;
		}

		public async Task<bool> UpdateClientAsync(int clientId, DTOClient? newClient)
		{
			return await this._clientRepository.UpdateClientAsync(clientId, newClient);
		}

		public async Task<string> AuthenticateAsync(DTOLogin userLoginDto)
		{
			Client? user = await this._clientRepository.GetClient(userLoginDto.Login, userLoginDto.Password);

			if (user == null)
			{
				_logger.LogWarning("Неудачная попытка входа: неверный логин или пароль."); // Логирование здесь
				throw new UnauthorizedAccessException("Неверные учетные данные"); // Или своё исключение
			}

			// Создание claims (бизнес-логика)
			var claims = new List<Claim>
			 {
				 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				 new Claim(ClaimTypes.Name, user.Login),
				 new Claim(ClaimTypes.Role, user.Status.ToString())
			};

			// Генерация токена
			var token = _tokenService.GenerateToken(claims);

			return token; // Возврат токена для контроллера
		}


		public async Task<Client> CreateClientAsync(DTOClient userDto)
		{
			Client user = this._mapper.CreateMapper().Map<Client>(userDto);

			if (!await this._clientRepository.GetEmail(user.EmailAddress))
			{
				throw new InvalidOperationException("Email address is already in use.");
			}

			// Проверка валидности телефона (если метод в репозитории)
			if (!await this._clientRepository.IsValidPhoneNumber(user.ContactPhoneNumber))
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

		public async Task<IEnumerable<Client>> GetClients()
		{
			return await this._repository.Get();
		}

		public async Task<ActionResult<Client?>> GetClient(int id)
		{
			return await this._repository.GetById(id);
		}

		public async Task<bool> DeleteClientAsync(int id)
		{
			return await this._repository.Delete(id);
		}
	}
}
