using Rechnungsverwaltung.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Rechnungsverwaltung.Printing
{
    class InvoicePrintData
    {
        public DateTime PrintingDate => DateTime.Now.Date;

        public Invoice Invoice { get; set; }

        public BitmapSource BarCode { get; set; }

        public BitmapSource QrCode { get; set; }

        //optional
        public IList<PositionEntity> Positions { get; set; }

        
    }
}
