using Rechnungsverwaltung.Context;
using Rechnungsverwaltung.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

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

        public InvoiceViewModel()
        {
            var ctx1 = new InvoiceContext();
            lists = ctx1.Invoices.OrderBy(x => x.ID).ToList();
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

                        lists = ctx.Invoices.OrderBy(x => x.ID).ToList();
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
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
