using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Model
{
    class PositionEntity        // n Seite
    {
        // Key 3. NF: Id von InvoiceEntity + ItemNr
        public int Id { get; set; } // Surrogate Key / Stellvertreter Key

        public int ItemNr { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public double ToPrice { get; set; }

        public int InvoiceId { get; set; }             // FK der Rechung
        public Invoice InvoiceEntity { get; set; }     // Referenz auf die Rechnung
    }
}
