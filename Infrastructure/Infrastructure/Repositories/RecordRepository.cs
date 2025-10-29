using Domain.Entities;
using Domain.Interface;
using Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RecordRepository : IRecordRepository
    {
        private ADatabaseConnection _connection;

        public RecordRepository(ADatabaseConnection connection)
        {
            this._connection = connection;
        }

        public async Task<IEnumerable<Record>> GetRecordsByMaster(int id)
        {
            var allRecords = await this._connection.Records.ToListAsync();

            return allRecords.Where(r => r.MasterId == id);
        }
    }
}
