using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Configuration;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fatura_Front
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly ConfigHelper _configHelper;
        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _configHelper=new ConfigHelper();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Email ve şifre boş olamaz.");
                return;
            }

            try
            {
                var (token, licenseID) = await _apiService.GetTokenAsync(email, password);
                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(licenseID))
                {   
                    _configHelper.SaveLicenseIDToConfig(licenseID);
                    _configHelper.SaveEmailPasswordToConfig(email,password);
                    _configHelper.SaveTokenToConfig(token);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Geçersiz giriş bilgileri.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

    }
}
