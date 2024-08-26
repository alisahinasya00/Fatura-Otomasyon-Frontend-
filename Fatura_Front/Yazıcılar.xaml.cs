using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Drawing.Printing;
using Fatura_Front.Models;
using System.Configuration;
using System.Reflection;

namespace Fatura_Front
{
    /// <summary>
    /// Yazıcılar.xaml etkileşim mantığı
    /// </summary>
    public partial class Yazıcılar : UserControl
    {
        public static string SelectedPrinter { get; private set; }
        private readonly ApiService _apiService;
        private string _token = ConfigurationManager.AppSettings["Token"];
        private string _licenseid = ConfigurationManager.AppSettings["LicenseID"];

        public Yazıcılar()
        {
            InitializeComponent();
            _apiService = new ApiService(); // Initialize ApiService
            LoadPrintersAsync(); // Call async method
        }

        private async Task LoadPrintersAsync()
        {
           try
            {
                List<string> printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                PrinterComboBox.ItemsSource = printers;
                string savedPrinterName = await _apiService.GetPrinterNameAsync(_licenseid);

                if (printers.Contains(savedPrinterName))
                {
                    PrinterComboBox.SelectedItem = savedPrinterName;
                }
                else if (printers.Count > 0)
                {
                    // Eğer listede mevcut yazıcı yoksa, ilk yazıcıyı seçili yap
                    PrinterComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Printer load error: {ex.Message}");

            }
        }


        private async void PrinterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PrinterComboBox.SelectedItem != null)
            {
                SelectedPrinter = PrinterComboBox.SelectedItem.ToString();
                try
                {
                    await _apiService.UpdatePrinterNameAsync(_licenseid, SelectedPrinter);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Yazıcı güncelleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
