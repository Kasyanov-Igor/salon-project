using Application.Services;
using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RecordController : Controller
    {
        private RecordService _recordService;
        private ILogger<ClientController> _logger;

        public RecordController(ILogger<ClientController> logger, RecordService clientService, IRepository<Client> repository)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._recordService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        [HttpGet]
        public async Task<IActionResult> GetRecords()
        {
            try
            {
                var clients = await _recordService.GetRecords();
                return Ok(clients);  // 200 OK
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении");
                return StatusCode(500, "Внутренняя ошибка сервера");  // 500
            }
        }

        [HttpGet("ByMaster/{id}")]
        public async Task<IEnumerable<Record>> GetRecordsByMaster(int id)
        {
            return await this._recordService.GetRecordsByMaster(id);
        }

        [HttpGet("ByMasterWeek/{id}")]
        public async Task<IEnumerable<Record>> GetRecordsByMasterWeek(int id)
        {
            return await this._recordService.GetRecordsByMasterWeek(id);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Record>> GetRecordById(int id)
        {
            var workout = await this._recordService.GetRecordById(id);
            if (workout == null)
            {
                return NotFound();
            }
            return Ok(workout);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRecord([FromForm] DTORecord dtoRecord)
        {
            if (!ModelState.IsValid)
            {
                this._logger.LogWarning("Неверные данные при создании зала.");
                return BadRequest(ModelState);
            }

            try
            {
                var record = await this._recordService.CreateRecordAsync(dtoRecord);
                this._logger.LogInformation($"(ID: {record.Id}) успешно создан.");

                return Ok(new { Message = $"Запись '{record.Title}' успешно создана." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Произошла ошибка при создании. {ex}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var deleted = await this._recordService.DeleteRecordAsync(id);
            if (!deleted)
            {
                return NotFound(new { Message = $"ERROR" });
            }

            return Ok(new { Message = $"Удаление прошло успешно" });
        }
    }
}
