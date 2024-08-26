using Fatura_Front.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing.Printing;
using System.IO;
using PdfiumViewer;


namespace Fatura_Front
{
    public partial class Faturalar: UserControl
    {
        private readonly ApiService _apiService;
        private string _token = ConfigurationManager.AppSettings["Token"];
        private string _licenseid = ConfigurationManager.AppSettings["LicenseID"];
        private string _email= ConfigurationManager.AppSettings["Email"];
        private string _password= ConfigurationManager.AppSettings["Password"];
        private DispatcherTimer _timer;

        public Faturalar()
        {
            InitializeComponent();
            _apiService = new ApiService();
            InitializeTimer();
            LoadFaturalar();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += Timer_Tick;
            if (Application.Current.MainWindow != null)
            {
                _timer.Start();
            }
        }
        private async void Timer_Tick(object sender, EventArgs e)
        {
            _token = ConfigurationManager.AppSettings["Token"];
            _licenseid = ConfigurationManager.AppSettings["LicenseID"];
            await LoadFaturalar();
        }

        private async Task LoadFaturalar()
        {
            try
            {
                List<Invoices> invoices = await _apiService.GetInvoicesAsync(_token, _licenseid);
                var printedInvoices=invoices.Where(invoice=>!invoice.printed).ToList();
                InvoicesListView.ItemsSource = printedInvoices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Faturalar yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var invoice = button?.DataContext as Invoices;

            if (invoice != null && !string.IsNullOrEmpty(invoice.InvoicePath))
            {
                try
                {
                    // PdfiumViewer kullanarak PDF aç
                    var pdfViewer = new PdfViewer();
                    pdfViewer.OpenPdf(invoice.InvoicePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF açılırken bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Geçerli bir dosya yolu bulunamadı.");
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var invoice = button?.DataContext as Invoices;

            if (invoice == null || string.IsNullOrEmpty(invoice.InvoicePath))
            {
                MessageBox.Show("Geçerli bir fatura seçilmedi veya dosya yolu bulunamadı.");
                return;
            }

            if (string.IsNullOrEmpty(Yazıcılar.SelectedPrinter))
            {
                MessageBox.Show("Lütfen bir yazıcı seçin.");
                return;
            }

            PrintPdf(invoice.InvoiceID,invoice.InvoicePath, Yazıcılar.SelectedPrinter);
        }

        private async Task PrintPdf(int Invoiceid, string pdfPath, string printerName)
        {
            try
            {
                using (var document = PdfDocument.Load(pdfPath))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.PrinterName = printerName; // Yazıcının adını buraya yazın
                        printDocument.Print();
                    }
                }

                try
                {
                    List<Invoices> invoices = await _apiService.UpdateInvoicesPrint(_token, Invoiceid);
                    InvoicesListView.ItemsSource = invoices;
                    LoadFaturalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Faturalar yüklenirken bir hata oluştu: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Hata oluştuğunda yapılacak işlemler
                MessageBox.Show($"PDF yazdırma işleminde bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
