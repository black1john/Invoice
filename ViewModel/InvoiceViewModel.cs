using Gma.QrCodeNet.Encoding;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using QRCoder;
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
        private Invoice rechnungen;
        public Invoice Rechnungen {
            get => rechnungen;
            set
            {
                rechnungen = value;
                RaisePropertyChanged(nameof(SeriesAmountInvoicePosition));
            }
        }
        public IList<Invoice> BubbleInvoice
        {
            get
            {
                using (InvoiceContext ctx = new InvoiceContext())
                {
                    return (from data in ctx.Invoices.Include("Position") select data).ToList();
                }
            }
        }
        public string ChosenName { get; set; }
        public string ChosenAddress { get; set; }
        public double ChosenAmount { get; set; }
        public DateTime ChosenDate { get; set; } = DateTime.Now;
        public int ChosenVat { get; set; }
        public ICommand InsertCommand { get; set; }
        public ICommand DeletCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        //Line Chart
        public SeriesCollection SeriesCollectionInvoiceAmounts {
            get
            {
                // DB Zugriff und in Serie konvertieren

                using (var context = new InvoiceContext())
                {
                    var invioces = context.Invoices.OrderBy(i => i.InvoiceDate);

                    var seriesCollection = new SeriesCollection();

                    var lineSeries = new LineSeries
                    {
                        Title = "Rechnungsverlauf",
                        Values = new ChartValues<DateTimePoint>(),
                    };

                    foreach (var invoice in invioces)
                    {
                        lineSeries.Values.Add(new DateTimePoint
                        {
                            DateTime = invoice.InvoiceDate,
                            Value = invoice.Amount,
                        });
                    }
                    seriesCollection.Add(lineSeries);
                    return seriesCollection;
                }
            }
        }
        public string[] LabelsInvoiceAmounts { get; set; }
        public Func<double, string> YFormatterInvoiceAmounts { get; set; }
        public Func<double, string> XFormatterInvoiceAmounts { get; set; }

        //Pie Chart
        public SeriesCollection SeriesAmountInvoicePosition
        {
            get
            {
                Func<ChartPoint, string> labelPiont = ChartPoint => $"{ChartPoint.Y} ({ChartPoint.Participation:P})";

                var seriesCollection = new SeriesCollection();

                if (Rechnungen != null)
                {
                    foreach (var position in Rechnungen.Position)
                    {
                        seriesCollection.Add(new PieSeries
                        {
                            Title = position.ItemNr.ToString(),
                            Values = new ChartValues<double> { position.Qty},
                            PushOut = position.Id == 1 ? 10 : 0,
                            DataLabels = true,
                            LabelPoint = labelPiont
                        });
                    }
                }

                return seriesCollection;
            }
        }

        //Bubble Chart
        public SeriesCollection SeriesCollectionInvoicePositionAmount
        {
            get
            {
                using (var context = new InvoiceContext())
                {
                    List<BubbleChart> bubbleChartData = new List<BubbleChart>();

                    foreach (var invoice in BubbleInvoice)
                    {
                        bubbleChartData.Add(new BubbleChart
                        {
                            InvoiceDate = invoice.InvoiceDate,
                            Amount = invoice.Amount,
                            PositionAmount = invoice.Position.Count
                        });
                    }

                    var seriesCollection = new SeriesCollection()
                    {
                        new ScatterSeries
                        {
                            Values = new ChartValues<BubbleChart>(bubbleChartData),
                            Configuration = Mappers.Weighted<BubbleChart>()
                            .X(i => i.InvoiceDate.Ticks)
                            .Y(i => i.Amount)
                            .Weight(i => i.PositionAmount)
                        }
                    };

                    return seriesCollection;
                }
            }
        }
        public Func<double, string> YFormatterInvoicePositionAmount { get; set; }
        public Func<double, string> XFormatterInvoicePositionAmount { get; set; }

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
                invoicePrintData.BarCode = CreateBarCode("BarCode");
                invoicePrintData.QrCode = CreateQrCode("QRCode");
                document.DataContext = invoicePrintData;

                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintDocument((document as IDocumentPaginatorSource).DocumentPaginator, "Invoice");
                }
            }, c => Rechnungen != null);

            YFormatterInvoiceAmounts = value => value.ToString("C");
            XFormatterInvoiceAmounts = value => new DateTime((long)value).ToString("dd.MM.yyyy");
            YFormatterInvoicePositionAmount = value => value.ToString("C");
            XFormatterInvoicePositionAmount = value => new DateTime((long)value).ToString("dd.MM.yyyy");
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

        private BitmapSource CreateQrCode(string toCode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(toCode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap result = qrCode.GetGraphic(20);

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         result.GetHbitmap(),
                         IntPtr.Zero,
                         System.Windows.Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
}
