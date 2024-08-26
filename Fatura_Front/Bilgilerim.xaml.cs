using Fatura_Front.Models;
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

namespace Fatura_Front
{
    public partial class Bilgilerim : UserControl
    {

        private readonly ApiService _apiService;
        private readonly string _licenseid = ConfigurationManager.AppSettings["LicenseID"];
        public Bilgilerim()
        {
            InitializeComponent();
            _apiService = new ApiService();
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                List<UserInfo> userInfoList = await _apiService.GetCustomerInfoAsync(_licenseid);
                if (userInfoList.Any())
                {
                    BindUserInfo(userInfoList.First());
                }
                else
                {
                    MessageBox.Show("Kullanıcı bilgisi bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindUserInfo(UserInfo userInfo)
        {
            FirstNameTextBlock.Text = userInfo?.Name ?? "Bilgi Yok";
            LastNameTextBlock.Text = userInfo?.Surname ?? "Bilgi Yok";
            PhoneNumberTextBlock.Text = userInfo?.Phone ?? "Bilgi Yok";
            EmailTextBlock.Text = userInfo?.Email ?? "Bilgi Yok";
            LicenseIdTextBlock.Text = userInfo?.LicenseID ?? "Bilgi Yok";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper configHelper= new ConfigHelper();
            configHelper.ResetLicenseID();
            configHelper.ResetToken();
            configHelper.ResetEmailPassword();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Mevcut pencereyi kapatmak yerine, uygulamayı yeniden başlatın
            Application.Current.Shutdown();
        }

    }
}
