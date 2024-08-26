using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;

namespace Fatura_Front
{
    public partial class MainWindow : Window
    {
        private readonly ConfigHelper _configHelper = new ConfigHelper();
        private readonly ApiService _apiService = new ApiService();
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            this.Width = 1000;
            this.Height = 500;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(55); 
            _timer.Tick += Timer_Tick; 
            _timer.Start(); 
        }

        private void MyNotifyIcon_TrayLeftMouseDown(object sender, 
            RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate(); 
        }

        private void Menu_Open_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            MyNotifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;  // Kapatma işlemini iptal eder
            this.Hide();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tokenGuncelle();
        }

        private async void tokenGuncelle()
        {
            string email = ConfigurationManager.AppSettings["Email"];
            string password = ConfigurationManager.AppSettings["Password"];
            string licenceid = ConfigurationManager.AppSettings["LicenseID"];
            try
            {
                var (newToken, newLicenseID) = await _apiService.GetTokenAsync(email, password);
                _configHelper.SaveTokenToConfig(newToken);
                _configHelper.SaveLicenseIDToConfig(newLicenseID);
                Console.WriteLine($"Token yenilendi: {newToken}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token yenileme hatası: {ex.Message}");
            }

           // MessageBox.Show("token yenilendi");

        }

       

    }
}