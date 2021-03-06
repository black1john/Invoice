using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class Invoice
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public double Amount { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public int Vat { get; set; }
        public ICollection<PositionEntity> Position { get; set; } = new List<PositionEntity>();
    }
}
