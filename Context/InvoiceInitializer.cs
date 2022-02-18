using Rechnungsverwaltung.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rechnungsverwaltung.Context
{
    class InvoiceInitializer : DropCreateDatabaseAlways<InvoiceContext>
    {
        protected override void Seed(InvoiceContext context)
        {
            IList<Invoice> defaults = new List<Invoice>();
            IList<PositionEntity> positions = new List<PositionEntity>();

            defaults.Add(new Invoice
            {
                CustomerName = "HTL",
                Amount = 250,
                CustomerAddress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 25),
                Vat = 20
            });

            defaults.Add(new Invoice
            {
                CustomerName = "HAK",
                Amount = 150,
                CustomerAddress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 15),
                Vat = 20
            });

            defaults.Add(new Invoice
            {
                CustomerName = "HTL",
                Amount = 200,
                CustomerAddress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 10),
                Vat = 20
            });

            defaults.Add(new Invoice
            {
                CustomerName = "HAK",
                Amount = 100,
                CustomerAddress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 05),
                Vat = 20
            });

            defaults.Add(new Invoice
            {
                CustomerName = "HTL",
                Amount = 250,
                CustomerAddress = "Ybbs",
                InvoiceDate = new DateTime(2020, 01, 01),
                Vat = 20
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 1,

                ItemNr = 1,
                Qty = 100,
                Price = 23.99,
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 2,

                ItemNr = 1,
                Qty = 50,
                Price = 19.99,
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 3,

                ItemNr = 1,
                Qty = 20,
                Price = 25,
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 1,

                ItemNr = 2,
                Qty = 23,
                Price = 24.99,
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 1,

                ItemNr = 3,
                Qty = 27,
                Price = 12.99,
            });

            positions.Add(new PositionEntity
            {
                InvoiceId = 5,

                ItemNr = 1,
                Qty = 13,
                Price = 10.99,
            });

            context.Invoices.AddRange(defaults);
            context.Positions.AddRange(positions);

            base.Seed(context);
        }
    }
}
