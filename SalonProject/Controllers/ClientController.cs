using Application.Services;
using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SalonProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private ClientService _clientService;
        private IWebHostEnvironment _environment;
        private ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger,
            IWebHostEnvironment environment, ClientService clientService, IRepository<Client> repository)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        /// <summary>
        /// Регистрирует нового клиента в системе бронирования.
        /// Он принимает данные клиента из формы, валидирует их, создаёт нового пользователя в базе данных
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// - Пример использования: Отправь POST-запрос с формой, содержащей поля DTOClient.
        /// </remarks>
        /// <param name="userDto">DTO с данными клиента для регистрации</param>
		/// <response code="400">Входные данные не заполнены или заполнены неверно</response>
		/// <response code="500">InternalServerError в случае неожиданных ошибок</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromForm] DTOClient userDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }
            try
            {
                await this._clientService.CreateClientAsync(userDto); // Вызов нового метода
                return Ok(new { Message = "Пользователь успешно зарегистрирован." });
            }
            catch (InvalidOperationException ex) // Специфично для дубликатов/валидаций
            {
                _logger.LogWarning($"Registration failed: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex) // Общие ошибки
            {
                _logger.LogError($"Unexpected error during registration: {ex.Message}", ex);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера. Попробуйте позже." });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate([FromForm] DTOLogin userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            try
            {
                var token = await this._clientService.AuthenticateAsync(userLoginDto);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _environment.IsProduction(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                Response.Cookies.Append("authToken", token, cookieOptions);

                return Ok(token);
            }
            catch (UnauthorizedAccessException ex) // Для неудачной аутентификации
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Неверные учетные данные",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex) // Общие ошибки (БД, валидация и т.д.)
            {
                _logger.LogError($"Unexpected error during login: {ex.Message}", ex);
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера. Попробуйте позже." });
            }
        }

        [HttpGet]
        public async Task<IEnumerable<Client>> GetClients()
        {
            return await this._clientService.GetClients();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client?>> GetClient(int id)
        {
            return await this._clientService.GetClient(id);
        }

        [HttpGet("current")]
        public async Task<ActionResult<Client?>> GetCurrentClient()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var client = await this._clientService.GetClient(Convert.ToInt32(userId));

            if (client == null)
            {
                return NotFound(); // Возвращаем 404, если клиент не найден
            }

            return client;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] DTOClient clientDto)
        {
            if (clientDto == null)
            {
                return BadRequest("Invalid client data.");
            }

            var updated = await this._clientService.UpdateClientAsync(id, clientDto);
            if (!updated)
            {
                return NotFound();
            }

            return Ok(new { Message = $"Обновление прошло успешно!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClientAsync(int id)
        {
            bool deleted = await this._clientService.DeleteClientAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return Ok(new { Message = $"Удаление прошло успешно" });
        }
    }
}
