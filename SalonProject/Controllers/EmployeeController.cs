using Application.Services;
using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private EmployeeService _clientService;
        private IWebHostEnvironment _environment;
        private ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger,
                IWebHostEnvironment environment, EmployeeService clientService, IRepository<Client> repository)
        {
            this._logger = logger;
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
        /// <response code="201">Пользователь успешно зарегистрирован</response>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterEmployee([FromForm] DTOEmployee userDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }
            try
            {
                Employee user = await this._clientService.CreateClientAsync(userDto); // Вызов нового метода
                return CreatedAtAction(nameof(GetClient), new { id = user.Id }, new
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    ContactPhoneNumber = user.PhoneNumber,
                    Message = "Пользователь успешно зарегистрирован."
                });
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

        /// <summary>
        /// Получает список всех клиентов.
        /// </summary>
        /// <remarks>
        /// Этот метод требует авторизации. В заголовке запроса должен быть указан Bearer токен авторизации.
        /// Пример заголовка: Authorization: Bearer {your-token}
        /// </remarks>
        /// <returns>
        /// Возвращает список клиентов в формате JSON с кодом ответа 200 (OK), если операция прошла успешно.
        /// В случае внутренней ошибки сервера возвращает код 500 (Internal Server Error) с сообщением об ошибке.
        /// </returns>
        /// <response code="200">Список клиентов успешно получен.</response>
        /// <response code="500">Внутренняя ошибка сервера при получении клиентов.</response>
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            try
            {
                var clients = await _clientService.GetClients();
                return Ok(clients);  // 200 OK
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиентов");
                return StatusCode(500, "Внутренняя ошибка сервера");  // 500
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee?>> GetClient(int id)
        {
            try
            {
                var client = await this._clientService.GetClient(id);
                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиентов");
                return StatusCode(500, "Внутренняя ошибка сервера");  // 500
            }
        }

        /// <summary>
        /// Получить текущего клиента на основе аутентифицированного пользователя.
        /// </summary>
        /// <returns>Информация о клиенте или соответствующий статус ошибки.</returns>
        /// <response code="200">Клиент найден и возвращён.</response>
        /// <response code="400">Некорректный идентификатор пользователя.</response>
        /// <response code="401">Пользователь не аутентифицирован.</response>
        /// <response code="404">Клиент не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("current")]
        [ProducesResponseType(typeof(Employee), 200)]  // Для Swagger: успешный ответ с типом Client
        public async Task<ActionResult<Employee?>> GetCurrentClient()
        {
            try
            {
                // Получаем userId из claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Идентификатор пользователя отсутствует в токене.");
                    return BadRequest("Некорректный идентификатор пользователя.");  // 400
                }

                // Получаем клиента через сервис
                var client = await _clientService.GetClient(Convert.ToInt32(userId));

                if (client == null)
                {
                    _logger.LogInformation("Клиент с ID {userId} не найден.", userId);
                    return NotFound();  // 404
                }

                _logger.LogInformation("Клиент с ID {userId} успешно найден.", userId);
                return Ok(client);  // 200 OK с данными клиента
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении текущего клиента для userId из токена.");
                return StatusCode(500, "Внутренняя ошибка сервера.");  // 500
            }
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
