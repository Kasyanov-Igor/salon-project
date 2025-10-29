using Domain.Domains;
using Domain.Entities;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class RecordService
    {
        private IRecordRepository _recordRepository;
        private IRepository<Record> _repository;
        private ILogger<RecordService> _logger;
        private MapperConfig _mapper;
        public RecordService(IRepository<Record> repository, ILogger<RecordService> logger, IRecordRepository recordRepository, MapperConfig mapper) 
        {
            this._repository = repository;
            this._logger = logger;
            this._recordRepository = recordRepository;
            this._mapper = mapper;
        }

        public async Task<Record> CreateRecordAsync(DTORecord recordDto)
        {
            Record record = this._mapper.CreateMapper().Map<Record>(recordDto);

            await this._repository.Add(record);

            return record;
        }

        public async Task<IEnumerable<Record>> GetRecordsByMaster(int idMaster)
        {
            var allRecords = await this._recordRepository.GetRecordsByMaster(idMaster);

            return allRecords;
        }

        public async Task<IEnumerable<Record>> GetRecordsByMasterWeek(int idMaster)
        {
            var allRecords = await this._recordRepository.GetRecordsByMaster(idMaster);

            return allRecords.Where(r => r.BookingTime >= DateTime.Now && r.BookingTime <= DateTime.Now.AddDays(7));
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            return await this._repository.Get();
        }

        public async Task<ActionResult<Record?>> GetRecordById(int id)
        {
            return await this._repository.GetById(id);
        }

        public async Task<bool> DeleteRecordAsync(int id)
        {
            return await this._repository.Delete(id);
        }
    }
}
