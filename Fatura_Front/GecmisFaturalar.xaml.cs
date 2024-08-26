using Fatura_Front.Models;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace Fatura_Front
{
    /// <summary>
    /// GecmisFaturalar.xaml etkileşim mantığı
    /// </summary>
    public partial class GecmisFaturalar : System.Windows.Controls.UserControl
    {
        private readonly ApiService _apiService;
        private string _token = ConfigurationManager.AppSettings["Token"];
        private string _licenseid = ConfigurationManager.AppSettings["LicenseID"];
        private DispatcherTimer _timer;
        public GecmisFaturalar()
        {
            InitializeComponent();
            _apiService = new ApiService();
            InitializeTimer();
            LoadGecmisFaturalar();
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
            await LoadGecmisFaturalar();
        }

        private async Task LoadGecmisFaturalar()
        {
            try
            {
                List<Invoices> invoices = await _apiService.GetInvoicesAsync(_token, _licenseid);
                var unprintedInvoices = invoices.Where(invoice => invoice.printed).ToList();
                PrintedInvoicesListView.ItemsSource = unprintedInvoices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Geçmiş faturalar yüklenirken bir hata oluştu: {ex.Message}");
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

            PrintPdf(invoice.InvoiceID, invoice.InvoicePath, Yazıcılar.SelectedPrinter);
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
            }
            catch (Exception ex)
            {
                // Hata oluştuğunda yapılacak işlemler
                MessageBox.Show($"PDF yazdırma işleminde bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
