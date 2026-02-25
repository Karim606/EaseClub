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

        public async Task<T> GetByIdAsync(Guid id,CancellationToken ct = default) =>
            await _context.Set<T>().FirstOrDefaultAsync(x => x.Id==id);

        public async Task AddAsync(T entity, CancellationToken ct = default) =>
            await _context.Set<T>().AddAsync(entity,ct);

        public async Task<bool> IsExistAsync(Guid id, CancellationToken ct = default) =>
            await _context.Set<T>().AnyAsync(x => x.Id == id,ct);

        public Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
    }

}
