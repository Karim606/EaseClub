using EaseClub.Domain.Common;
using EaseClub.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public abstract class EfRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly AppDbContext _context;

        public EfRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> GetByIdAsync(Guid id) =>
            await _context.Set<T>().FirstOrDefaultAsync(x => x.Id==id);

        public async Task AddAsync(T entity) =>
            await _context.Set<T>().AddAsync(entity);

        public Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
    }

}
