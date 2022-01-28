using Rechnungsverwaltung.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Context
{
    class InvoiceContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }
    }
}
