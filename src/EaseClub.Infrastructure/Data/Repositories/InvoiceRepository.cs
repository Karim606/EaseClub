using EaseClub.Domain.Common.Interfaces;
using EaseClub.Domain.Payment;
using EaseClub.Domain.Payment.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Data.Repositories
{
    public class InvoiceRepository : EfRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context):base(context)
        {
        }

        public async Task<Invoice> GetInvoiceWithTransactions(Guid invoiceId,CancellationToken ct = default)
        {
            return await _context.Invoices.Include(x => x.Transactions).FirstOrDefaultAsync(x => x.Id == invoiceId);
        }
    }
}
