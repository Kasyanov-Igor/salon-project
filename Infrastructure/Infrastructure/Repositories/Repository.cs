using Application.Services;
using AutoMapper;
using Domain.Interface;
using Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private ADatabaseConnection _connection;
        private MapperConfig _mapper;

        public Repository(ADatabaseConnection connection, MapperConfig mapper)
        {
            this._connection = connection;
            this._mapper = mapper;
        }

        public async Task Add(TEntity entity)
        {
            await _connection.Set<TEntity>().AddAsync(entity);
            await _connection.SaveChangesAsync();
        }

        public async Task<bool> Delete(int id)
        {
            var gym = await _connection.Set<TEntity>().FindAsync(id);
            if (gym == null) return false;

            _connection.Set<TEntity>().Remove(gym);
            await _connection.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TEntity>> Get()
        {
            return await _connection.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetById(int id)
        {
            return await _connection.Set<TEntity>().FindAsync(id);
        }

        public async Task<bool> Update(int id, TEntity updatedEntity)
        {
            var entity = await _connection.Set<TEntity>().FindAsync(id);
            if (entity == null) return false;

            // Используй AutoMapper для маппинга (скопирует свойства из updatedEntity в entity)
            // Если updatedEntity — DTO, а entity — сущность, это сработает (например, DTOClient -> Client)
            this._mapper.CreateMapper().Map(updatedEntity, entity);

            await _connection.SaveChangesAsync();
            return true;
        }
    }
}
