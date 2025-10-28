using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {
        public Task Add(TEntity entity);
        public Task<IEnumerable<TEntity>> Get();
        public Task<TEntity> GetById(int id);

        public Task<bool> Update(int id, TEntity updatedEntity);

        public Task<bool> Delete(int id);
    }
}
