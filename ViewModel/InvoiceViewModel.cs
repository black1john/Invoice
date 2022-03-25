using Rechnungsverwaltung.Context;
using Rechnungsverwaltung.Model;
using Rechnungsverwaltung.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Rechnungsverwaltung.ViewModel
{
    class InvoiceViewModel : INotifyPropertyChanged
    {
        private InvoiceList relist;
        public InvoiceList ReList
        {
            get => relist;
            set
            {
                relist = value;
                RaisePropertyChanged();
            }
        }
        List<Invoice> lists = new List<Invoice>();
        public Invoice Rechnungen { get; set; }
        public string ChosenName { get; set; }
        public string ChosenAddress { get; set; }
        public double ChosenAmount { get; set; }
        public DateTime ChosenDate { get; set; } = DateTime.Now;
        public int ChosenVat { get; set; }
        public ICommand InsertCommand { get; set; }
        public ICommand DeletCommand { get; set; }
        public ICommand PrintCommand { get; set; }

        public InvoiceViewModel()
        {
            var ctx1 = new InvoiceContext();
            lists = ctx1.Invoices.Include("Position").ToList();
            ReList = InvoiceList.ConvertList(lists);

            InsertCommand = new RelayCommand(e => 
            {
                Invoice invoice = new Invoice() 
                { 
                    CustomerName=ChosenName, 
                    CustomerAddress=ChosenAddress, 
                    Amount=ChosenAmount,
                    InvoiceDate=ChosenDate,
                    Vat=ChosenVat,
                };
                try
                {
                    using (var ctx = new InvoiceContext()) //für Zugriff auf Context notwendig
                    {
                        ctx.Invoices.Add(invoice); //Datensatz einfügen
                        ctx.SaveChanges(); //speicher / commit

                        lists = ctx.Invoices.Include("Position").OrderBy(x => x.ID).ToList();
                        ReList = InvoiceList.ConvertList(lists);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }, c => true);

            DeletCommand = new RelayCommand(e =>
            {
                DialogResult dialogResult = MessageBox.Show("Wollen Sie diesen Eintrag Löschen", "", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        using (var ctx = new InvoiceContext())
                        {
                            var user = ctx.Invoices.Find(Rechnungen.ID);
                            if (user != null)
                            {
                                ctx.Invoices.Remove(user);
                                ctx.SaveChanges();

                                lists = ctx.Invoices.OrderBy(x => x.ID).ToList();
                                ReList = InvoiceList.ConvertList(lists);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }, c => Rechnungen != null);

            PrintCommand = new RelayCommand(e =>
            {
                FlowDocument document = InvoiceViewModel.getFlowDocument("Printing/Invoice.xaml");

                var invoicePrintData = new InvoicePrintData();
                invoicePrintData.Invoice = Rechnungen;
                invoicePrintData.Positions = (IList<PositionEntity>)Rechnungen.Position;
                invoicePrintData.BarCode = CreateBarCode("123456789");
                document.DataContext = invoicePrintData;

                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintDocument((document as IDocumentPaginatorSource).DocumentPaginator, "Invoice");
                }
            }, c => Rechnungen != null);
        }

        private static FlowDocument getFlowDocument(String path)
        {
            String rawDocument = "";
            using (StreamReader streamReader = File.OpenText(path))
            {
                rawDocument = streamReader.ReadToEnd();
            }

            FlowDocument flowDocument = XamlReader.Load(new XmlTextReader(new StringReader(rawDocument))) as FlowDocument;
            return flowDocument;
        }

        private BitmapSource CreateBarCode(string toCode)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            System.Drawing.Image img = b.Encode(BarcodeLib.TYPE.CODE93, toCode, Color.Black, Color.White, 200, 50);

            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}
